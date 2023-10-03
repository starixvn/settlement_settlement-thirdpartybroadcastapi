namespace ThirdPartyBroadcast.Handlers;

using Services;

public class GroupTwoHandler :
        IRequestHandler<AutoPinGoalRequest, Result>,
        IRequestHandler<AutoVirtualBetRadarRequest, Result>,
        IRequestHandler<AutoVirtualSoccerRequest, Result>
{
    private readonly PinGoalService pinGoalService;

    private readonly VirtualBetRadarService virtualBetRadarService;

    private readonly VirtualSoccerService virtualSoccerService;

    public GroupTwoHandler(
        PinGoalService pinGoalService,
        VirtualBetRadarService virtualBetRadarService,
        VirtualSoccerService virtualSoccerService)
    {
        this.pinGoalService = pinGoalService;
        this.virtualBetRadarService = virtualBetRadarService;
        this.virtualSoccerService = virtualSoccerService;
    }

    public async Task<Result> Handle(AutoPinGoalRequest request, CancellationToken cancellationToken) => await pinGoalService.Settle(request);

    public async Task<Result> Handle(AutoVirtualBetRadarRequest request, CancellationToken cancellationToken) => await virtualBetRadarService.Settle(request);

    public async Task<Result> Handle(AutoVirtualSoccerRequest request, CancellationToken cancellationToken) => await virtualSoccerService.Settle(request);
}