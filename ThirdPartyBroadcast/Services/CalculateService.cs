using Fanex.Data.Repository;
using Fanex.Settlement.ScaleOut.Request;
using Fanex.Settlement.ScaleOut.Services;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ThirdPartyBroadcast.Criteria;
using ThirdPartyBroadcast.Models;

namespace ThirdPartyBroadcast.Services
{
    public interface CalculateService
    {
        Task<Result> Execute(CalculateCriteria criteria);

        Task<Result> ExecuteValidationMultiServer(BaseCriteria criteria);

        Task<Result> ValidationDeployFlag(string spName);
    }

    public class CalculateServiceImp : CalculateService
    {
        private readonly IDynamicRepository repository;

        private readonly AppSettingsKey appSettingsKey;

        private readonly ScaleOutSettleThirdPartyService scaleOutSettleThirdPartyService;

        private readonly ScaleOutSettleService scaleOutSettleService;

        public CalculateServiceImp(
            IDynamicRepository repository,
            IOptions<AppSettingsKey> appSettingsKey,
            ScaleOutSettleThirdPartyService scaleOutSettleThirdPartyService,
            ScaleOutSettleService scaleOutSettleService)
        {
            this.repository = repository;
            this.appSettingsKey = appSettingsKey.Value;
            this.scaleOutSettleThirdPartyService = scaleOutSettleThirdPartyService;
            this.scaleOutSettleService = scaleOutSettleService;
        }

        public async Task<Result> Execute(CalculateCriteria criteria)
        {
            var results = new List<Result>();
            foreach (var server in appSettingsKey.SiteDbServer)
            {
                criteria.DbServer = server;
                await repository.ExecuteAsync(criteria);
                results = await ResultMessage(results, server, criteria.ErrMsg);
            }

            return new Result
            {
                ErrorCode = results.Count,
                ErrorMessage = string.Join(" | ", results.Select(r => r.ErrorMessage).ToArray())
            };
        }

        private async Task<List<Result>> ResultMessage(List<Result> results, string server, string message)
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                var siteDbGroupId = await scaleOutSettleService.GetSiteGroupId(server);
                var result = new Result
                {
                    ErrorMessage = $"SiteDbGroupId {siteDbGroupId}: {message}"
                };

                results.Add(result);
            }

            return results;
        }

        public async Task<Result> ExecuteValidationMultiServer(BaseCriteria criteria)
        {
            foreach (var server in appSettingsKey.SiteDbServer)
            {
                criteria.DbServer = server;
                await repository.ExecuteAsync(criteria);
                if (criteria.ErrCode > 0)
                {
                    return new Result
                    {
                        ErrorCode = criteria.ErrCode,
                        ErrorMessage = criteria.ErrMsg
                    };
                }
            }
            return new Result();
        }

        public async Task<Result> ValidationDeployFlag(string spName)
        {
            var getDeploymentInfoRequest = new GetDeploymentInfoRequest
            {
                SPName = spName
            };

            var result = await scaleOutSettleThirdPartyService.GetDeploymentInfo(getDeploymentInfoRequest);

            return new Result
            {
                ErrorCode = result.ErrorCode,
                ErrorMessage = result.ErrorMessage
            };
        }
    }
}