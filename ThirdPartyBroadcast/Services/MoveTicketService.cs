using Fanex.Settlement.ScaleOut.Criteria.Normal;
using Microsoft.Extensions.Options;
using ThirdPartyBroadcast.Criteria.MoveTicket;
using ActionName = ThirdPartyBroadcast.Constants.ActionName;

namespace ThirdPartyBroadcast.Services;

public interface MoveTicketService
{
    Task<Result> MoveTicket(MoveTicketRequest request);
}

public class MoveTicketServiceImp : MoveTicketService
{
    private readonly IDynamicRepository repository;
    private readonly LogDirectlyService logDirectlyService;
    private readonly AppSettingsKey appSettingsKey;

    private const string NotExistValidTicketToMove =
        "WARNING! Cannot continue due to current has no valid ticket to move.";

    private const int NotExistValidTicketToMoveCode = 1_001_049;

    public MoveTicketServiceImp(
        IDynamicRepository repository,
        LogDirectlyService logDirectlyService,
        IOptions<AppSettingsKey> options)
    {
        this.repository = repository;
        this.logDirectlyService = logDirectlyService;
        appSettingsKey = options.Value;
    }

    public async Task<Result> MoveTicket(MoveTicketRequest request)
    {
        var result = await VerifyBeforeMoveTickets(request.MatchId);

        if (result.ErrorCode > 0)
        {
            await ActionLog(request, result.ErrorMessage, ActionId.MoveTicketFailed);
            return result;
        }

        var getTickets = await GetTickets(request.MatchId);

        return await ExecuteMoveTicket(request, getTickets);
    }

    private async Task<Result> VerifyBeforeMoveTickets(int matchId)
    {
        var validateCriteria = new ValidateEventCriteria
        {
            MatchId = matchId,
            Action = ActionName.MoveTicket,
            SpName = SpName.MoveTicket,
            Mode = "50"
        };

        await repository.ExecuteAsync(validateCriteria);
        return new Result
        {
            ErrorCode = validateCriteria.ErrCode,
            ErrorMessage = validateCriteria.ErrMsg
        };
    }

    private async Task ActionLog(MoveTicketRequest request, string message, int actionId)
    {
        var criteria = new WriteLogCriteria(request.MatchId, message, actionId, request.AdminId, request.Ip);
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

        var tasks = appSettingsKey.SiteDbServer.Select(s => MoveTicket(s, criteria)).ToList();
        var watch = new System.Diagnostics.Stopwatch();
        watch.Start();
        var results = await Task.WhenAll(tasks.ToArray());
        watch.Stop();

        var result = await HandleAfterMoveTicketAsync(results, watch.ElapsedMilliseconds.ToString(), request, getTickets);

        return new Result
        {
            ErrorCode = result.ErrorCode,
            ErrorMessage = result.ErrorMessage
        };
    }

    private async Task<ResultMoveTicket> MoveTicket(string dbServer, MoveTicketCriteria criteria)
    {
        criteria.DbServer = dbServer;
        await repository.ExecuteAsync(criteria);

        return new ResultMoveTicket
        {
            ErrorMessage = criteria.ErrMsg,
            ErrorCode = criteria.ErrCode,
            TicketCount = criteria.TicketCount
        };
    }

    private async Task<Result> HandleAfterMoveTicketAsync(ResultMoveTicket[] results, string duration, MoveTicketRequest request, MoveTicketInfo getTickets)
    {
        var resultFailed = results.Where(r => r.ErrorCode > 0).ToList();
        var notExistTicketMoved = !Array.Exists(results, r => r.TicketCount > 0);
        if (resultFailed.Any())
        {
            await ActionLog(request, resultFailed[0].ErrorMessage, ActionId.MoveTicketFailed);
            return resultFailed[0];
        }

        if (notExistTicketMoved)
        {
            await ActionLog(request, NotExistValidTicketToMove, ActionId.MoveTicketFailed);
            return new Result { ErrorMessage = NotExistValidTicketToMove, ErrorCode = NotExistValidTicketToMoveCode };
        }

        var logMessage = $"{getTickets.ChildMatchId}<br/> ({getTickets.Resources})<br/>Duration:{duration}ms; TC:{results.Select(r => r.TicketCount).Sum()}";
        WriteLog(request.MatchId, getTickets.ChildMatchId, getTickets.Resources);
        await ActionLog(request, logMessage, ActionId.MoveTicket);
        await UpdateChildMatchInfo(getTickets.ChildMatchId);

        return results[0];
    }
}