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

    public NumberGameServiceImp(
        IMapper mapper,
        ScaleOutSettleThirdPartyService scaleOutSettleThirdPartyService,
        CalculateService calculateService)
    {
        this.mapper = mapper;
        this.scaleOutSettleThirdPartyService = scaleOutSettleThirdPartyService;
        this.calculateService = calculateService;
    }

    public async Task<Result> Settle(AutoNumberGameRequest request)
    {
        var resultValidation = await calculateService.ValidationDeployFlag(SpName.NumberGame);

        if (resultValidation.ErrorCode == 0)
        {
            await UpdateMatchInfo(request);
            var criteria = mapper.Map<AutoNumberGameRequest, NumberGameCriteria>(request);
            return await calculateService.Execute(criteria);
        }

        return resultValidation;
    }

    private async Task UpdateMatchInfo(AutoNumberGameRequest request)
    {
        if (!request.IsLive)
        {
            var updateRequest = new UpdateMatchInfo3rdRequest
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