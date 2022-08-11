using AutoMapper;
using Fanex.Data.Repository;
using Fanex.Settlement.ScaleOut.Request;
using Fanex.Settlement.ScaleOut.Services;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using ThirdPartyBroadcast.Criteria;
using ThirdPartyBroadcast.Models;
using ThirdPartyBroadcast.Requests;

namespace ThirdPartyBroadcast.Services
{
    public interface FastMarketService
    {
        Task<Result> Settle(FastMarketRequest request);
    }

    public class FastMarketServiceImp : FastMarketService
    {
        private readonly ScaleOutSettleService scaleOutSettleService;

        private readonly IMapper mapper;

        private readonly IDynamicRepository repository;

        private readonly AppSettingsKey appSettingsKey;

        private readonly string fastMarketMode = "FM";

        public FastMarketServiceImp(ScaleOutSettleService scaleOutSettleService,
            IMapper mapper,
             IDynamicRepository repository,
             IOptions<AppSettingsKey> appSettingsKey)
        {
            this.scaleOutSettleService = scaleOutSettleService;
            this.mapper = mapper;
            this.repository = repository;
            this.appSettingsKey = appSettingsKey.Value;
        }

        public async Task<Result> Settle(FastMarketRequest request)
        {
            var criteria = mapper.Map<FastMarketRequest, FastMarketCriteria>(request);

            var validationResult = await ValidateBeforePush(criteria);
            if (validationResult.ErrorCode == 0)
            {
                await UpdateMatchInfo(criteria);
                return await ExecuteCalculate(criteria);
            }

            return validationResult;
        }

        private async Task<Result> ExecuteCalculate(FastMarketCriteria criteria)
        {
            foreach (var server in appSettingsKey.SiteDbServer)
            {
                criteria.DbServer = server;
                await repository.ExecuteAsync(criteria);
                if(criteria.ErrCode > 0)
                {
                    return new Result { ErrorCode = criteria.ErrCode, ErrorMessage = criteria.ErrMsg };
                }
            }

            return new Result();
        }

        private async Task<Result> ValidateBeforePush(FastMarketCriteria criteria )
        {
            var validationRequest = new EventTicketRequest
            {
                MatchId = criteria.MatchId,
                Action = criteria.Action,
                SpName = "NAP_Admin_Settlement_Fast_Market_Auto_1s_Push",
                Mode = fastMarketMode,
                SelectedResources = criteria.Results,
                IsAuto = true
            };

            var validationResult = await scaleOutSettleService.ValidateEventAndTicket(validationRequest);
  
                return new Result
                {
                    ErrorCode = validationResult.ErrorCode,
                    ErrorMessage = validationResult.ErrorMessage
                };
          
        }


        private async Task UpdateMatchInfo(FastMarketCriteria criteria)
        {
            var requestUpdateInfo = new UpdateMatchInfoRequest
            {
                MatchId = criteria.MatchId,
                Action = criteria.Action,
                AdminId = criteria.AdminId,
                Results = criteria.Results,
                EventCounters = criteria.EventCounters,
                Mode = fastMarketMode,
                IsSettleDetermined = criteria.IsSettleDetermined,
                IsAuto = true
            };

            await scaleOutSettleService.UpdateMatchInfo(requestUpdateInfo);
        }

    }
}
