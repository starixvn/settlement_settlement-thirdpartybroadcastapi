namespace ThirdPartyBroadcast.Services;

using Criteria.VirtualBetRadar;

public interface VirtualBetRadarService
{
    Task<Result> Settle(AutoVirtualBetRadarRequest request);
}

public class VirtualBetRadarServiceImp : VirtualBetRadarService
{
    private readonly IDynamicRepository repository;

    private readonly IMapper mapper;

    private readonly CalculateService calculateService;

    private readonly LogDirectlyService logDirectlyService;

    public VirtualBetRadarServiceImp(
        IDynamicRepository repository,
        IMapper mapper,
        CalculateService calculateService,
        LogDirectlyService logDirectlyService)
    {
        this.repository = repository;
        this.mapper = mapper;
        this.calculateService = calculateService;
        this.logDirectlyService = logDirectlyService;
    }

    public async Task<Result> Settle(AutoVirtualBetRadarRequest request)
    {
        var criteria = mapper.Map<AutoVirtualBetRadarRequest, VirtualBetRadarCriteria>(request);
        var validation = await ValidateBeforeSettle(request);
        criteria.NotifyFlag = await ValidateMatchStatus(request);
        if (validation.ErrorCode > 0)
        {
            return validation;
        }

        await UpdateMatchInfo(request);

        var message = GenerateMessageLog(criteria);
        logDirectlyService.WriteLog(criteria.MatchId, message);

        var settleResult = await calculateService.Execute(criteria);

        HandleAfterSettle(request, settleResult);
        return settleResult;
    }

    private static string GenerateMessageLog(VirtualBetRadarCriteria criteria)
        => $@"NAP: VS BetRadar - Calculate {criteria.Action} for matchId: {criteria.MatchId}
              Scores: FT=[{criteria.Fth}-{criteria.Fta}]; HT=[{criteria.Hth}-{criteria.Hta}]
              Resource Results: {criteria.ResourceResult}";

    private void HandleAfterSettle(AutoVirtualBetRadarRequest request, Result settleResult)
    {
        const string successfullyMessage = "NAP: VS BetRadar - API inserted into single queue successfully.";
        var actionId = request.Action.ToLowerInvariant().Equals(ActionType.Calculate) ? ActionId.VirtualBetRadarCalc : ActionId.VirtualBetRadarRefund;
        var criteria = new WriteLogCriteria(request.MatchId, successfullyMessage, actionId, request.AdminId, request.Ip)
        {
            FinalAwayScore = request.Fta,
            FinalHomeScore = request.Fth,
            HtAwayScore = request.Hta,
            HtHomeScore = request.Hth
        };

        if (settleResult.ErrorCode == 0)
        {
            repository.Execute(criteria);
        }
        else
        {
            WriteLogFailed(criteria, settleResult.ErrorMessage);
        }
    }

    private void WriteLogFailed(WriteLogCriteria criteria, string errorMessage)
    {
        const string failedMessage = "NAP: VS BetRadar - Error: API inserted into single queue failed.";
        criteria.Remark = $"{failedMessage} {errorMessage}";
        repository.Execute(criteria);

        var sendMailCriteria = mapper.Map<WriteLogCriteria, SendMailCriteria>(criteria);
        sendMailCriteria.Message = errorMessage;
        repository.Execute(sendMailCriteria);
    }

    private async Task<Result> ValidateBeforeSettle(AutoVirtualBetRadarRequest request)
    {
        var validateDeployFlag = await calculateService.ValidationDeployFlag(SpName.VirtualBetRadar);
        if (validateDeployFlag.ErrorCode > 0)
        {
            return validateDeployFlag;
        }

        var criteriaValidation = mapper.Map<AutoVirtualBetRadarRequest, BetRadarValidationCriteria>(request);
        return await calculateService.ExecuteValidationMultiServer(criteriaValidation);
    }

    private async Task UpdateMatchInfo(AutoVirtualBetRadarRequest request)
    {
        var criteria = mapper.Map<AutoVirtualBetRadarRequest, BetRadarUpdateMatchCriteria>(request);
        await repository.ExecuteAsync(criteria);
    }

    private async Task<int> ValidateMatchStatus(AutoVirtualBetRadarRequest request)
    {
        const int notifyRefund = 10;
        const int notifySingleValue = 2;
        if (!request.Action.ToLowerInvariant().Equals(ActionType.Calculate))
        {
            return notifyRefund;
        }

        var criteria = new GetMatchStatusCriteria { MatchId = request.MatchId };
        var matchStatus = await repository.GetAsync<VsBetRadarMatchStatus>(criteria);
        var hasNotify = matchStatus != null && (matchStatus.Calculate || matchStatus.CalRefund);
        return hasNotify ? notifySingleValue + ParlayNotify(matchStatus) : 0;
    }

    private static int ParlayNotify(VsBetRadarMatchStatus matchStatus)
    {
        const int notifyParlayValue = 8;
        return matchStatus.Multiple || matchStatus.Refund ? notifyParlayValue : 0;
    }
}