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
        Task<Result> Excecute(CalculateCriteria criteria);

        Task<Result> ExecuteValidationMultiServer(BaseCriteria criteria);

        Task<Result> ValidationDeloyFlag(string spName);
    }

    public class CalculateServiceImp : CalculateService
    {
        private readonly IDynamicRepository repository;

        private readonly AppSettingsKey appSettingsKey;

        private readonly ScaleOutSettleThirdPartyService scaleOutSettleThirdPartyService;

        public CalculateServiceImp(
            IDynamicRepository repository,
            IOptions<AppSettingsKey> appSettingsKey,
            ScaleOutSettleThirdPartyService scaleOutSettleThirdPartyService)
        {
            this.repository = repository;
            this.appSettingsKey = appSettingsKey.Value;
            this.scaleOutSettleThirdPartyService = scaleOutSettleThirdPartyService;
        }

        public async Task<Result> Excecute(CalculateCriteria criteria)
        {
            var listResult = new List<Result>();

            foreach (var server in appSettingsKey.SiteDbServer)
            {
                criteria.DbServer = server;
                await repository.ExecuteAsync(criteria);
                var result = new Result
                {
                    ErrorCode = criteria.ErrCode,
                    ErrorMessage = $"{server}: {criteria.ErrMsg}"
                };

                if (result.ErrorCode > 0)
                {
                    listResult.Add(result);
                }
            }

            return MergeFailedResult(listResult);
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

        public async Task<Result> ValidationDeloyFlag(string spName)
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

        private Result MergeFailedResult(List<Result> listResult)
        {
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