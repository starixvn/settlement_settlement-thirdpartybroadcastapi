using AutoMapper;
using Fanex.Data.Repository;
using Fanex.Settlement.ScaleOut.Criteria;
using Fanex.Settlement.ScaleOut.Models;
using Fanex.Settlement.ScaleOut.Services;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThirdPartyBroadcast.Criteria;
using ThirdPartyBroadcast.Models;

namespace ThirdPartyBroadcast.Services
{
    public interface CalculateService
    {
        Task<Result> Excecute(CalculateCriteria criteria);

        Task<Result> ExecuteValidationMultiServer(BaseCriteria criteria);
    }

    public class CalculateServiceImp : CalculateService
    {
        private readonly IDynamicRepository repository;

        private readonly AppSettingsKey appSettingsKey;

        public CalculateServiceImp(
            IDynamicRepository repository,
            IOptions<AppSettingsKey> appSettingsKey)
        {
            this.repository = repository;
            this.appSettingsKey = appSettingsKey.Value;
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