using AutoMapper;
using Fanex.Data.Repository;
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
            var validationResult = await calculateService.ValidationDeployFlag(SpName.BitCoin);
            if (validationResult.ErrorCode == 0)
            {
                var eventStatus = await GetEventStatus(request.MatchId);
                await UpdateMatchInfo(request);
                var criteria = mapper.Map<AutoBitCoinRequest, BitCoinCriteria>(request);
                var result = await calculateService.Execute(criteria);
                await Notification(result, request, eventStatus);
                return result;
            }

            return validationResult;
        }

        private async Task<string> GetEventStatus(int matchId)
        {
            var criteria = new BitCoinEventStatusCriteria { MatchId = matchId };

            return await repository.GetAsync<string>(criteria);
        }

        private async Task Notification(Result result, AutoBitCoinRequest request, string eventStatus)
        {
            if (result.ErrorCode > 0 || string.IsNullOrWhiteSpace(eventStatus))
            {
                return;
            }

            if (eventStatus.ToLower().Equals(EventStatus.Completed) || request.Action.ToLower() == ActionType.Refund)
            {
                var criteria = new NotifyCriteria
                {
                    MatchId = request.MatchId,
                    NotifyAction = request.Action.ToLower() == ActionType.Refund ? ActionId.BitCoinRefund : ActionId.BitCoinCalc
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