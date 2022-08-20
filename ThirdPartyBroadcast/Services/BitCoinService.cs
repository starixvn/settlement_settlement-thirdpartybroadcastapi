using AutoMapper;
using Fanex.Data.Repository;
using Fanex.Settlement.ScaleOut.Request;
using Fanex.Settlement.ScaleOut.Services;
using System.Threading.Tasks;
using ThirdPartyBroadcast.Criteria.BitCoin;
using ThirdPartyBroadcast.Models;
using ThirdPartyBroadcast.Requests;

namespace ThirdPartyBroadcast.Services
{
    public interface BitCoinService
    {
        Task<Result> Settle(AutoBitCoinRequest request);
    }

    public class BitCoinServiceImp : BitCoinService
    {
        private readonly IDynamicRepository repository;

        private readonly IMapper mapper;

        private readonly CalculateService calculateService;

        public BitCoinServiceImp(
            IDynamicRepository repository,
            IMapper mapper,
            CalculateService calculateService)
        {
            this.repository = repository;
            this.mapper = mapper;
            this.calculateService = calculateService;
        }

        public async Task<Result> Settle(AutoBitCoinRequest request)
        {
            var validationResult = await calculateService.ExecuteValidationMultiServer(new BitCoinValidationCriteria());
            if (validationResult.ErrorCode == 0)
            {
                await UpdateMatchInfo(request);
                var criteria = mapper.Map<AutoBitCoinRequest, BitCoinCriteria>(request);
                return await calculateService.Excecute(criteria);
            }

            return validationResult;
        }

        private async Task UpdateMatchInfo(AutoBitCoinRequest request)
        {
            var criteria = mapper.Map<AutoBitCoinRequest, BitCoinUpdateMatchCriteria>(request);
            await repository.ExecuteAsync(criteria);
        }
    }
}