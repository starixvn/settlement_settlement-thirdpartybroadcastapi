namespace ThirdPartyBroadcast.Services;

using Criteria.VirtualSoccer;

public interface VirtualSoccerService
{
    Task<Result> Settle(AutoVirtualSoccerRequest request);
}

public class VirtualSoccerServiceImp : VirtualSoccerService
{
    private readonly ScaleOutSettleThirdPartyService scaleOutSettleThirdPartyService;

    private readonly IDynamicRepository repository;

    private readonly IMapper mapper;

    private readonly CalculateService calculateService;

    private readonly LogDirectlyService logDirectlyService;

    public VirtualSoccerServiceImp(ScaleOutSettleThirdPartyService scaleOutSettleThirdPartyService,
        IDynamicRepository repository,
        IMapper mapper,
        CalculateService calculateService,
        LogDirectlyService logDirectlyService)
    {
        this.scaleOutSettleThirdPartyService = scaleOutSettleThirdPartyService;
        this.repository = repository;
        this.mapper = mapper;
        this.calculateService = calculateService;
        this.logDirectlyService = logDirectlyService;
    }

    public async Task<Result> Settle(AutoVirtualSoccerRequest request)
    {
        var criteria = mapper.Map<AutoVirtualSoccerRequest, VirtualSoccerCriteria>(request);

        var validationResult = await calculateService.ValidationDeployFlag(SpName.VirtualSports);
        if (validationResult.ErrorCode == 0)
        {
            await UpdateMatchInfo(request);
            var message = GenerateMessageLog(criteria);
            await logDirectlyService.WriteLog(criteria.MatchId, message);

            return await calculateService.Execute(criteria);
        }

        return validationResult;
    }

    private static string GenerateMessageLog(VirtualSoccerCriteria criteria)
        => $"NAP: Calculate {criteria.Action} for matchId: {criteria.MatchId} score: {criteria.Fth}-{criteria.Fta}";

    private async Task UpdateMatchInfo(AutoVirtualSoccerRequest request)
    {
        var updateInfoRequest = mapper.Map<AutoVirtualSoccerRequest, UpdateMatchInfo3RdRequest>(request);
        var criteria = mapper.Map<AutoVirtualSoccerRequest, VrSoccerUpdateMatchCriteria>(request);

        if (request.Action.ToLowerInvariant().Equals(ActionType.Calculate))
        {
            updateInfoRequest.FTAway = request.Fta;
            updateInfoRequest.FTHome = request.Fth;
            criteria.Command = "C";
            updateInfoRequest.Command = $"{SpName.VirtualSports}-CAL";
        }
        else
        {
            criteria.Command = "F";
            updateInfoRequest.Command = $"{SpName.VirtualSports}-REFUND";
        }

        await scaleOutSettleThirdPartyService.UpdateMatchInfo3rd(updateInfoRequest);
        await repository.ExecuteAsync(criteria);
    }
}