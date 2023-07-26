using System.Xml.Serialization;
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

    private readonly SendMailService sendMailService;

    private const int ErrorCodeNotExistHtft = 1000003;

    private const string ErrorMsgNotExistHtft = "The input score values are not valid!";

    public LeapGamingServiceImp(ScaleOutSettleService scaleOutSettleService,
        IMapper mapper,
        CalculateService calculateService, LogDirectlyService logDirectlyService, IDynamicRepository repository, SendMailService sendMailService)
    {
        this.scaleOutSettleService = scaleOutSettleService;
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

        await UpdateMatchRemark(request.MatchId, request.Action);
        await logDirectlyService.WriteLog(request.MatchId);

        var result = await calculateService.Execute(criteria);

        await HandleAfterSettle(request, matchStatus);

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

    private async Task HandleAfterSettle(LeapGamingRequest request, MatchStatus matchStatus)
    {
        var message = $"VS 3 - Calculate({request.Action})";
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

    private async Task<Result> ValidationMatch(LeapGamingRequest request)
    {
        try
        {
            var serializer = new XmlSerializer(typeof(ResourcesModel));
            using var sr = new StringReader(request.SelectedResources);

            var resources = (ResourcesModel)serializer.Deserialize(sr);

            if (resources == null || resources.Items.Count == 0 || !ValidateResult(resources.Items))
            {
                return new Result
                {
                    ErrorCode = ErrorCodeNotExistHtft,
                    ErrorMessage = ErrorMsgNotExistHtft
                };
            }
        }
        catch (Exception)
        {
            return new Result
            {
                ErrorCode = ErrorCodeNotExistHtft,
                ErrorMessage = ErrorMsgNotExistHtft
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
            var mappingTypes = new[] { typeof(MatchInfo), typeof(TicketInfo), typeof(TicketInfo) };
            var criteria = new LeapGamingSendMailCriteria() { MatchId = matchId };
            var fetched = await repository.FetchMultipleAsync(criteria, mappingTypes);

            SendMailForType(EmailType.Credit, action, fetched[1].Cast<TicketInfo>().ToList(), criteria.Credit_Recipients, criteria.Copy_recipients, matchId, fetched[0].Cast<MatchInfo>().FirstOrDefault());
            SendMailForType(EmailType.Deposit, action, fetched[2].Cast<TicketInfo>().ToList(), criteria.Deposit_Recipients, criteria.Copy_recipients, matchId, fetched[0].Cast<MatchInfo>().FirstOrDefault());
        }
    }

    private void SendMailForType(string type, string action, IReadOnlyCollection<TicketInfo> ticketInfos, string toRecipients, string copyRecipients, int matchId, MatchInfo matchInfos)
    {
        if (!ticketInfos.Any()) return;
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

    private static bool ValidateResult(IReadOnlyCollection<ResourceItemModel> items)
    {
        var htResult = items.FirstOrDefault(i => i.Id == ResourceId.HalfTimeResourceId);
        var ftResult = items.FirstOrDefault(i => i.Id == ResourceId.FullTimeResourceId);

        return IsValidResult(htResult) && IsValidResult(ftResult);
    }

    private static bool IsValidResult(ResourceItemModel result)
    {
        return result != null && int.TryParse(result.Home, out _) && int.TryParse(result.Away, out _);
    }
}