using AutoMapper;
using Fanex.Data.Repository;
using Fanex.Settlement.ScaleOut.Services;
using System.Threading.Tasks;
using ThirdPartyBroadcast.Criteria.VirtualSoccer;
using ThirdPartyBroadcast.Models;
using ThirdPartyBroadcast.Requests;

namespace ThirdPartyBroadcast.Services
{
    public interface VirtualSoccerService
    {
        Task<Result> Settle(AutoVirtualSoccerRequest request);
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

        public async Task<Result> Settle(AutoVirtualSoccerRequest request)
        {
            var criteria = mapper.Map<AutoVirtualSoccerRequest, VirtualSoccerCriteria>(request);

            var validationResult = await ValidateBeforePush(request);
            if (validationResult.ErrorCode == 0)
            {
                await UpdateMatchInfo(request);
                return await calculateService.Excecute(criteria);
            }

            return validationResult;
        }

        private async Task<Result> ValidateBeforePush(AutoVirtualSoccerRequest request)
        {
            var validationEventCriteria = new VirtualSoccerValidationEventCriteria { MatchId = request.MatchId };
            var validateEvent = await ValidateEvent(validationEventCriteria);

            if (validateEvent.ErrorCode > 0)
            {
                return validateEvent;
            }

            var validationCriteria = new VirtualSoccerValidationCriteria { MatchId = request.MatchId };

            return await calculateService.ExecuteValidationMultiServer(validationCriteria);
        }

        private async Task<Result> ValidateEvent(VirtualSoccerValidationEventCriteria criteria)
        {
            await repository.ExecuteAsync(criteria);

            return new Result
            {
                ErrorCode = criteria.ErrCode,
                ErrorMessage = criteria.ErrMsg
            };
        }

        private async Task UpdateMatchInfo(AutoVirtualSoccerRequest request)
        {
            var criteria = mapper.Map<AutoVirtualSoccerRequest, VRSoccerUpdateMatchCriteria>(request);
            await repository.ExecuteAsync(criteria);
        }
    }
}