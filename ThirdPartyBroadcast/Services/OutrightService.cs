using AutoMapper;
using Fanex.Data.Repository;
using Fanex.Settlement.ScaleOut.Request;
using Fanex.Settlement.ScaleOut.Services;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using ThirdPartyBroadcast.Constants;
using ThirdPartyBroadcast.Criteria.OutRight;
using ThirdPartyBroadcast.Criteria.VirtualBetRadar;
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
            var validateDeployFlag = await calculateService.ValidationDeloyFlag(SpName.OutRight);

            if (validateDeployFlag.ErrorCode == 0)
            {
                var criteria = mapper.Map<AutoOutrightRequest, OutRightCriteria>(request);
                return await calculateService.Excecute(criteria);
            }

            return validateDeployFlag;
        }
    }
}