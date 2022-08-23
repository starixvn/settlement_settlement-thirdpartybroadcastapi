using AutoMapper;
using Fanex.Data.Repository;
using Fanex.Settlement.ScaleOut.Request;
using Fanex.Settlement.ScaleOut.Services;
using System.Threading.Tasks;
using ThirdPartyBroadcast.Constants;
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

        private const string CalculateAction = "CAL";

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

            var validationResult = await calculateService.ValidationDeloyFlag(SpName.VirtualSports);
            if (validationResult.ErrorCode == 0)
            {
                await UpdateMatchInfo(request);
                return await calculateService.Excecute(criteria);
            }

            return validationResult;
        }

        private async Task UpdateMatchInfo(AutoVirtualSoccerRequest request)
        {
            var updateInfoRequest = mapper.Map<AutoVirtualSoccerRequest, UpdateMatchInfo3rdRequest>(request);
            var criteria = mapper.Map<AutoVirtualSoccerRequest, VRSoccerUpdateMatchCriteria>(request);
            updateInfoRequest.Command = "completed";
            criteria.Command = "F";
            if (request.Action.ToLower() == ActionType.Calculate)
            {
                updateInfoRequest.FTAway = request.Fta;
                updateInfoRequest.FTHome = request.Fth;
                criteria.Command = "C";
            }
            await scaleOutSettleThirdPartyService.UpdateMatchInfo3rd(updateInfoRequest);
            await repository.ExecuteAsync(criteria);
        }
    }
}