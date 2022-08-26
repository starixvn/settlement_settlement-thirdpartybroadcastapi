using System.Threading.Tasks;
using AutoMapper;
using Fanex.Data.Repository;
using Fanex.Settlement.ScaleOut.Request;
using Fanex.Settlement.ScaleOut.Services;
using ThirdPartyBroadcast.Constants;
using ThirdPartyBroadcast.Criteria.HappyFive;
using ThirdPartyBroadcast.Criteria.VirtualBetRadar;
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
        private readonly IMapper mapper;

        private readonly CalculateService calculateService;

        private readonly IDynamicRepository repository;

        private readonly ScaleOutSettleThirdPartyService scaleOutSettleThirdPartyService;

        public HappyFiveServiceImp(
            IMapper mapper,
            CalculateService calculateService,
            IDynamicRepository repository,
            ScaleOutSettleThirdPartyService scaleOutSettleThirdPartyService)
        {
            this.mapper = mapper;
            this.calculateService = calculateService;
            this.repository = repository;
            this.scaleOutSettleThirdPartyService = scaleOutSettleThirdPartyService;
        }

        public async Task<Result> Settle(AutoHappyFiveRequest request)
        {
            var resultValidation = await ValidateBeforeSettle(request);

            if (resultValidation.ErrorCode == 0)
            {
                await UpdateMatchInfo(request);
                var criteria = mapper.Map<AutoHappyFiveRequest, HappyFiveCriteria>(request);
                return await calculateService.Excecute(criteria);
            }

            return resultValidation;
        }

        private async Task<Result> ValidateBeforeSettle(AutoHappyFiveRequest request)
        {
            var validateDeployFlag = await calculateService.ValidationDeloyFlag(SpName.HappyFive);
            if (validateDeployFlag.ErrorCode > 0)
            {
                return validateDeployFlag;
            }
            else
            {
                var criteriaValidation = new ValidateHappyFive { MatchId = request.MatchId, Action = request.Action };
                await repository.ExecuteAsync(criteriaValidation);
                return new Result
                {
                    ErrorCode = criteriaValidation.ErrCode,
                    ErrorMessage = criteriaValidation.ErrMsg
                };
            }
        }

        private async Task UpdateMatchInfo(AutoHappyFiveRequest request)
        {
            var updateRequest = new UpdateMatchInfo3rdRequest
            {
                Command = "completed",
                MatchId = request.MatchId,
                Action = request.Action,
                Creator = request.AdminId
            };
            await scaleOutSettleThirdPartyService.UpdateMatchInfo3rd(updateRequest);
        }
    }
}