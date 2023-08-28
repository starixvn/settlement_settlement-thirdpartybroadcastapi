namespace ThirdPartyBroadcast.Services;

public interface BetBuilderService
{
    Task<Result> Settle(AutoBetBuilderRequest request);
}

public class BetBuilderServiceImp : BetBuilderService
{
    private const int MaxLength = 115;

    private readonly ScaleOutSettleThirdPartyService scaleOutSettleThirdPartyService;

    private readonly ScaleOutSettleService scaleOutSettleService;

    private readonly CalculateService calculateService;

    private readonly IMapper mapper;

    private readonly XmlConvertService xmlConvertService;

    private readonly IDynamicRepository repository;

    private readonly LogDirectlyService logDirectlyService;

    public BetBuilderServiceImp(ScaleOutSettleThirdPartyService scaleOutSettleThirdPartyService,
            ScaleOutSettleService scaleOutSettleService,
            CalculateService calculateService,
            IMapper mapper,
            XmlConvertService xmlConvertService,
            IDynamicRepository repository,
            LogDirectlyService logDirectlyService)
    {
        this.scaleOutSettleThirdPartyService = scaleOutSettleThirdPartyService;
        this.scaleOutSettleService = scaleOutSettleService;
        this.calculateService = calculateService;
        this.mapper = mapper;
        this.xmlConvertService = xmlConvertService;
        this.repository = repository;
        this.logDirectlyService = logDirectlyService;
    }

    public async Task<Result> Settle(AutoBetBuilderRequest request)
    {
        var (validationResult, invalidMarketIds) = await ValidationBeforeSettle(request);
        if (validationResult.ErrorCode == 0)
        {
            var selectedMarket = RemoveInvalidMarket(invalidMarketIds, request.Resources);
            WriteLog(request, invalidMarketIds, validationResult);
            return string.IsNullOrEmpty(selectedMarket) ? validationResult : await UpdateAndSettle(request, selectedMarket, validationResult);
        }

        return validationResult;
    }

    private async Task<Result> UpdateAndSettle(AutoBetBuilderRequest request, string selectedMarket, Result validateResult)
    {
        await UpdateMatchInfo(request, selectedMarket);

        var criteria = mapper.Map<AutoBetBuilderRequest, BetBuilderCriteria>(request);
        criteria.SelectedMarket = selectedMarket;

        var message = GenerateLogMessage(criteria, request.Resources);
        await logDirectlyService.WriteLog(criteria.MatchId, message);

        var settleResult = await calculateService.Execute(criteria);

        return HandleError(validateResult, settleResult);
    }

    private static string GenerateLogMessage(BetBuilderCriteria criteria, string resources)
        => $"NAP: Calculate {criteria.Action} for matchId: {criteria.MatchId} {(!string.IsNullOrEmpty(resources) ? resources + "<br/>" : string.Empty)}; IsAuto:{criteria.IsAuto}; BetType:38";

    private static Result HandleError(Result validateResult, Result settleResult)
    {
        if (validateResult.ErrorCode > 0 && settleResult.ErrorCode > 0)
        {
            validateResult.ErrorMessage += $"| {settleResult.ErrorMessage}";
            return validateResult;
        }

        return validateResult.ErrorCode > 0 ? validateResult : settleResult;
    }

    private void WriteLog(AutoBetBuilderRequest request, string invalidMarketIds, Result result)
    {
        if (!string.IsNullOrEmpty(invalidMarketIds))
        {
            const int existInvalidMarketErrorCode = 1_000_007;
            const string logMessage = "MarketIDs cannot be auto settled since they have been settled already:";
            var marketIds = invalidMarketIds.Length > MaxLength
                ? $"{invalidMarketIds[..MaxLength]}..."
                : invalidMarketIds;

            var actionId = request.Action.ToLowerInvariant() == ActionType.Calculate
                ? ActionId.BetBuilderCalc
                : ActionId.BetBuilderRefund;

            var message = $"{logMessage} {marketIds}";

            var criteria = new WriteLogCriteria(request.MatchId, message, actionId, request.AdminId, request.Ip);
            repository.Execute(criteria);
            result.ErrorMessage = message;
            result.ErrorCode = existInvalidMarketErrorCode;
        }
    }

    private async Task UpdateMatchInfo(AutoBetBuilderRequest request, string selectedMarket)
    {
        var requestUpdate = new UpdateMatchInfoRequest
        {
            MatchId = request.MatchId,
            Mode = Mode.BetBuilder,
            IsAuto = request.IsAuto,
            Action = request.Action,
            SelectedMarket = selectedMarket
        };

        await scaleOutSettleService.UpdateMatchInfo(requestUpdate);
    }

    private string RemoveInvalidMarket(string invalidMarketIds, string selectionMarket)
    {
        var listMarket = xmlConvertService.Deserialize<ResourcesMarketModel>(selectionMarket);
        var listInvalidMarket = invalidMarketIds.Split(',').ToList();
        listInvalidMarket.ForEach(item =>
        {
            var index = listMarket.Items.FindIndex(i => i.MarketId == item);
            if (index > -1)
            {
                listMarket.Items.RemoveAt(index);
            }
        });

        return listMarket.Items.Count > 0 ? xmlConvertService.Serialize(listMarket) : string.Empty;
    }

    private async Task<(Result, string)> ValidationBeforeSettle(AutoBetBuilderRequest request)
    {
        var eventTicketThirdPartyRequest = new BetBuilderRequest
        {
            MatchId = request.MatchId,
            Action = request.Action,
            IsAuto = request.IsAuto,
            SelectedMarket = request.Resources,
            VendorID = request.VendorId
        };

        var (resultValidate, invalidMarketIds) = await scaleOutSettleThirdPartyService.ValidateBetBuilder(eventTicketThirdPartyRequest);

        var result = new Result
        {
            ErrorCode = resultValidate.ErrorCode,
            ErrorMessage = resultValidate.ErrorMessage
        };

        return (result, invalidMarketIds ?? string.Empty);
    }
}