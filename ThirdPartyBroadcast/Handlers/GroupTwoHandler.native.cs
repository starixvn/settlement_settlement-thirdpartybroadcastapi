namespace ThirdPartyBroadcast.Handlers;

using Services;

public class GroupTwoHandler :
        IRequestHandler<AutoPinGoalRequest, Result>,
        IRequestHandler<AutoVirtualSoccerRequest, Result>
{
    private readonly PinGoalService pinGoalService;
    private readonly VirtualSoccerService virtualSoccerService;

    public GroupTwoHandler(
        PinGoalService pinGoalService,
        VirtualSoccerService virtualSoccerService)
    {
        this.pinGoalService = pinGoalService;
        this.virtualSoccerService = virtualSoccerService;
    }

    public async Task<Result> Handle(AutoPinGoalRequest request, CancellationToken cancellationToken) => await pinGoalService.Settle(request);

    public async Task<Result> Handle(AutoVirtualSoccerRequest request, CancellationToken cancellationToken) => await virtualSoccerService.Settle(request);
}