using System.Threading.Tasks;
using ThirdPartyBroadcast.Models;
using ThirdPartyBroadcast.Requests;
using Fanex.Data.Repository;
using AutoMapper;
using ThirdPartyBroadcast.Constants;
using ThirdPartyBroadcast.Criteria.VirtualBetRadar;
using ThirdPartyBroadcast.Criteria;

namespace ThirdPartyBroadcast.Services
{
    public interface VirtualBetRadarService
    {
        Task<Result> Settle(AutoVirtualBetRadarRequest request);
    }

    public class VirtualBetRadarServiceImp : VirtualBetRadarService
    {
        private readonly IDynamicRepository repository;

        private readonly IMapper mapper;

        private readonly CalculateService calculateService;

        
        public VirtualBetRadarServiceImp(
            IDynamicRepository repository,
            IMapper mapper,
            CalculateService calculateService)
        {
            this.repository = repository;
            this.mapper = mapper;
            this.calculateService = calculateService;
        }

        public async Task<Result> Settle(AutoVirtualBetRadarRequest request)
        {
            var criteria = mapper.Map<AutoVirtualBetRadarRequest, VirtualBetRadarCriteria>(request);
            var validation = await ValidateBeforeSettle(request);
            if (validation.ErrorCode == 0)
            {
                await UpdateMatchInfo(request);
                var settleResult = await calculateService.Execute(criteria);
                HandleAfterSettle(request, settleResult);
                return settleResult;
            }

            return validation;
        }

        private void HandleAfterSettle(AutoVirtualBetRadarRequest request, Result settleResult)
        {
            const string successfullyMessage = "NAP: VS BetRadar - API inserted into single queue successfully.";
            var actionId = request.Action.ToLowerInvariant().Equals(ActionType.Calculate) ? ActionId.VirtualBetRadarCalc : ActionId.VirtualBetRadarRefund;
            var criteria = new WriteLogCriteria(request.MatchId, successfullyMessage, actionId, request.AdminId, request.Ip)
            {
                FinalAwayScore = request.Fta,
                FinalHomeScore = request.Fth,
                HtAwayScore = request.Hta,
                HtHomeScore = request.Hth
            };

            if (settleResult.ErrorCode == 0)
            {
                repository.Execute(criteria);
            }
            else
            {
                WriteLogFailed(criteria, settleResult.ErrorMessage);
            }
        }

        private void WriteLogFailed(WriteLogCriteria criteria, string errorMessage)
        {
            const string failedMessage = "NAP: VS BetRadar - Error: API inserted into single queue failed.";
            criteria.Remark = $"{failedMessage} {errorMessage}";
            repository.Execute(criteria);

            var sendMailCriteria = mapper.Map<WriteLogCriteria, SendMailCriteria>(criteria);
            sendMailCriteria.Message = errorMessage;
            repository.Execute(sendMailCriteria);
        }

        private async Task<Result> ValidateBeforeSettle(AutoVirtualBetRadarRequest request)
        {
            var validateDeployFlag = await calculateService.ValidationDeployFlag(SpName.VirtualBetRadar);
            if (validateDeployFlag.ErrorCode > 0)
            {
                return validateDeployFlag;
            }
            else
            {
                var criteriaValidation = mapper.Map<AutoVirtualBetRadarRequest, BetRadarValidationCriteria>(request);
                return await calculateService.ExecuteValidationMultiServer(criteriaValidation);
            }
        }

        private async Task UpdateMatchInfo(AutoVirtualBetRadarRequest request)
        {
            var criteria = mapper.Map<AutoVirtualBetRadarRequest, BetRadarUpdateMatchCriteria>(request);
            await repository.ExecuteAsync(criteria);
        }
    }
}