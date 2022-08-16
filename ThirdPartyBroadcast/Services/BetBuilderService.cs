using Fanex.Settlement.ScaleOut.Request;
using Fanex.Settlement.ScaleOut.Services;
using System.Threading.Tasks;
using ThirdPartyBroadcast.Constants;
using ThirdPartyBroadcast.Models;
using ThirdPartyBroadcast.Requests;

namespace ThirdPartyBroadcast.Services
{
    public interface BetBuilderService
    {
        Task<Result> Settle(BetBuilderRequest request);
    }

    public class BetBuilderServiceImp : BetBuilderService
    {
        private readonly ScaleOutSettleThirdPartyService scaleOutSettleThirdPartyService;

        public BetBuilderServiceImp(ScaleOutSettleThirdPartyService scaleOutSettleThirdPartyService)
        {
            this.scaleOutSettleThirdPartyService = scaleOutSettleThirdPartyService;
        }

        public async Task<Result> Settle(BetBuilderRequest request)
        {
            return new Result();
        }

        private async Task<Result> ValidationBeforeSettle(BetBuilderRequest request)
        {
            var eventTicketThirdPartyRequest = new EventTicketThirdPartyRequest
            {
                MatchId = request.MatchId,
                Mode = ModeType.BetBuilder,
                Action = request.Action,
                IsAuto = request.IsAuto,
                SpName = "NAP_Admin_Settlement_BetBuilder_Push",
                VendorId = request.VendorId
            };

            var result = await scaleOutSettleThirdPartyService.ValidateEventAndTicket3rd(eventTicketThirdPartyRequest);

            return new Result
            {
                ErrorCode = result.ErrorCode,
                ErrorMessage = result.ErrorMessage
            };
        }
    }
}