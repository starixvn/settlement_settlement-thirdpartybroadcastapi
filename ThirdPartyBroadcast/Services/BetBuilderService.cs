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
using ThirdPartyBroadcast.Criteria.BitCoin;
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

        private readonly IDynamicRepository repository;

        private readonly XmlConvertService xmlConvertService;

        public BetBuilderServiceImp(ScaleOutSettleThirdPartyService scaleOutSettleThirdPartyService,
                ScaleOutSettleService scaleOutSettleService,
                CalculateService calculateService,
                IMapper mapper,
                IDynamicRepository repository,
                XmlConvertService xmlConvertService)
        {
            this.scaleOutSettleThirdPartyService = scaleOutSettleThirdPartyService;
            this.scaleOutSettleService = scaleOutSettleService;
            this.calculateService = calculateService;
            this.mapper = mapper;
            this.repository = repository;
            this.xmlConvertService = xmlConvertService;
        }

        public async Task<Result> Settle(BetBuilderRequest request)
        {
            var validationResult = await ValidationBeforeSettle(request);
            if (validationResult.ErrorCode == 0)
            {
                var selectedMarket = await UpdateAndGetMarket(request);
                await UpdateMatchInfo(request, selectedMarket);
                var criteria = mapper.Map<BetBuilderRequest, BetBuilderCriteria>(request);
                criteria.SelectedMarket = selectedMarket;
                return await calculateService.Excecute(criteria);
            }

            return validationResult;
        }

        private async Task<string> UpdateAndGetMarket(BetBuilderRequest betBuilderRequest)
        {
            var criteria = mapper.Map<BetBuilderRequest, UpdateMarketCriteria>(betBuilderRequest);
            var listMarketId = (await repository.FetchAsync<int>(criteria)).ToList();
            var listItem = new List<ResourcesItem>();
            listMarketId.ForEach(item => listItem.Add(new ResourcesItem { MarketId = item }));
            return listItem.Count > 0
            ? xmlConvertService.Serialize(new Resources { ResourceItems = listItem })
              : string.Empty;
        }

        private async Task UpdateMatchInfo(BetBuilderRequest request, string selectedMarket)
        {
            var requestUpdate = new UpdateMatchInfoRequest
            {
                Mode = ModeType.BetBuilder,
                IsAuto = request.IsAuto,
                Action = request.Action,
                SelectedSelection = request.Resources,
                SelectedMarket = selectedMarket
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