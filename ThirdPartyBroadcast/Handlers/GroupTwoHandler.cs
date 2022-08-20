using MediatR;
using System.Threading;
using System.Threading.Tasks;
using ThirdPartyBroadcast.Models;
using ThirdPartyBroadcast.Requests;
using ThirdPartyBroadcast.Services;

namespace ThirdPartyBroadcast.Handlers
{
    public class GroupTwoHandler : 
        IRequestHandler<AutoBitCoinRequest, Result>,
        IRequestHandler<AutoPinGoalRequest, Result>,
        IRequestHandler<AutoVirtualBetRadarRequest, Result>,
        IRequestHandler<AutoVirtualSoccerRequest, Result>
    {
        private readonly BitCoinService bitCoinService;

        private readonly PinGoalService pinGoalService;

        private readonly VirtualBetRadarService virtualBetRadarService;

        private readonly VirtualSoccerService virtualSoccerService;

        public GroupTwoHandler(
            BitCoinService bitCoinService,
            PinGoalService pinGoalService,
            VirtualBetRadarService virtualBetRadarService,
            VirtualSoccerService virtualSoccerService)
        {
            this.bitCoinService = bitCoinService;
            this.pinGoalService = pinGoalService;
            this.virtualBetRadarService = virtualBetRadarService;
            this.virtualSoccerService = virtualSoccerService;
        }
        public async Task<Result> Handle(AutoBitCoinRequest request, CancellationToken cancellationToken) => await bitCoinService.Settle(request);

        public async Task<Result> Handle(AutoPinGoalRequest request, CancellationToken cancellationToken) => await pinGoalService.Settle(request);

        public async Task<Result> Handle(AutoVirtualBetRadarRequest request, CancellationToken cancellationToken) => await virtualBetRadarService.Settle(request);

        public async Task<Result> Handle(AutoVirtualSoccerRequest request, CancellationToken cancellationToken) => await virtualSoccerService.Settle(request);
    }
}
