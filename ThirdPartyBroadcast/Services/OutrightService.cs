using AutoMapper;
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

        public OutrightServiceImp(
            IMapper mapper,
            CalculateService calculateService)
        {
            this.mapper = mapper;
            this.calculateService = calculateService;
        }

        public async Task<Result> Settle(AutoOutrightRequest request)
        {
            var validateDeployFlag = await calculateService.ValidationDeployFlag(SpName.OutRight);

            if (validateDeployFlag.ErrorCode == 0)
            {
                var criteria = mapper.Map<AutoOutrightRequest, OutRightCriteria>(request);
                return await calculateService.Execute(criteria);
            }

            return validateDeployFlag;
        }
    }
}