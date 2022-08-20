using System.Threading.Tasks;
using ThirdPartyBroadcast.Models;
using ThirdPartyBroadcast.Requests;
using Fanex.Settlement.ScaleOut.Services;
using Fanex.Data.Repository;
using AutoMapper;
using Fanex.Settlement.ScaleOut.Request;
using ThirdPartyBroadcast.Constants;
using ThirdPartyBroadcast.Criteria.VirtualBetRadar;

namespace ThirdPartyBroadcast.Services
{
    public interface VirtualBetRadarService
    {
        Task<Result> Settle(AutoVirtualBetRadarRequest request);
    }

    public class VirtualBetRadarServiceImp : VirtualBetRadarService
    {
        private readonly ScaleOutSettleThirdPartyService scaleOutSettleThirdPartyService;

        private readonly IDynamicRepository repository;

        private readonly IMapper mapper;

        private readonly CalculateService calculateService;

        public VirtualBetRadarServiceImp(ScaleOutSettleThirdPartyService scaleOutSettleThirdPartyService,
            IDynamicRepository repository,
            IMapper mapper,
            CalculateService calculateService)
        {
            this.scaleOutSettleThirdPartyService = scaleOutSettleThirdPartyService;
            this.repository = repository;
            this.mapper = mapper;
            this.calculateService = calculateService;
        }

        public async Task<Result> Settle(AutoVirtualBetRadarRequest request)
        {
            var criteria = mapper.Map<AutoVirtualBetRadarRequest, VirtualBetRadarCriteria>(request);
            var criteriaValidation = new BetRadarValidationCriteria { MatchId = request.MatchId, SelectedResources = request.ResourceResult };
            var validationResult = await calculateService.ExecuteValidationMultiServer(criteriaValidation);
            if (validationResult.ErrorCode == 0)
            {
                await UpdateMatchInfo(request);
                return await calculateService.Excecute(criteria);
            }

            return validationResult;
        }

        private async Task UpdateMatchInfo(AutoVirtualBetRadarRequest request)
        {
            var criteria = mapper.Map<AutoVirtualBetRadarRequest, BetRadarUpdateMatchCriteria>(request);
            await repository.ExecuteAsync(criteria);
        }
    }
}