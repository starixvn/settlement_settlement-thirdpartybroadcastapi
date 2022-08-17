using AutoMapper;
using Fanex.Data.Repository;
using Fanex.Settlement.ScaleOut.Request;
using Fanex.Settlement.ScaleOut.Services;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using ThirdPartyBroadcast.Constants;
using ThirdPartyBroadcast.Criteria.BitCoin;
using ThirdPartyBroadcast.Models;
using ThirdPartyBroadcast.Requests;

namespace ThirdPartyBroadcast.Services
{
    public interface BitCoinService
    {
        Task<Result> Settle(BitCoinRequest request);
    }

    public class BitCoinServiceImp : BitCoinService
    {
        private readonly ScaleOutSettleThirdPartyService scaleOutSettleThirdPartyService;

        private readonly IDynamicRepository repository;

        private readonly IMapper mapper;

        private readonly CalculateService calculateService;

        public BitCoinServiceImp(ScaleOutSettleThirdPartyService scaleOutSettleThirdPartyService,
            IDynamicRepository repository,
            IMapper mapper,
            CalculateService calculateService)
        {
            this.scaleOutSettleThirdPartyService = scaleOutSettleThirdPartyService;
            this.repository = repository;
            this.mapper = mapper;
            this.calculateService = calculateService;
        }

        public async Task<Result> Settle(BitCoinRequest request)
        {
            var validationResult = await ValidateBeforePush(request);
            if (validationResult.ErrorCode == 0)
            {
                await UpdateMatchInfo(request);
                var criteria = mapper.Map<BitCoinRequest, BitCoinCriteria>(request);
                return await calculateService.Excecute(criteria);
            }

            return validationResult;
        }

        private async Task<Result> ValidateBeforePush(BitCoinRequest request)
        {
            var validationRequest = new EventTicketThirdPartyRequest
            {
                MatchId = request.MatchId,
                Action = request.Action,
                SpName = "NAP_Admin_Settlement_s175",
                SelectedResources = request.GameResult,
                Mode = ModeType.Bitcoin,
                IsAuto = request.IsAuto
            };

            var validationResult = await scaleOutSettleThirdPartyService.ValidateEventAndTicket3rd(validationRequest);

            return new Result
            {
                ErrorCode = validationResult.ErrorCode,
                ErrorMessage = validationResult.ErrorMessage
            };
        }

        private async Task UpdateMatchInfo(BitCoinRequest request)
        {
            var criteria = mapper.Map<BitCoinRequest, BitCoinUpdateMatchCriteria>(request);
            await repository.ExecuteAsync(criteria);
        }
    }
}