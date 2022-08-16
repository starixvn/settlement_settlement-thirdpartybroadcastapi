using AutoMapper;
using Fanex.Data.Repository;
using Fanex.Settlement.ScaleOut.Request;
using Fanex.Settlement.ScaleOut.Services;
using System.Threading.Tasks;
using ThirdPartyBroadcast.Constants;
using ThirdPartyBroadcast.Criteria.VirtualBetRadar;
using ThirdPartyBroadcast.Criteria.VirtualSoccer;
using ThirdPartyBroadcast.Models;
using ThirdPartyBroadcast.Requests;

namespace ThirdPartyBroadcast.Services
{
    public interface VirtualSoccerService
    {
        Task<Result> Settle(VirtualSoccerRequest request);
    }

    public class VirtualSoccerServiceImp : VirtualSoccerService
    {
        private readonly ScaleOutSettleThirdPartyService scaleOutSettleThirdPartyService;

        private readonly IDynamicRepository repository;

        private readonly IMapper mapper;

        private readonly CalculateService calculateService;

        public VirtualSoccerServiceImp(ScaleOutSettleThirdPartyService scaleOutSettleThirdPartyService,
            IDynamicRepository repository,
            IMapper mapper,
            CalculateService calculateService)
        {
            this.scaleOutSettleThirdPartyService = scaleOutSettleThirdPartyService;
            this.repository = repository;
            this.mapper = mapper;
            this.calculateService = calculateService;
        }

        public async Task<Result> Settle(VirtualSoccerRequest request)
        {
            var criteria = mapper.Map<VirtualSoccerRequest, VirtualSoccerCriteria>(request);

            var validationResult = await ValidateBeforePush(request);
            if (validationResult.ErrorCode == 0)
            {
                await UpdateMatchInfo(request);
                return await calculateService.Excecute(criteria);
            }

            return validationResult;
        }

        private async Task<Result> ValidateBeforePush(VirtualSoccerRequest request)
        {
            var validationRequest = new EventTicketThirdPartyRequest
            {
                MatchId = request.MatchId,
                Action = request.Action,
                SpName = "NAP_Admin_Settlement_VS_180",
                Mode = ModeType.VirtualSoccer,
                IsAuto = request.IsAuto
            };

            var validationResult = await scaleOutSettleThirdPartyService.ValidateEventAndTicket3rd(validationRequest);

            return new Result
            {
                ErrorCode = validationResult.ErrorCode,
                ErrorMessage = validationResult.ErrorMessage
            };
        }

        private async Task UpdateMatchInfo(VirtualSoccerRequest request)
        {
            var criteria = mapper.Map<VirtualSoccerRequest, VRSoccerUpdateMatchCriteria>(request);
            await repository.ExecuteAsync(criteria);
        }
    }
}