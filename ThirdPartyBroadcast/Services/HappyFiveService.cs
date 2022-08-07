using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fanex.Data.Repository;
using Kaden.XmlConverter;
using Microsoft.Extensions.Options;
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

        private readonly XmlConvertService xmlConvertService;

        private readonly AppSettingsKey appSettingsKey;

        public HappyFiveServiceImp(
            IDynamicRepository repository,
            XmlConvertService xmlConvertService,
            IOptions<AppSettingsKey> appSettingsKey)
        {
            this.repository = repository;
            this.xmlConvertService = xmlConvertService;
            this.appSettingsKey = appSettingsKey.Value;
        }

        public async Task<Result> Settle(HappyFiveRequest request)
        {
            var criteria = CreateCriteria(request);

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

        private HappyFiveCriteria CreateCriteria(HappyFiveRequest request)
        {
            var resources = ResourcesBalls(request.Balls);

            return new HappyFiveCriteria
            {
                MatchId = request.MatchId,
                Action = request.Action,
                Resources = resources,
                IsAuto = request.IsAuto,
            };
        }

        private string ResourcesBalls(List<BallModel> balls)
        {
            var resourceItems = balls.Select(ball => new ResourcesItem { Seq = ball.Seq, NumberBall = ball.NumberBall }).ToList();

            var resources = new Resources
            {
                ResourceItems = resourceItems
            };

            return xmlConvertService.Serialize(resources);
        }
    }
}
