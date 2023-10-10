namespace ThirdPartyBroadcast.Handlers;

using Services;

public class GroupTwoHandler :
        IRequestHandler<AutoPinGoalRequest, Result>
{
    private readonly PinGoalService pinGoalService;

    public GroupTwoHandler(
        PinGoalService pinGoalService)
    {
        this.pinGoalService = pinGoalService;
    }

    public async Task<Result> Handle(AutoPinGoalRequest request, CancellationToken cancellationToken) => await pinGoalService.Settle(request);
}