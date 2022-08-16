using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Fanex.Data.Repository;
using Fanex.Settlement.ScaleOut.Request;
using Fanex.Settlement.ScaleOut.Services;
using Microsoft.Extensions.Options;
using ThirdPartyBroadcast.Constants;
using ThirdPartyBroadcast.Criteria;
using ThirdPartyBroadcast.Criteria.HappyFive;
using ThirdPartyBroadcast.Criteria.NumberGame;
using ThirdPartyBroadcast.Criteria.OutRight;
using ThirdPartyBroadcast.Models;
using ThirdPartyBroadcast.Requests;

namespace ThirdPartyBroadcast.Services
{
    public interface NumberGameService
    {
        Task<Result> Settle(NumberGameRequest request);
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

        public async Task<Result> Settle(NumberGameRequest request)
        {
            var resultValidation = await ValidationBeforeSettle(request);

            if (resultValidation.ErrorCode == 0)
            {
                await UpdateMatchInfo(request);
                var criteria = mapper.Map<NumberGameRequest, NumberGameCriteria>(request);
                return await calculateService.Excecute(criteria);
            }

            return resultValidation;
        }

        private async Task UpdateMatchInfo(NumberGameRequest request)
        {
            var criteria = mapper.Map<NumberGameRequest, NumberGameUpdateMatchCriteria>(request);
            await repository.ExecuteAsync(criteria);
        }

        private async Task<Result> ValidationBeforeSettle(NumberGameRequest request)
        {
            var eventTicketThirdPartyRequest = new EventTicketThirdPartyRequest
            {
                MatchId = request.MatchId,
                Mode = ModeType.NumberGame,
                Action = request.Action,
                IsAuto = request.IsAuto,
                SpName = "NAP_Admin_Settlement_s161"
            };

            var result = await scaleOutSettleThirdPartyService.ValidateEventAndTicket3rd(eventTicketThirdPartyRequest);

            return new Result
            {
                ErrorCode = result.ErrorCode,
                ErrorMessage = result.ErrorMessage
            };
        }
    }
}