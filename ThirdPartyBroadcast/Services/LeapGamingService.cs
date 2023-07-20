using ThirdPartyBroadcast.Criteria.LeapGaming;
using ThirdPartyBroadcast.Criteria.VirtualBetRadar;
using ThirdPartyBroadcast.Criteria.VirtualSoccer;

namespace ThirdPartyBroadcast.Services;

public interface LeapGamingService
{
    Task<Result> Settle(LeapGamingRequest request);
}

public class LeapGamingServiceImp : LeapGamingService
{
    private readonly ScaleOutSettleService scaleOutSettleService;

    private readonly IMapper mapper;

    private readonly CalculateService calculateService;

    private readonly LogDirectlyService logDirectlyService;

    private readonly IDynamicRepository repository;

    public LeapGamingServiceImp(ScaleOutSettleService scaleOutSettleService,
        IMapper mapper,
        CalculateService calculateService, LogDirectlyService logDirectlyService, IDynamicRepository repository)
    {
        this.scaleOutSettleService = scaleOutSettleService;
        this.mapper = mapper;
        this.calculateService = calculateService;
        this.logDirectlyService = logDirectlyService;
        this.repository = repository;
    }

    public async Task<Result> Settle(LeapGamingRequest request)
    {
        var criteria = mapper.Map<LeapGamingRequest, LeapGamingCriteria>(request);
        var validationResult = await ValidationMatch(request);
        if (validationResult.ErrorCode > 0)
        {
            return validationResult;
        }

        var matchStatus = await MatchStatus(request.MatchId);

        await UpdateMatchInfo(request);

        await UpdateMatchRemark(request.MatchId, request.Action);

        await logDirectlyService.WriteLog(request.MatchId);

        var result = await calculateService.Execute(criteria);

        await SendMail(request.Action, matchStatus ?? new MatchStatus(), request.MatchId);

        return result;
    }

    private async Task UpdateMatchRemark(int matchId, string action)
    {
        var criteria = new VrSoccerUpdateMatchCriteria()
        {
            MatchId = matchId,
            Command = action == "CAL" ? "C" : "F"
        };

        await repository.ExecuteAsync(criteria);
    }

    private async Task UpdateMatchInfo(LeapGamingRequest request)
    {
        var requestUpdate = mapper.Map<LeapGamingRequest, UpdateMatchInfoRequest>(request);
        requestUpdate.Mode = Mode.LeapGaming;
        await scaleOutSettleService.UpdateMatchInfo(requestUpdate);
    }

    private async Task<Result> ValidationMatch(LeapGamingRequest request)
    {
        var validateMatchCriteria = mapper.Map<LeapGamingValidationCriteria>(request);

        await repository.ExecuteAsync(validateMatchCriteria);

        if (validateMatchCriteria.ErrCode > 0)
        {
            return new Result
            {
                ErrorCode = validateMatchCriteria.ErrCode,
                ErrorMessage = validateMatchCriteria.ErrMsg
            };
        }

        var validateDeployFlag = await calculateService.ValidationDeployFlag(SpName.LeapGaming);
        return validateDeployFlag.ErrorCode > 0 ? validateDeployFlag : new Result();
    }

    private async Task<MatchStatus> MatchStatus(int matchId)
    {
        var criteria = new GetMatchStatusCriteria { MatchId = matchId };
        return await repository.GetAsync<MatchStatus>(criteria);
    }

    private async Task SendMail(string action, MatchStatus matchStatus, int matchId)
    {
        if (action.ToLowerInvariant().Equals(ActionType.Refund) || matchStatus.Calculate || matchStatus.CalRefund)
        {
            var criteria = new LeapGamingSendMailCriteria() { MatchId = matchId, Action = action };
            await repository.ExecuteAsync(criteria);
        }
    }
}