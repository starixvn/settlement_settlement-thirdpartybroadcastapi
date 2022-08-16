using AutoMapper;
using Fanex.Settlement.ScaleOut.Request;
using Fanex.Settlement.ScaleOut.Services;
using System.Threading.Tasks;
using ThirdPartyBroadcast.Constants;
using ThirdPartyBroadcast.Criteria;
using ThirdPartyBroadcast.Models;
using ThirdPartyBroadcast.Requests;

namespace ThirdPartyBroadcast.Services
{
    public interface PinGoalService
    {
        Task<Result> Settle(PinGoalRequest request);
    }

    public class PinGoalServiceImp : PinGoalService
    {
        private readonly ScaleOutSettleService scaleOutSettleService;

        private readonly IMapper mapper;

        private readonly CalculateService calculateService;

        public PinGoalServiceImp(ScaleOutSettleService scaleOutSettleService,
            IMapper mapper,
            CalculateService calculateService)
        {
            this.scaleOutSettleService = scaleOutSettleService;
            this.mapper = mapper;
            this.calculateService = calculateService;
        }

        public async Task<Result> Settle(PinGoalRequest request)
        {
            var criteria = mapper.Map<PinGoalRequest, PinGoalCriteria>(request);
            var validationResult = await ValidateBeforePush(request);
            if (validationResult.ErrorCode == 0)
            {
                await UpdateMatchInfo(request);
                return await calculateService.Excecute(criteria);
            }

            return validationResult;
        }

        private async Task<Result> ValidateBeforePush(PinGoalRequest request)
        {
            var validationRequest = new EventTicketRequest
            {
                MatchId = request.MatchId,
                Action = request.Action,
                SpName = "NAP_Admin_Settlement_Profile_Push",
                SelectedResources = request.SelectedResources,
                Mode = ModeType.Pingoal,
                IsAuto = request.IsAuto
            };

            var validationResult = await scaleOutSettleService.ValidateEventAndTicket(validationRequest);

            return new Result
            {
                ErrorCode = validationResult.ErrorCode,
                ErrorMessage = validationResult.ErrorMessage
            };
        }

        private async Task UpdateMatchInfo(PinGoalRequest request)
        {
            var requestUpdate = mapper.Map<PinGoalRequest, UpdateMatchInfoRequest>(request);
            await scaleOutSettleService.UpdateMatchInfo(requestUpdate);
        }
    }
}