using AutoMapper;
using Fanex.Data.Repository;
using Fanex.Settlement.ScaleOut.Request;
using Fanex.Settlement.ScaleOut.Services;
using Kaden.XmlConverter;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Threading.Tasks;
using ThirdPartyBroadcast.Constants;
using ThirdPartyBroadcast.Criteria.BetBuilder;
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

        public async Task<Result> Settle(BetBuilderRequest request)
        {
            var validationResult = await ValidationBeforeSettle(request);
            if (validationResult.ErrorCode == 0)
            {
                await UpdateMatchInfo(request);
                var criteria = mapper.Map<BetBuilderRequest, BetBuilderCriteria>(request);
                criteria.SelectedMarket = request.Resources;
                return await calculateService.Excecute(criteria);
            }

            return validationResult;
        }

        private async Task UpdateMatchInfo(BetBuilderRequest request)
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

        private async Task<Result> ValidationBeforeSettle(BetBuilderRequest request)
        {
            var eventTicketThirdPartyRequest = new EventTicketThirdPartyRequest
            {
                MatchId = request.MatchId,
                Mode = ModeType.BetBuilder,
                Action = request.Action,
                IsAuto = request.IsAuto,
                SpName = "NAP_Admin_Settlement_BetBuilder_Push",
                VendorId = request.VendorId,
                SelectedResources = request.Resources
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