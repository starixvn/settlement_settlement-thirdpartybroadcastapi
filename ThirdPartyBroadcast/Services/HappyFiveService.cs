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
using ThirdPartyBroadcast.Models;
using ThirdPartyBroadcast.Requests;

namespace ThirdPartyBroadcast.Services
{
    public interface HappyFiveService
    {
        Task<Result> Settle(HappyFiveRequest request);
    }

    public class HappyFiveServiceImp : HappyFiveService
    {
        private readonly IDynamicRepository repository;

        private readonly ScaleOutSettleThirdPartyService scaleOutSettleThirdPartyService;

        private readonly AppSettingsKey appSettingsKey;

        private readonly IMapper mapper;

        public HappyFiveServiceImp(
            IDynamicRepository repository,
            ScaleOutSettleThirdPartyService scaleOutSettleThirdPartyService,
            IOptions<AppSettingsKey> appSettingsKey,
            IMapper mapper)
        {
            this.repository = repository;
            this.scaleOutSettleThirdPartyService = scaleOutSettleThirdPartyService;
            this.appSettingsKey = appSettingsKey.Value;
            this.mapper = mapper;
        }

        public async Task<Result> Settle(HappyFiveRequest request)
        {
            var resultValidation = await ValidationBeforeSettle(request);

            if (resultValidation.ErrorCode == 0)
            {
                await UpdateMatchInfo(request);
                return await MultiExecute(request);
            }

            return resultValidation;
        }

        private async Task UpdateMatchInfo(HappyFiveRequest request)
        {
            var criteria = mapper.Map<HappyFiveRequest, HappyFiveUpdateMatchCriteria>(request);
            await repository.ExecuteAsync(criteria);
        }

        private async Task<Result> ValidationBeforeSettle(HappyFiveRequest request)
        {
            var eventTicketThirdPartyRequest = new EventTicketThirdPartyRequest
            {
                MatchId = request.MatchId,
                Mode = ModeType.HappyFive,
                Action = request.Action,
                IsAuto = request.IsAuto,
                SpName = "NAP_Admin_Settlement_s164"
            };

            var result = await scaleOutSettleThirdPartyService.ValidateEventAndTicket3rd(eventTicketThirdPartyRequest);

            return new Result
            {
                ErrorCode = result.ErrorCode,
                ErrorMessage = result.ErrorMessage
            };
        }

        private async Task<Result> MultiExecute(HappyFiveRequest request)
        {
            var criteria = mapper.Map<HappyFiveRequest, HappyFiveCriteria>(request);

            var listResult = new List<Result>();

            foreach (var server in appSettingsKey.SiteDbServer)
            {
                criteria.DbServer = server;
                await repository.ExecuteAsync(criteria);
                var result = new Result
                {
                    ErrorCode = criteria.ErrCode,
                    ErrorMessage =$"{server}: {criteria.ErrMsg}" 
                };

                if (result.ErrorCode > 0)
                {
                    listResult.Add(result);
                }
            }

            if (listResult.Any())
            {
                return new Result
                {
                    ErrorCode = 1000,
                    ErrorMessage = string.Join(" |", listResult.Select(x => x.ErrorMessage))
                };
            }

            return new Result();
        }
    }
}
