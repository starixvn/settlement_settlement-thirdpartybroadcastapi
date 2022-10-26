using AutoMapper;
using Fanex.Settlement.ScaleOut.Services;
using System.Threading.Tasks;
using ThirdPartyBroadcast.Constants;
using ThirdPartyBroadcast.Criteria.OutRight;
using ThirdPartyBroadcast.Models;
using ThirdPartyBroadcast.Requests;

namespace ThirdPartyBroadcast.Services
{
    public interface OutrightService
    {
        Task<Result> Settle(AutoOutrightRequest request);
    }

    public class OutrightServiceImp : OutrightService
    {
        private readonly IMapper mapper;

        private readonly CalculateService calculateService;

        private readonly ScaleOutSettleThirdPartyService scaleOutSettleThirdPartyService;

        public OutrightServiceImp(
            IMapper mapper,
            CalculateService calculateService,
            ScaleOutSettleThirdPartyService scaleOutSettleThirdPartyService)
        {
            this.mapper = mapper;
            this.calculateService = calculateService;
            this.scaleOutSettleThirdPartyService = scaleOutSettleThirdPartyService;
        }

        public async Task<Result> Settle(AutoOutrightRequest request)
        {
            var validation = await ValidateBeforeSettle(request);

            if (validation.ErrorCode == 0)
            {
                var criteria = mapper.Map<AutoOutrightRequest, OutRightCriteria>(request);
                return await calculateService.Execute(criteria);
            }

            return validation;
        }

        private async Task<Result> ValidateBeforeSettle(AutoOutrightRequest request)
        {
            var validateDeployFlag = await calculateService.ValidationDeployFlag(SpName.OutRight);
            if (validateDeployFlag.ErrorCode > 0)
            {
                return validateDeployFlag;
            }
            else
            {
                const string eventTable = "outright";
                var criteriaValidation =
                    await scaleOutSettleThirdPartyService.ValidateMoveBetDay(request.MatchId.ToString(), eventTable);
                return new Result
                {
                    ErrorCode = criteriaValidation.ErrorCode,
                    ErrorMessage = criteriaValidation.ErrorMessage
                };
            }
        }
    }
}