using Fanex.Data.Repository;
using Fanex.Settlement.ScaleOut.Request;
using Fanex.Settlement.ScaleOut.Services;
using Microsoft.Extensions.Options;
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

        private const int DeployFlag = 99;

        public CalculateServiceImp(
            IDynamicRepository repository,
            IOptions<AppSettingsKey> appSettingsKey,
            ScaleOutSettleThirdPartyService scaleOutSettleThirdPartyService)
        {
            this.repository = repository;
            this.appSettingsKey = appSettingsKey.Value;
            this.scaleOutSettleThirdPartyService = scaleOutSettleThirdPartyService;
        }

        public async Task<Result> Execute(CalculateCriteria criteria)
        {
            var results = new List<Result>();
            foreach (var server in appSettingsKey.SiteDbServer)
            {
                criteria.DbServer = server;
                await repository.ExecuteAsync(criteria);
                results = ResultMessage(results, server, criteria);
            }

            var deployFlagError = results.Where(result => result.ErrorCode == DeployFlag);
            var errorCode = 0;

            if (deployFlagError.Any())
            {
                errorCode = DeployFlag;
            }
            else if (results.Count > 0)
            {
                errorCode = results[0].ErrorCode;
            }

            return new Result
            {
                ErrorCode = errorCode,
                ErrorMessage = string.Join(" | ", results.Select(r => r.ErrorMessage).ToArray())
            };
        }

        private static List<Result> ResultMessage(List<Result> results, string server, CalculateCriteria criteria)
        {
            if (criteria.ErrCode > 0)
            {
                var result = new Result
                {
                    ErrorCode = criteria.ErrCode,
                    ErrorMessage = $"{server}: {criteria.ErrMsg}"
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