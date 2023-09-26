using ThirdPartyBroadcast.Criteria.NumberGame;

namespace ThirdPartyBroadcast.Services;

public interface NumberGameService
{
    Task<Result> Settle(AutoNumberGameRequest request);
}

public class NumberGameServiceImp : NumberGameService
{
    private readonly IMapper mapper;

    private readonly ScaleOutSettleThirdPartyService scaleOutSettleThirdPartyService;

    private readonly CalculateService calculateService;

    private readonly LogDirectlyService logDirectlyService;

    public NumberGameServiceImp(
        IMapper mapper,
        ScaleOutSettleThirdPartyService scaleOutSettleThirdPartyService,
        CalculateService calculateService,
        LogDirectlyService logDirectlyService)
    {
        this.mapper = mapper;
        this.scaleOutSettleThirdPartyService = scaleOutSettleThirdPartyService;
        this.calculateService = calculateService;
        this.logDirectlyService = logDirectlyService;
    }

    public async Task<Result> Settle(AutoNumberGameRequest request)
    {
        var resultValidation = await calculateService.ValidationDeployFlag(SpName.NumberGame);

        if (resultValidation.ErrorCode == 0)
        {
            await UpdateMatchInfo(request);

            var criteria = mapper.Map<AutoNumberGameRequest, NumberGameCriteria>(request);

            var message = GenerateLogMessage(criteria);
            await logDirectlyService.WriteLog(criteria.MatchId, message);

            return await calculateService.Execute(criteria);
        }

        return resultValidation;
    }

    private static string GenerateLogMessage(NumberGameCriteria criteria)
    {
        if (criteria.IsLive)
        {
            return $"Calculate Number Game for matchId: {criteria.MatchId}- [LIVE] - Sequence number: {criteria.DrawSeq}";
        }

        return criteria.Action.ToLowerInvariant().Equals(ActionType.Calculate) ? $"Calculate Number Game for matchId: {criteria.MatchId}" : string.Empty;
    }

    private async Task UpdateMatchInfo(AutoNumberGameRequest request)
    {
        if (!request.IsLive)
        {
            var updateRequest = new UpdateMatchInfo3RdRequest
            {
                Command = EventStatus.Completed,
                MatchId = request.MatchId,
                Action = request.Action,
                Creator = request.AdminId
            };
            await scaleOutSettleThirdPartyService.UpdateMatchInfo3rd(updateRequest);
        }
    }
}