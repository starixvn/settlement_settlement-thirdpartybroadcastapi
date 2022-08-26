using System.Threading.Tasks;
using AutoMapper;
using Fanex.Data.Repository;
using Fanex.Settlement.ScaleOut.Request;
using Fanex.Settlement.ScaleOut.Services;
using Microsoft.Extensions.Options;
using ThirdPartyBroadcast.Constants;
using ThirdPartyBroadcast.Criteria;
using ThirdPartyBroadcast.Models;
using ThirdPartyBroadcast.Requests;

namespace ThirdPartyBroadcast.Services
{
    public interface NumberGameService
    {
        Task<Result> Settle(AutoNumberGameRequest request);
    }

    public class NumberGameServiceImp : NumberGameService
    {
        private readonly IDynamicRepository repository;

        private readonly AppSettingsKey appSettingsKey;

        private readonly IMapper mapper;

        private readonly ScaleOutSettleThirdPartyService scaleOutSettleThirdPartyService;

        private readonly CalculateService calculateService;

        public NumberGameServiceImp(
            IDynamicRepository repository,
            IOptions<AppSettingsKey> appSettingsKey,
            IMapper mapper,
            ScaleOutSettleThirdPartyService scaleOutSettleThirdPartyService,
            CalculateService calculateService)
        {
            this.repository = repository;
            this.appSettingsKey = appSettingsKey.Value;
            this.mapper = mapper;
            this.scaleOutSettleThirdPartyService = scaleOutSettleThirdPartyService;
            this.calculateService = calculateService;
        }

        public async Task<Result> Settle(AutoNumberGameRequest request)
        {
            var resultValidation = await calculateService.ValidationDeloyFlag(SpName.NumberGame);

            if (resultValidation.ErrorCode == 0)
            {
                await UpdateMatchInfo(request);
                var criteria = mapper.Map<AutoNumberGameRequest, NumberGameCriteria>(request);
                return await calculateService.Excecute(criteria);
            }

            return resultValidation;
        }

        private async Task UpdateMatchInfo(AutoNumberGameRequest request)
        {
            if (!request.IsLive)
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
}