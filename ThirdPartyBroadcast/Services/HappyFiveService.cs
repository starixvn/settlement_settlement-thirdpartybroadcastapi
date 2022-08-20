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
        Task<Result> Settle(AutoHappyFiveRequest request);
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

        public async Task<Result> Settle(AutoHappyFiveRequest request)
        {
            var resultValidation = await ValidationBeforeSettle(request);

            if (resultValidation.ErrorCode == 0)
            {
                await UpdateMatchInfo(request);
                var criteria = mapper.Map<AutoHappyFiveRequest, HappyFiveCriteria>(request);
                return await calculateService.Excecute(criteria);
            }

            return resultValidation;
        }

        private async Task UpdateMatchInfo(AutoHappyFiveRequest request)
        {
            var criteria = mapper.Map<AutoHappyFiveRequest, HappyFiveUpdateMatchCriteria>(request);
            await repository.ExecuteAsync(criteria);
        }

        private async Task<Result> ValidationBeforeSettle(AutoHappyFiveRequest request)
        {
            var eventTicketThirdPartyRequest = new EventTicketHappyFiveRequest
            {
                MatchId = request.MatchId,
                Action = request.Action,
                IsAuto = request.IsAuto,
            };

            var result = await scaleOutSettleThirdPartyService.ValidateEventAndTicketHappyFive(eventTicketThirdPartyRequest);

            return new Result
            {
                ErrorCode = result.ErrorCode,
                ErrorMessage = result.ErrorMessage
            };
        }
    }
}