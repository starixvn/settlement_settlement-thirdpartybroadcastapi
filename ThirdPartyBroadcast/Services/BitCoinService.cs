using AutoMapper;
using Fanex.Data.Repository;
using Fanex.Settlement.ScaleOut.Request;
using Fanex.Settlement.ScaleOut.Services;
using MediatR;
using System.Threading.Tasks;
using ThirdPartyBroadcast.Constants;
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

        private const string compeleted = "completed";

        private const int RefundId = 81034;

        private const int CalcId = 81033;

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
            var validationResult = await calculateService.ValidationDeloyFlag(SpName.BitCoin);
            if (validationResult.ErrorCode == 0)
            {
                var eventStatus = await GetEventStatus(request.MatchId);
                await UpdateMatchInfo(request);
                var criteria = mapper.Map<AutoBitCoinRequest, BitCoinCriteria>(request);
                var result = await calculateService.Excecute(criteria);
                await SendMail(result, request, eventStatus);
                return result;
            }

            return validationResult;
        }

        private async Task<string> GetEventStatus(int matchId)
        {
            var criteria = new BitCoinEventStatusCriteria { MatchId = matchId };

            return await repository.GetAsync<string>(criteria);
        }

        private async Task SendMail(Result result, AutoBitCoinRequest request, string eventStatus)
        {
            if (result.ErrorCode > 0 || string.IsNullOrEmpty(eventStatus))
            {
                return;
            }
            if (eventStatus.ToLower().Equals(compeleted) || request.Action.ToLower() == ActionType.Refund)
            {
                var criteria = new SendMailCriteria
                {
                    MatchId = request.MatchId,
                    NotifyAction = request.Action.ToLower() == ActionType.Refund ? RefundId : CalcId
                };
                await repository.ExecuteAsync(criteria);
            }
        }

        private async Task UpdateMatchInfo(AutoBitCoinRequest request)
        {
            var criteria = mapper.Map<AutoBitCoinRequest, BitCoinUpdateMatchCriteria>(request);
            await repository.ExecuteAsync(criteria);

            var updateEventStatus = new UpdateEventStatusCriteria
            {
                MatchId = request.MatchId,
                Action = request.Action
            };
            await repository.ExecuteAsync(updateEventStatus);
        }
    }
}