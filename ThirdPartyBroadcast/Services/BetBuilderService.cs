using AutoMapper;
using Fanex.Settlement.ScaleOut.Request;
using Fanex.Settlement.ScaleOut.Services;
using System.Threading.Tasks;
using ThirdPartyBroadcast.Constants;
using ThirdPartyBroadcast.Criteria.BetBuilder;
using ThirdPartyBroadcast.Models;
using AutoBetBuilderRequest = ThirdPartyBroadcast.Requests.AutoBetBuilderRequest;

namespace ThirdPartyBroadcast.Services
{
    public interface BetBuilderService
    {
        Task<Result> Settle(AutoBetBuilderRequest request);
    }

    public class BetBuilderServiceImp : BetBuilderService
    {
        private readonly ScaleOutSettleThirdPartyService scaleOutSettleThirdPartyService;

        private readonly ScaleOutSettleService scaleOutSettleService;

        private readonly CalculateService calculateService;

        private readonly IMapper mapper;

        public BetBuilderServiceImp(ScaleOutSettleThirdPartyService scaleOutSettleThirdPartyService,
                ScaleOutSettleService scaleOutSettleService,
                CalculateService calculateService,
                IMapper mapper)
        {
            this.scaleOutSettleThirdPartyService = scaleOutSettleThirdPartyService;
            this.scaleOutSettleService = scaleOutSettleService;
            this.calculateService = calculateService;
            this.mapper = mapper;
        }

        public async Task<Result> Settle(AutoBetBuilderRequest request)
        {
            var validationResult = await ValidationBeforeSettle(request);
            if (validationResult.ErrorCode == 0)
            {
                await UpdateMatchInfo(request);
                var criteria = mapper.Map<AutoBetBuilderRequest, BetBuilderCriteria>(request);
                criteria.SelectedMarket = request.Resources;
                return await calculateService.Excecute(criteria);
            }

            return validationResult;
        }

        private async Task UpdateMatchInfo(AutoBetBuilderRequest request)
        {
            var requestUpdate = new UpdateMatchInfoRequest
            {
                Mode = ModeType.BetBuilder,
                IsAuto = request.IsAuto,
                Action = request.Action,
                SelectedMarket = request.Resources
            };
            await scaleOutSettleService.UpdateMatchInfo(requestUpdate);
        }

        private async Task<Result> ValidationBeforeSettle(AutoBetBuilderRequest request)
        {
            var eventTicketThirdPartyRequest = new BetBuilderRequest
            {
                MatchId = request.MatchId,
                Action = request.Action,
                IsAuto = request.IsAuto,
                SelectedResources = request.Resources,
                VendorID = request.VendorId
            };

            var (result, invalidMarketIds) = await scaleOutSettleThirdPartyService.ValidateBetBuilder(eventTicketThirdPartyRequest);

            return new Result
            {
                ErrorCode = result.ErrorCode,
                ErrorMessage = result.ErrorMessage
            };
        }
    }
}