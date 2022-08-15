using AutoMapper;
using Fanex.Data.Repository;
using Fanex.Settlement.ScaleOut.Request;
using Fanex.Settlement.ScaleOut.Services;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using ThirdPartyBroadcast.Constants;
using ThirdPartyBroadcast.Criteria;
using ThirdPartyBroadcast.Models;
using ThirdPartyBroadcast.Requests;

namespace ThirdPartyBroadcast.Services
{
    public interface BitCoinService
    {
        Task<Result> Settle(BitCoinRequest request);
    }

    public class BitCoinServiceImp : BitCoinService
    {
        private readonly ScaleOutSettleService scaleOutSettleService;

        private readonly IDynamicRepository repository;

        private readonly AppSettingsKey appSettingsKey;

        private readonly IMapper mapper;

        public BitCoinServiceImp(ScaleOutSettleService scaleOutSettleService,
            IDynamicRepository repository,
            IMapper mapper,
            IOptions<AppSettingsKey> appSettingsKey)
        {
            this.scaleOutSettleService = scaleOutSettleService;
            this.repository = repository;
            this.appSettingsKey = appSettingsKey.Value;
            this.mapper = mapper;
        }

        public async Task<Result> Settle(BitCoinRequest request)
        {
            var criteria = mapper.Map<BitCoinRequest, BitCoinCriteria>(request);

            var validationResult = await ValidateBeforePush(request);
            if (validationResult.ErrorCode == 0)
            {
                await UpdateMatchInfo(request);
                return await ExecuteCalculate(criteria);
            }

            return validationResult;
        }

        private async Task<Result> ExecuteCalculate(BitCoinCriteria criteria)
        {
            foreach (var server in appSettingsKey.SiteDbServer)
            {
                criteria.DbServer = server;
                await repository.ExecuteAsync(criteria);
                if (criteria.ErrCode > 0)
                {
                    return new Result { ErrorCode = criteria.ErrCode, ErrorMessage = criteria.ErrMsg };
                }
            }

            return new Result();
        }

        private async Task<Result> ValidateBeforePush(BitCoinRequest request)
        {
            var validationRequest = new EventTicketRequest
            {
                MatchId = request.MatchId,
                Action = request.Action,
                SpName = "NAP_Admin_Settlement_s175",
                SelectedResources = request.GameResult,
                Mode = ModeType.Bitcoin,
                IsAuto = true
            };

            var validationResult = await scaleOutSettleService.ValidateEventAndTicket(validationRequest);

            return new Result
            {
                ErrorCode = validationResult.ErrorCode,
                ErrorMessage = validationResult.ErrorMessage
            };
        }

        private async Task UpdateMatchInfo(BitCoinRequest request)
        {
            var criteria = mapper.Map<BitCoinRequest, BitCoinUpdateMatchCriteria>(request);
            await repository.ExecuteAsync(criteria);
        }
    }
}