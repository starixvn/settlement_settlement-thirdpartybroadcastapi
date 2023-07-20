namespace ThirdPartyBroadcast.Services;

using Criteria.HappyFive;

public interface HappyFiveService
{
    Task<Result> Settle(AutoHappyFiveRequest request);
}

public class HappyFiveServiceImp : HappyFiveService
{
    private readonly IMapper mapper;

    private readonly CalculateService calculateService;

    private readonly IDynamicRepository repository;

    private readonly ScaleOutSettleThirdPartyService scaleOutSettleThirdPartyService;

    private readonly LogDirectlyService logDirectlyService;

    public HappyFiveServiceImp(
        IMapper mapper,
        CalculateService calculateService,
        IDynamicRepository repository,
        ScaleOutSettleThirdPartyService scaleOutSettleThirdPartyService,
        LogDirectlyService logDirectlyService)
    {
        this.mapper = mapper;
        this.calculateService = calculateService;
        this.repository = repository;
        this.scaleOutSettleThirdPartyService = scaleOutSettleThirdPartyService;
        this.logDirectlyService = logDirectlyService;
    }

    public async Task<Result> Settle(AutoHappyFiveRequest request)
    {
        var resultValidation = await ValidateBeforeSettle(request);

        if (resultValidation.ErrorCode == 0)
        {
            await UpdateMatchInfo(request);
            var criteria = mapper.Map<AutoHappyFiveRequest, HappyFiveCriteria>(request);

            await logDirectlyService.WriteLog(criteria.MatchId);

            return await calculateService.Execute(criteria);
        }

        return resultValidation;
    }

    private async Task<Result> ValidateBeforeSettle(AutoHappyFiveRequest request)
    {
        var validateDeployFlag = await calculateService.ValidationDeployFlag(SpName.HappyFive);
        if (validateDeployFlag.ErrorCode > 0)
        {
            return validateDeployFlag;
        }

        var criteriaValidation = new ValidateHappyFive { MatchId = request.MatchId, Action = request.Action };
        await repository.ExecuteAsync(criteriaValidation);
        return new Result
        {
            ErrorCode = criteriaValidation.ErrCode,
            ErrorMessage = criteriaValidation.ErrMsg
        };
    }

    private async Task UpdateMatchInfo(AutoHappyFiveRequest request)
    {
        var updateRequest = new UpdateMatchInfo3rdRequest
        {
            Command = EventStatus.Completed,
            MatchId = request.MatchId,
            Action = request.Action,
            Creator = request.AdminId
        };
        await scaleOutSettleThirdPartyService.UpdateMatchInfo3rd(updateRequest);
    }
}