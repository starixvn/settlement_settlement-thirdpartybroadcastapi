using MediatR;
using System.Threading;
using System.Threading.Tasks;
using ThirdPartyBroadcast.Models;
using ThirdPartyBroadcast.Requests;
using ThirdPartyBroadcast.Services;

namespace ThirdPartyBroadcast.Handlers
{
    public class GroupOneHandler : 
        IRequestHandler<HappyFiveRequest, Result>,
        IRequestHandler<FastMarketRequest, Result>,
        IRequestHandler<OutrightRequest, Result>,
        IRequestHandler<BetBuilderRequest, Result>,
        IRequestHandler<NumberGameRequest, Result>
    {
        private readonly HappyFiveService happyFiveService;

        private readonly FastMarketService fastMarketService;

        private readonly OutrightService outrightService;

        private readonly BetBuilderService betBuilderService;

        private readonly NumberGameService numberGameService;

        public GroupOneHandler(
            HappyFiveService happyFiveService,
            FastMarketService fastMarketService,
            OutrightService outrightService,
            BetBuilderService betBuilderService,
            NumberGameService numberGameService)
        {
            this.happyFiveService = happyFiveService;
            this.fastMarketService = fastMarketService;
            this.outrightService = outrightService;
            this.betBuilderService = betBuilderService;
            this.numberGameService = numberGameService;
        }
        public async Task<Result> Handle(HappyFiveRequest request, CancellationToken cancellationToken) => await happyFiveService.Settle(request);

        public async Task<Result> Handle(FastMarketRequest request, CancellationToken cancellationToken) => await fastMarketService.Settle(request);

        public async Task<Result> Handle(OutrightRequest request, CancellationToken cancellationToken) => await outrightService.Settle(request);

        public async Task<Result> Handle(BetBuilderRequest request, CancellationToken cancellationToken) => await betBuilderService.Settle(request);

        public async Task<Result> Handle(NumberGameRequest request, CancellationToken cancellationToken) => await numberGameService.Settle(request);

    }
}
