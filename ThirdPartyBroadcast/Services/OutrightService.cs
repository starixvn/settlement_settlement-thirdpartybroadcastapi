using AutoMapper;
using Fanex.Data.Repository;
using Fanex.Settlement.ScaleOut.Request;
using Fanex.Settlement.ScaleOut.Services;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using ThirdPartyBroadcast.Constants;
using ThirdPartyBroadcast.Criteria.OutRight;
using ThirdPartyBroadcast.Models;
using ThirdPartyBroadcast.Requests;

namespace ThirdPartyBroadcast.Services
{
    public interface OutrightService
    {
        Task<Result> Settle(OutrightRequest request);
    }

    public class OutrightServiceImp : OutrightService
    {
        private readonly IDynamicRepository repository;

        private readonly ScaleOutSettleThirdPartyService scaleOutSettleThirdPartyService;

        private readonly IMapper mapper;

        private readonly CalculateService calculateService;

        public OutrightServiceImp(
            IDynamicRepository repository,
            ScaleOutSettleThirdPartyService scaleOutSettleThirdPartyService,
            IMapper mapper,
            CalculateService calculateService)
        {
            this.repository = repository;
            this.scaleOutSettleThirdPartyService = scaleOutSettleThirdPartyService;
            this.mapper = mapper;
            this.calculateService = calculateService;
        }

        public async Task<Result> Settle(OutrightRequest request)
        {
            var resultValidation = await ValidationBeforeSettle(request);

            if (resultValidation.ErrorCode == 0)
            {
                await UpdateMatchInfo(request);
                var criteria = mapper.Map<OutrightRequest, OutRightCriteria>(request);
                return await calculateService.Excecute(criteria);
            }

            return resultValidation;
        }

        private async Task UpdateMatchInfo(OutrightRequest request)
        {
            if (request.Action == "UNDO")
            {
                var criteria = mapper.Map<OutrightRequest, OutRightUpdateMatchCriteria>(request);
                await repository.ExecuteAsync(criteria);
            }
        }

        private async Task<Result> ValidationBeforeSettle(OutrightRequest request)
        {
            var eventTicketThirdPartyRequest = new EventTicketThirdPartyRequest
            {
                MatchId = request.MatchId,
                Mode = ModeType.Outright,
                Action = request.Action,
                IsAuto = true,
                SpName = "NAP_Admin_Settlement_Outright"
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