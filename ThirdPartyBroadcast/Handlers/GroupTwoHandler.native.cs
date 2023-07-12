namespace ThirdPartyBroadcast.Handlers;

using Services;

public class GroupTwoHandler :
        IRequestHandler<AutoBitCoinRequest, Result>,
        IRequestHandler<AutoPinGoalRequest, Result>,
        IRequestHandler<AutoVirtualBetRadarRequest, Result>,
        IRequestHandler<AutoVirtualSoccerRequest, Result>,
        IRequestHandler<LeapGamingRequest, Result>
{
    private readonly BitCoinService bitCoinService;

    private readonly PinGoalService pinGoalService;

    private readonly VirtualBetRadarService virtualBetRadarService;

    private readonly VirtualSoccerService virtualSoccerService;

    private readonly LeapGamingService leapGamingService;

    public GroupTwoHandler(
        BitCoinService bitCoinService,
        PinGoalService pinGoalService,
        VirtualBetRadarService virtualBetRadarService,
        VirtualSoccerService virtualSoccerService,
        LeapGamingService leapGamingService)
    {
        this.bitCoinService = bitCoinService;
        this.pinGoalService = pinGoalService;
        this.virtualBetRadarService = virtualBetRadarService;
        this.virtualSoccerService = virtualSoccerService;
        this.leapGamingService = leapGamingService;
    }

    public async Task<Result> Handle(AutoBitCoinRequest request, CancellationToken cancellationToken) => await bitCoinService.Settle(request);

    public async Task<Result> Handle(AutoPinGoalRequest request, CancellationToken cancellationToken) => await pinGoalService.Settle(request);

    public async Task<Result> Handle(AutoVirtualBetRadarRequest request, CancellationToken cancellationToken) => await virtualBetRadarService.Settle(request);

    public async Task<Result> Handle(AutoVirtualSoccerRequest request, CancellationToken cancellationToken) => await virtualSoccerService.Settle(request);

    public async Task<Result> Handle(LeapGamingRequest request, CancellationToken cancellationToken) => await leapGamingService.Settle(request);
}