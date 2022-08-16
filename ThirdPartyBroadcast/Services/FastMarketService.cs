using AutoMapper;
using Fanex.Data.Repository;
using Fanex.Settlement.ScaleOut.Request;
using Fanex.Settlement.ScaleOut.Services;
using MediatR;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
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

        private readonly CalculateService calculateService;

        private readonly string fastMarketMode = "FM";

        public FastMarketServiceImp(ScaleOutSettleService scaleOutSettleService,
            IMapper mapper,
             IDynamicRepository repository,
             IOptions<AppSettingsKey> appSettingsKey,
             CalculateService calculateService)
        {
            this.scaleOutSettleService = scaleOutSettleService;
            this.mapper = mapper;
            this.repository = repository;
            this.appSettingsKey = appSettingsKey.Value;
            this.calculateService = calculateService;
        }

        public async Task<Result> Settle(FastMarketRequest request)
        {
            var criteria = mapper.Map<FastMarketRequest, FastMarketCriteria>(request);
            var requestValidation = mapper.Map<FastMarketCriteria, EventTicketFastMarketRequest>(criteria);
            var validationResult = await scaleOutSettleService.ValidateEventAndTicketFastMarket(requestValidation);
            if (validationResult.ErrorCode == 0)
            {
                await UpdateMatchInfo(criteria);
                return await calculateService.Excecute(criteria);
            }

            return new Result();
        }

        private async Task UpdateMatchInfo(FastMarketCriteria criteria)
        {
            var requestUpdateInfo = new UpdateMatchInfoRequest
            {
                MatchId = criteria.MatchId,
                Action = criteria.Action,
                AdminId = criteria.AdminId,
                Results = criteria.Results,
                Mode = fastMarketMode,
                IsSettleDetermined = criteria.IsSettleDetermined,
                IsAuto = criteria.IsAuto
            };

            await scaleOutSettleService.UpdateMatchInfo(requestUpdateInfo);
        }
    }
}