using System.Xml.Serialization;
using ThirdPartyBroadcast.Criteria.LeapGaming;
using ThirdPartyBroadcast.Criteria.VirtualBetRadar;

namespace ThirdPartyBroadcast.Services;

public interface LeapGamingService
{
    Task<Result> Settle(LeapGamingRequest request);
}

public class LeapGamingServiceImp : LeapGamingService
{
    private readonly IMapper mapper;

    private readonly CalculateService calculateService;

    private readonly LogDirectlyService logDirectlyService;

    private readonly IDynamicRepository repository;

    private readonly SendMailService sendMailService;

    private const int ErrorCodeNotExistValue = 1_000_003;

    private const string ErrorMsgNotExistValue = "The input score values are not valid!";

    public LeapGamingServiceImp(IMapper mapper,
        CalculateService calculateService, LogDirectlyService logDirectlyService, IDynamicRepository repository, SendMailService sendMailService)
    {
        this.mapper = mapper;
        this.calculateService = calculateService;
        this.logDirectlyService = logDirectlyService;
        this.repository = repository;
        this.sendMailService = sendMailService;
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

        await logDirectlyService.WriteLog(request.MatchId);

        var result = await calculateService.Execute(criteria);

        await HandleAfterSettle(request, matchStatus, result);

        return result;
    }

    private async Task HandleAfterSettle(LeapGamingRequest request, MatchStatus matchStatus, Result resultSettle)
    {
        var criteria = CreateWriteLogCriteria(request, resultSettle);

        await repository.ExecuteAsync(criteria);

        await SendMail(request.Action, matchStatus ?? new MatchStatus(), request.MatchId);
    }

    private static WriteLogCriteria CreateWriteLogCriteria(LeapGamingRequest request, Result resultSettle)
    {
        var resources = DeserializeResources(request.SelectedResources);
        var message = $"VS 3 - Calculate({request.Action}) {(resultSettle.ErrorCode > 0 ? resultSettle.ErrorMessage : string.Empty)}";
        var actionId = request.Action.ToLowerInvariant().Equals(ActionType.Calculate) ? ActionId.LeapGamingCalc : ActionId.LeapGamingRefund;
        var criteria = new WriteLogCriteria(request.MatchId, message, actionId, request.AdminId, request.Ip);
        var htResult = resources.Items.FirstOrDefault(i => i.Id == ResourceId.HalfTimeResourceId);
        var ftResult = resources.Items.FirstOrDefault(i => i.Id == ResourceId.FullTimeResourceId);

        criteria.FinalAwayScore = int.TryParse(ftResult!.Away, out var away) ? away : 0;
        criteria.FinalHomeScore = int.TryParse(ftResult!.Home, out var home) ? home : 0;
        criteria.HtAwayScore = int.TryParse(htResult!.Away, out var htAway) ? htAway : 0;
        criteria.HtHomeScore = int.TryParse(htResult!.Home, out var htHome) ? htHome : 0;

        return criteria;
    }

    private async Task UpdateMatchInfo(LeapGamingRequest request)
    {
        var requestUpdate = mapper.Map<LeapGamingRequest, UpdateMatchCriteria>(request);
        requestUpdate.Command = EventStatus.Completed;
        await repository.ExecuteAsync(requestUpdate);
    }

    private async Task<Result> ValidationMatch(LeapGamingRequest request)
    {
        var resultValidateMatchScore = ValidateMatchScore(request);

        if (resultValidateMatchScore.ErrorCode > 0)
        {
            return resultValidateMatchScore;
        }

        var validateDeployFlag = await calculateService.ValidationDeployFlag(SpName.LeapGaming);
        return validateDeployFlag.ErrorCode > 0 ? validateDeployFlag : new Result();
    }

