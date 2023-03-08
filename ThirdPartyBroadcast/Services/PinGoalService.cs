namespace ThirdPartyBroadcast.Services;

public interface PinGoalService
{
    Task<Result> Settle(AutoPinGoalRequest request);
}

public class PinGoalServiceImp : PinGoalService
{
    private readonly ScaleOutSettleService scaleOutSettleService;

    private readonly IMapper mapper;

    private readonly CalculateService calculateService;

    public PinGoalServiceImp(ScaleOutSettleService scaleOutSettleService,
        IMapper mapper,
        CalculateService calculateService)
    {
        this.scaleOutSettleService = scaleOutSettleService;
        this.mapper = mapper;
        this.calculateService = calculateService;
    }

    public async Task<Result> Settle(AutoPinGoalRequest request)
    {
        var criteria = mapper.Map<AutoPinGoalRequest, PinGoalCriteria>(request);
        var validationResult = await ValidateBeforePush(request);
        if (validationResult.ErrorCode == 0)
        {
            await UpdateMatchInfo(request);
            return await calculateService.Execute(criteria);
        }

        return validationResult;
    }

    private async Task<Result> ValidateBeforePush(AutoPinGoalRequest request)
    {
        var validationRequest = new EventTicketRequest
        {
            MatchId = request.MatchId,
            Action = request.Action,
            SpName = "NAP_Admin_Settlement_Profile_Push",
            SelectedResources = request.SelectedResources,
            Mode = Mode.PinGoal,
            IsAuto = request.IsAuto
        };

        var validationResult = await scaleOutSettleService.ValidateEventAndTicket(validationRequest);

        return new Result
        {
            ErrorCode = validationResult.ErrorCode,
            ErrorMessage = validationResult.ErrorMessage
        };
    }

    private async Task UpdateMatchInfo(AutoPinGoalRequest request)
    {
        var requestUpdate = mapper.Map<AutoPinGoalRequest, UpdateMatchInfoRequest>(request);
        await scaleOutSettleService.UpdateMatchInfo(requestUpdate);
    }
}