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
        Task<Result> Settle(VirtualBetRadarRequest request);
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

        public async Task<Result> Settle(VirtualBetRadarRequest request)
        {
            var criteria = mapper.Map<VirtualBetRadarRequest, VirtualBetRadarCriteria>(request);

            var validationResult = await ValidateBeforePush(request);
            if (validationResult.ErrorCode == 0)
            {
                await UpdateMatchInfo(request);
                return await calculateService.Excecute(criteria);
            }

            return validationResult;
        }

        private async Task<Result> ValidateBeforePush(VirtualBetRadarRequest request)
        {
            var validationRequest = new EventTicketThirdPartyRequest
            {
                MatchId = request.MatchId,
                Action = request.Action,
                SpName = "NAP_Admin_Settlement_VS_BetRadar_Push",
                SelectedResources = request.ResourceResult,
                Mode = ModeType.VirtualBetradar,
                IsAuto = request.IsAuto
            };

            var validationResult = await scaleOutSettleThirdPartyService.ValidateEventAndTicket3rd(validationRequest);

            return new Result
            {
                ErrorCode = validationResult.ErrorCode,
                ErrorMessage = validationResult.ErrorMessage
            };
        }

        private async Task UpdateMatchInfo(VirtualBetRadarRequest request)
        {
            var criteria = mapper.Map<VirtualBetRadarRequest, BetRadarUpdateMatchCriteria>(request);
            await repository.ExecuteAsync(criteria);
        }
    }
}