    private static Result ValidateMatchScore(LeapGamingRequest request)
    {
        try
        {
            var resources = DeserializeResources(request.SelectedResources);

            if (resources == null || resources.Items.Count == 0 || !ValidateResult(resources.Items, request.Action))
            {
                return new Result
                {
                    ErrorCode = ErrorCodeNotExistValue,
                    ErrorMessage = ErrorMsgNotExistValue
                };
            }
        }
        catch (Exception)
        {
            return new Result
            {
                ErrorCode = ErrorCodeNotExistValue,
                ErrorMessage = ErrorMsgNotExistValue
            };
        }

        return new Result();
    }

    private static ResourcesModel DeserializeResources(string selectedResourced)
    {
        var serializer = new XmlSerializer(typeof(ResourcesModel));
        using var sr = new StringReader(selectedResourced);

        return (ResourcesModel)serializer.Deserialize(sr);
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
            const int depositPosition = 2;
            const int creditPosition = 1;
            var mappingTypes = new[] { typeof(MatchInfo), typeof(TicketInfo), typeof(TicketInfo) };
            var criteria = new LeapGamingSendMailCriteria { MatchId = matchId };
            var fetched = await repository.FetchMultipleAsync(criteria, mappingTypes);

            SendMailForType(EmailType.Credit,
                action,
                fetched[creditPosition].Cast<TicketInfo>().ToList(),
                criteria.CreditRecipients, criteria.CopyRecipients,
                matchId,
                fetched.First().Cast<MatchInfo>().FirstOrDefault());
            SendMailForType(EmailType.Deposit,
                action,
                fetched[depositPosition].Cast<TicketInfo>().ToList(),
                criteria.DepositRecipients,
                criteria.CopyRecipients, matchId,
                fetched.First().Cast<MatchInfo>().FirstOrDefault());
        }
    }

    private void SendMailForType(string type, string action, IReadOnlyCollection<TicketInfo> ticketInfos, string toRecipients, string copyRecipients, int matchId, MatchInfo matchInfos)
    {
        if (!ticketInfos.Any())
        {
            return;
        }
        var mailModel = new NotificationModel
        {
            MatchInfo = matchInfos,
            MatchId = matchId,
            Action = string.Equals(action, ActionType.Refund, StringComparison.OrdinalIgnoreCase) ? "Refund" : "ReSettle",
            To = toRecipients,
            Copy = copyRecipients,
            Type = type
        };

        mailModel.Ticket = type switch
        {
            EmailType.Credit => new List<TicketModel> { new TicketModel { TicketInfos = ticketInfos.ToList() } },
            EmailType.Deposit => SplitTicketByLicensee(ticketInfos.ToList()),
            _ => mailModel.Ticket
        };

        mailModel.MatchInfo.TotalTickets = mailModel.Ticket.SelectMany(i => i.TicketInfos).Distinct().Count();

        sendMailService.SendMail(mailModel);
    }

    private static IEnumerable<TicketModel> SplitTicketByLicensee(IReadOnlyCollection<TicketInfo> ticketInfos)
    {
        var distinctLicensees = ticketInfos.Select(ticket => ticket.SiteName).Distinct();

        return distinctLicensees.Select(licensee => new TicketModel
        {
            TicketInfos = ticketInfos.Where(ticket => ticket.SiteName == licensee).ToList()
        });
    }

    private static bool ValidateResult(IReadOnlyCollection<ResourceItemModel> items, string action)
    {
        var htResult = items.FirstOrDefault(i => i.Id == ResourceId.HalfTimeResourceId);
        var ftResult = items.FirstOrDefault(i => i.Id == ResourceId.FullTimeResourceId);

        return IsValidResult(htResult, action) && IsValidResult(ftResult, action);
    }

    private static bool IsValidResult(ResourceItemModel result, string action)
    {
        if (action.ToLowerInvariant() == ActionType.Refund)
        {
            return result is { Home: "", Away: "" };
        }
        return result != null && int.TryParse(result.Home, out _) && int.TryParse(result.Away, out _);
    }
}