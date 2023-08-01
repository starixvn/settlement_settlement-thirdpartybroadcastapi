using ThirdPartyBroadcast.Criteria.LeapGaming;
using ThirdPartyBroadcast.Criteria.VirtualBetRadar;

namespace ThirdPartyBroadcast.UnitTests.Services;

public class LeapGamingServiceTests
{
    private readonly CalculateService calculateService;

    private readonly LogDirectlyService logDirectlyService;

    private readonly IDynamicRepository repository;

    private readonly LeapGamingService service;

    private readonly SendMailService sendMailService;

    public LeapGamingServiceTests()
    {
        calculateService = Substitute.For<CalculateService>();
        logDirectlyService = Substitute.For<LogDirectlyService>();
        repository = Substitute.For<IDynamicRepository>();
        sendMailService = Substitute.For<SendMailService>();
        var mapperConfig = new MapperConfigurationExpression();
        mapperConfig.AddProfile<ThirdPartyBroadcastMappingProfile>();
        var config = new MapperConfiguration(mapperConfig);
        var mapper = new Mapper(config);

        service = new LeapGamingServiceImp(mapper, calculateService, logDirectlyService, repository, sendMailService);
    }

    [Fact]
    public async Task Settle_ScoreIncorrectFormat_StopProcessAndReturn()
    {
        var result = await service.Settle(new LeapGamingRequest { SelectedResources = "<Resources> <score Id=\"00\" Home=\"1\" Away=\"1\"/> <score Id=\"01\" Home=\"1\" Away=\"1\"/></Resources>" });

        Assert.True(result.ErrorCode > 0);
        await repository.DidNotReceive().ExecuteAsync(Arg.Any<UpdateMatchCriteria>());
    }

    [Fact]
    public async Task Settle_DeployFlagTurnOn_StopProcessAndReturn()
    {
        var resultFailed = new Result
        {
            ErrorCode = 1
        };

        calculateService.ValidationDeployFlag(SpName.LeapGaming).Returns(resultFailed);

        var result = await service.Settle(new LeapGamingRequest { SelectedResources = "<Resources> <Resource Id=\"00\" Home=\"1\" Away=\"1\"/> <Resource Id=\"01\" Home=\"1\" Away=\"1\"/></Resources>" });

        Assert.Equal(resultFailed.ErrorCode, result.ErrorCode);
        await repository.DidNotReceive().ExecuteAsync(Arg.Any<UpdateMatchCriteria>());
    }

    [Fact]
    public async Task Settle_ValidationPassed_UpdateAndSettleMatch()
    {
        calculateService.ValidationDeployFlag(SpName.LeapGaming).Returns(new Result());
        repository.GetAsync<MatchStatus>(Arg.Any<GetMatchStatusCriteria>()).Returns(new MatchStatus());
        calculateService.Execute(Arg.Any<LeapGamingCriteria>()).Returns(new Result());

        var fakeData = new IEnumerable<object>[3];
        fakeData[0] = new List<MatchInfo> { new() };
        fakeData[1] = new List<TicketInfo> { new() { CustId = 1 } };
        fakeData[2] = new List<TicketInfo> { new() { CustId = 1 } };
        repository
            .FetchMultipleAsync(Arg.Any<LeapGamingSendMailCriteria>(),
                Arg.Any<Type[]>()).Returns(fakeData);

        await service.Settle(new LeapGamingRequest
        {
            Action = "CAL",
            MatchId = 1,
            SelectedResources = "<Resources> <Resource Id=\"00\" Home=\"1\" Away=\"1\"/> <Resource Id=\"01\" Home=\"1\" Away=\"1\"/></Resources>"
        });

        await repository.Received().ExecuteAsync(Arg.Any<UpdateMatchCriteria>());
        await logDirectlyService.Received().WriteLog(1);
        await calculateService.Received().Execute(Arg.Any<LeapGamingCriteria>());
    }

    [Theory]
    [InlineData("CAL", true, false, "<Resources> <Resource Id=\"00\" Home=\"1\" Away=\"1\"/> <Resource Id=\"01\" Home=\"1\" Away=\"1\"/></Resources>", 2)]
    [InlineData("CAL", false, true, "<Resources> <Resource Id=\"00\" Home=\"1\" Away=\"1\"/> <Resource Id=\"01\" Home=\"1\" Away=\"1\"/></Resources>", 2)]
    [InlineData("CAL", false, false, "<Resources> <Resource Id=\"00\" Home=\"1\" Away=\"1\"/> <Resource Id=\"01\" Home=\"1\" Away=\"1\"/></Resources>", 0)]
    [InlineData("REFUND", false, false, "<Resources> <Resource Id=\"00\" Home=\"\" Away=\"\"/> <Resource Id=\"01\" Home=\"\" Away=\"\"/></Resources>", 2)]
    public async Task Settle_ActionReCalOrRefund_SendMail(string action, bool calculate, bool calRefund, string selectedResources, int mailCount)
    {
        calculateService.ValidationDeployFlag(SpName.LeapGaming).Returns(new Result());
        repository.GetAsync<MatchStatus>(Arg.Any<GetMatchStatusCriteria>()).Returns(new MatchStatus { Calculate = calculate, CalRefund = calRefund });
        calculateService.Execute(Arg.Any<LeapGamingCriteria>()).Returns(new Result());

        var fakeData = new IEnumerable<object>[3];
        fakeData[0] = new List<MatchInfo> { new() };
        fakeData[1] = new List<TicketInfo> { new() { CustId = 1 } };
        fakeData[2] = new List<TicketInfo> { new() { CustId = 1 } };
        repository
            .FetchMultipleAsync(Arg.Any<LeapGamingSendMailCriteria>(),
                Arg.Any<Type[]>()).Returns(fakeData);

        await service.Settle(new LeapGamingRequest
        {
            Action = action,
            MatchId = 1,
            SelectedResources = selectedResources
        });

        sendMailService.Received(mailCount).SendMail(Arg.Any<NotificationModel>());
    }
}