using System.Xml.Serialization;
using ThirdPartyBroadcast.Criteria.LeapGaming;
using ThirdPartyBroadcast.Criteria.VirtualBetRadar;
using ThirdPartyBroadcast.Criteria.VirtualSoccer;

namespace ThirdPartyBroadcast.Services;

public record struct Match(int SportType);

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

    private const int ErrorCodeNotExistMatch = 1_000_002;

    private const string ErrorMsgNotExistValue = "The input score values are not valid!";

    private const string ErrorMsgMatchNoValid = "Match does not exist or the sport type is not valid!";

    private const int SportType = 178;

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
        var validationResult = await ValidationMatch(request, criteria);
        if (validationResult.ErrorCode > 0)
        {
            return validationResult;
        }

        var matchStatus = await MatchStatus(request.MatchId);

        await UpdateMatchInfo(request);

        await UpdateMatchRemark(request.MatchId, request.Action);
        await logDirectlyService.WriteLog(request.MatchId);

        var result = await calculateService.Execute(criteria);

        await HandleAfterSettle(request, matchStatus, result);

        return result;
    }

    private async Task UpdateMatchRemark(int matchId, string action)
    {
        var criteria = new VrSoccerUpdateMatchCriteria
        {
            MatchId = matchId,
            Command = action.ToLowerInvariant() == ActionType.Calculate ? "C" : "F"
        };

        await repository.ExecuteAsync(criteria);
    }

    private async Task HandleAfterSettle(LeapGamingRequest request, MatchStatus matchStatus, Result resultSettle)
    {
        var message = $"VS 3 - Calculate({request.Action}) {(resultSettle.ErrorCode > 0 ? resultSettle.ErrorMessage : string.Empty)}";
        var actionId = request.Action.ToLowerInvariant().Equals(ActionType.Calculate) ? ActionId.LeapGamingCalc : ActionId.LeapGamingRefund;
        var criteria = new WriteLogCriteria(request.MatchId, message, actionId, request.AdminId, request.Ip);

        await repository.ExecuteAsync(criteria);

        await SendMail(request.Action, matchStatus ?? new MatchStatus(), request.MatchId);
    }

    private async Task UpdateMatchInfo(LeapGamingRequest request)
    {
        var requestUpdate = mapper.Map<LeapGamingRequest, UpdateMatchCriteria>(request);
        requestUpdate.Command = EventStatus.Completed;
        await repository.ExecuteAsync(requestUpdate);
    }

    private async Task<Result> ValidationMatch(LeapGamingRequest request, LeapGamingCriteria criteria)
    {
        var resultValidateMatchInfo = await ValidateMatchInfo(request.MatchId, criteria);
        if (resultValidateMatchInfo.ErrorCode > 0)
        {
            return resultValidateMatchInfo;
        }
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
            var serializer = new XmlSerializer(typeof(ResourcesModel));
            using var sr = new StringReader(request.SelectedResources);

            var resources = (ResourcesModel)serializer.Deserialize(sr);

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

    private async Task<Result> ValidateMatchInfo(int matchId, LeapGamingCriteria criteriaSettle)
    {
        var criteria = new LeapGamingMatchCriteria { MatchId = matchId };
        var match = await repository.GetAsync<Match>(criteria);

        if (match is not { SportType: SportType })
        {
            return new Result
            {
                ErrorCode = ErrorCodeNotExistMatch,
                ErrorMessage = ErrorMsgMatchNoValid
            };
        }

        criteriaSettle.SportType = match.SportType;

        return new Result();
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
                criteria.Credit_Recipients, criteria.Copy_recipients,
                matchId,
                fetched.First().Cast<MatchInfo>().FirstOrDefault());
            SendMailForType(EmailType.Deposit,
                action,
                fetched[depositPosition].Cast<TicketInfo>().ToList(),
                criteria.Deposit_Recipients,
                criteria.Copy_recipients, matchId,
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