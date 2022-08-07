using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Fanex.Data.Repository;
using Kaden.XmlConverter;
using Microsoft.Extensions.Options;
using ThirdPartyBroadcast.Criteria;
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

        private readonly XmlConvertService xmlConvertService;

        private readonly AppSettingsKey appSettingsKey;

        private readonly IMapper mapper;

        public NumberGameServiceImp(
            IDynamicRepository repository,
            XmlConvertService xmlConvertService,
            IOptions<AppSettingsKey> appSettingsKey,
            IMapper mapper)
        {
            this.repository = repository;
            this.xmlConvertService = xmlConvertService;
            this.appSettingsKey = appSettingsKey.Value;
            this.mapper = mapper;
        }
        public async Task<Result> Settle(NumberGameRequest request)
        {
            var criteria = mapper.Map<NumberGameRequest, NumberGameCriteria>(request);
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
