using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Threading.Tasks;
using AutoMapper;
using Fanex.Data.Repository;
using Fanex.Settlement.ScaleOut.Request;
using Fanex.Settlement.ScaleOut.Services;
using Kaden.XmlConverter;
using Microsoft.Extensions.Options;
using ThirdPartyBroadcast.Constants;
using ThirdPartyBroadcast.Criteria.HappyFive;
using ThirdPartyBroadcast.Models;
using ThirdPartyBroadcast.Requests;

namespace ThirdPartyBroadcast.Services
{
    public interface HappyFiveService
    {
        Task<Result> Settle(HappyFiveRequest request);
    }

    public class HappyFiveServiceImp : HappyFiveService
    {
        private readonly IDynamicRepository repository;

        private readonly ScaleOutSettleThirdPartyService scaleOutSettleThirdPartyService;

        private readonly IMapper mapper;

        private readonly CalculateService calculateService;

        public HappyFiveServiceImp(
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

        public async Task<Result> Settle(HappyFiveRequest request)
        {
            var resultValidation = await ValidationBeforeSettle(request);

            if (resultValidation.ErrorCode == 0)
            {
                await UpdateMatchInfo(request);
                var criteria = mapper.Map<HappyFiveRequest, HappyFiveCriteria>(request);
                return await calculateService.Excecute(criteria);
            }

            return resultValidation;
        }

        private async Task UpdateMatchInfo(HappyFiveRequest request)
        {
            var criteria = mapper.Map<HappyFiveRequest, HappyFiveUpdateMatchCriteria>(request);
            await repository.ExecuteAsync(criteria);
        }

        private async Task<Result> ValidationBeforeSettle(HappyFiveRequest request)
        {
            var eventTicketThirdPartyRequest = new EventTicketThirdPartyRequest
            {
                MatchId = request.MatchId,
                Mode = ModeType.HappyFive,
                Action = request.Action,
                IsAuto = request.IsAuto,
                SpName = "NAP_Admin_Settlement_s164"
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