using AutoMapper;
using Fanex.Data.Repository;
using Fanex.Settlement.ScaleOut.Request;
using Fanex.Settlement.ScaleOut.Services;
using Kaden.XmlConverter;
using System.Linq;
using System.Threading.Tasks;
using ThirdPartyBroadcast.Constants;
using ThirdPartyBroadcast.Criteria;
using ThirdPartyBroadcast.Criteria.BetBuilder;
using ThirdPartyBroadcast.Models;
using ThirdPartyBroadcast.Requests;

namespace ThirdPartyBroadcast.Services
{
    public interface BetBuilderService
    {
        Task<Result> Settle(AutoBetBuilderRequest request);
    }

    public class BetBuilderServiceImp : BetBuilderService
    {
        private readonly ScaleOutSettleThirdPartyService scaleOutSettleThirdPartyService;

        private readonly ScaleOutSettleService scaleOutSettleService;

        private readonly CalculateService calculateService;

        private readonly IMapper mapper;

        private readonly XmlConvertService xmlConvertService;

        private readonly IDynamicRepository repository;

        private const string LogMessage = "MarketIDs cannot be auto settled since they have been settled already: ";

        private const int ExistInvalidMarketErrorCode = 1000007;

        private const int Maxlenght = 115;

        public BetBuilderServiceImp(ScaleOutSettleThirdPartyService scaleOutSettleThirdPartyService,
                ScaleOutSettleService scaleOutSettleService,
                CalculateService calculateService,
                IMapper mapper,
                XmlConvertService xmlConvertService,
                IDynamicRepository repository)
        {
            this.scaleOutSettleThirdPartyService = scaleOutSettleThirdPartyService;
            this.scaleOutSettleService = scaleOutSettleService;
            this.calculateService = calculateService;
            this.mapper = mapper;
            this.xmlConvertService = xmlConvertService;
            this.repository = repository;
        }

        public async Task<Result> Settle(AutoBetBuilderRequest request)
        {
            var (validationResult, invalidMarketIds) = await ValidationBeforeSettle(request);
            if (validationResult.ErrorCode == 0)
            {
                var selectedMarket = RemoveInvalidMarket(invalidMarketIds, request.Resources);
                WriteLog(request, invalidMarketIds, validationResult);
                return string.IsNullOrEmpty(selectedMarket) ? validationResult : await UpdateAndSettle(request, selectedMarket, validationResult);
            }

            return validationResult;
        }

        private async Task<Result> UpdateAndSettle(AutoBetBuilderRequest request, string selectedMarket, Result validateResult)
        {
            await UpdateMatchInfo(request, selectedMarket);
            var criteria = mapper.Map<AutoBetBuilderRequest, BetBuilderCriteria>(request);
            criteria.SelectedMarket = selectedMarket;
            var settleResult = await calculateService.Excecute(criteria);
            return HandleError(validateResult, settleResult);
        }

        private Result HandleError(Result validateResult, Result SettleResult)
        {
            if (validateResult.ErrorCode > 0 && SettleResult.ErrorCode > 0)
            {
                validateResult.ErrorMessage += $"| {SettleResult.ErrorMessage}";
                return validateResult;
            }
            return validateResult.ErrorCode > 0 ? validateResult : SettleResult;
        }

        private void WriteLog(AutoBetBuilderRequest request, string invalidMarketIds, Result result)
        {
            if (!string.IsNullOrEmpty(invalidMarketIds))
            {
                var marketIds = invalidMarketIds.Length > Maxlenght ? $"{invalidMarketIds[0..Maxlenght]}..." : invalidMarketIds;
                var actionId = request.Action.ToLower() == ActionType.Calculate ? 81012 : 81010;
                var message = $"{LogMessage}{marketIds}";
                var criteria = new WriteLogCriteria(request.MatchId, message, actionId, request.AdminId, request.Ip);
                repository.Execute(criteria);
                result.ErrorMessage = message;
                result.ErrorCode = ExistInvalidMarketErrorCode;
            }
        }

        private async Task UpdateMatchInfo(AutoBetBuilderRequest request, string selectedMarket)
        {
            var requestUpdate = new UpdateMatchInfoRequest
            {
                MatchId = request.MatchId,
                Mode = ModeType.BetBuilder,
                IsAuto = request.IsAuto,
                Action = request.Action,
                SelectedMarket = selectedMarket
            };

            await scaleOutSettleService.UpdateMatchInfo(requestUpdate);
        }

        private string RemoveInvalidMarket(string invalidMarketIds, string selectionMarket)
        {
            var listMarket = xmlConvertService.Deserialize<ResourcesModel>(selectionMarket);
            var listInvalidMarket = invalidMarketIds.Split(',').ToList();
            listInvalidMarket.ForEach(item =>
            {
                var index = listMarket.Items.FindIndex(i => i.MarketId == item);
                if (index > -1)
                {
                    listMarket.Items.RemoveAt(index);
                }
            });

            return listMarket.Items.Count > 0 ? xmlConvertService.Serialize(listMarket) : string.Empty;
        }

        private async Task<(Result, string)> ValidationBeforeSettle(AutoBetBuilderRequest request)
        {
            var eventTicketThirdPartyRequest = new BetBuilderRequest
            {
                MatchId = request.MatchId,
                Action = request.Action,
                IsAuto = request.IsAuto,
                SelectedMarket = request.Resources,
                VendorID = request.VendorId
            };

            var (resultValidate, invalidMarketIds) = await scaleOutSettleThirdPartyService.ValidateBetBuilder(eventTicketThirdPartyRequest);

            var result = new Result
            {
                ErrorCode = resultValidate.ErrorCode,
                ErrorMessage = resultValidate.ErrorMessage
            };

            return (result, invalidMarketIds ?? "");
        }
    }
}