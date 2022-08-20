using System.Threading.Tasks;
using AutoMapper;
using Fanex.Data.Repository;
using Fanex.Settlement.ScaleOut.Request;
using Fanex.Settlement.ScaleOut.Services;
using Microsoft.Extensions.Options;
using ThirdPartyBroadcast.Constants;
using ThirdPartyBroadcast.Criteria;
using ThirdPartyBroadcast.Criteria.NumberGame;
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
            var resultValidation = await ValidationBeforeSettle(request);

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
                var criteria = mapper.Map<AutoNumberGameRequest, NumberGameUpdateMatchCriteria>(request);
                await repository.ExecuteAsync(criteria);
            }
        }

        private async Task<Result> ValidationBeforeSettle(AutoNumberGameRequest request)
        {
            var eventTicketThirdPartyRequest = new NumberGameRequest
            {
                MatchId = request.MatchId,
                IsAuto = request.IsAuto,
            };

            var result = await scaleOutSettleThirdPartyService.ValidateNumberGame(eventTicketThirdPartyRequest);

            return new Result
            {
                ErrorCode = result.ErrorCode,
                ErrorMessage = result.ErrorMessage
            };
        }
    }
}