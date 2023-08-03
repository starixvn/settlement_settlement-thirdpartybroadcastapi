using ThirdPartyBroadcast.Criteria.MoveTicket;

namespace ThirdPartyBroadcast.Services;

public interface MoveTicketService
{
    Task<Result> MoveTicket(MoveTicketRequest request);
}

public class MoveTicketServiceImp : MoveTicketService
{
    private const string ActionMoveTicket = "MoveTicket";
    private readonly IDynamicRepository repository;
    private readonly LogDirectlyService logDirectlyService;
    private readonly ScaleOutSettleService scaleOutSettleService;
    private readonly CalculateService calculateService;

    public MoveTicketServiceImp(
        IDynamicRepository repository,
        LogDirectlyService logDirectlyService,
        ScaleOutSettleService scaleOutSettleService,
        CalculateService calculateService)
    {
        this.repository = repository;
        this.logDirectlyService = logDirectlyService;
        this.scaleOutSettleService = scaleOutSettleService;
        this.calculateService = calculateService;
    }

    public async Task<Result> MoveTicket(MoveTicketRequest request)
    {
        var result = await VerifyBeforeMoveTickets(request.MatchId);

        if (result.ErrorCode > 0)
        {
            await ActionLog(request, result.ErrorMessage);
            return result;
        }

        var getTickets = await GetTickets(request.MatchId);
        WriteLog(request.MatchId, getTickets.ChildMatchId, getTickets.Resources);

        return await ExecuteMoveTicket(request, getTickets);
    }

    private async Task ActionLog(MoveTicketRequest request, string message)
    {
        var criteria = new WriteLogCriteria(request.MatchId, message, ActionId.MoveTicket, request.AdminId, request.Ip);
        await repository.ExecuteAsync(criteria);
    }

    private void WriteLog(int matchId, int childMatchId, string resources)
    {
        var messageLog = $"Move ticket of matchId: {matchId} into child match: {childMatchId}, Settled Results: {resources}";

        logDirectlyService.WriteLog(matchId, messageLog);
    }

    private async Task UpdateChildMatchInfo(int childMatchId)
    {
        var updateChildMatchCriteria = new UpdateChildMatchCriteria { MatchId = childMatchId };
        await repository.ExecuteAsync(updateChildMatchCriteria);

        var updateChildMatchRemarkCriteria = new UpdateChildMatchRemarkCriteria { MatchId = childMatchId };
        await repository.ExecuteAsync(updateChildMatchRemarkCriteria);
    }

    private async Task<MoveTicketInfo> GetTickets(int matchId)
    {
        var criteria = new GetMoveTicketInfoCriteria { MatchId = matchId };
        return await repository.GetAsync<MoveTicketInfo>(criteria);
    }

    private async Task<Result> ExecuteMoveTicket(MoveTicketRequest request, MoveTicketInfo getTickets)
    {
        var criteria = new MoveTicketCriteria
        {
            MatchId = request.MatchId,
            AdminId = request.AdminId,
            Ip = request.Ip,
            EventDate = getTickets.EventDate,
            ChildMatchId = getTickets.ChildMatchId,
            Resources = getTickets.Resources
        };

        var result = await calculateService.ExecuteValidationMultiServer(criteria);
        if (result.ErrorCode == 0)
        {
            await UpdateChildMatchInfo(getTickets.ChildMatchId);
        }

        return result;
    }

    private async Task<Result> VerifyBeforeMoveTickets(int matchId)
    {
        var validateRequest = new EventTicketRequest
        {
            MatchId = matchId,
            Action = ActionMoveTicket,
            SpName = SpName.MoveTicket,
            Mode = "50"
        };

        var resultMessage = await scaleOutSettleService.ValidateEventAndTicket(validateRequest);
        return new Result
        {
            ErrorCode = resultMessage.ErrorCode,
            ErrorMessage = resultMessage.ErrorMessage
        };
    }
}