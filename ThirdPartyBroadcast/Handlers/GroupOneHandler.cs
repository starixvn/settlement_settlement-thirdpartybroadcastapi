using MediatR;
using System.Threading;
using System.Threading.Tasks;
using ThirdPartyBroadcast.Models;
using ThirdPartyBroadcast.Requests;
using ThirdPartyBroadcast.Services;

namespace ThirdPartyBroadcast.Handlers
{
    public class GroupOneHandler : IRequestHandler<HappyFiveRequest, Result>,
        IRequestHandler<NumberGameRequest, Result>
    {
        private readonly HappyFiveService happyFiveService;

        private readonly NumberGameService numberGameService;

        public GroupOneHandler(
            HappyFiveService happyFiveService,
            NumberGameService numberGameService)
        {
            this.happyFiveService = happyFiveService;
            this.numberGameService = numberGameService;
        }
        public async Task<Result> Handle(HappyFiveRequest request, CancellationToken cancellationToken) => await happyFiveService.Settle(request);

        public async Task<Result> Handle(NumberGameRequest request, CancellationToken cancellationToken) => await numberGameService.Settle(request);
    }
}
