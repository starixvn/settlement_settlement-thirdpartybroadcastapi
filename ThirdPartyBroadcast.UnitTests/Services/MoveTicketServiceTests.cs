using Fanex.Settlement.ScaleOut.Criteria.Normal;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using ThirdPartyBroadcast.Criteria.MoveTicket;

namespace ThirdPartyBroadcast.UnitTests.Services;

public class MoveTicketServiceTests
{
    private readonly IDynamicRepository repository;
    private readonly LogDirectlyService logDirectlyService;
    private readonly MoveTicketService service;

    public MoveTicketServiceTests()
    {
        repository = Substitute.For<IDynamicRepository>();
        logDirectlyService = Substitute.For<LogDirectlyService>();
        repository = Substitute.For<IDynamicRepository>();
        var accessor = Substitute.For<IHttpContextAccessor>();
        var context = new DefaultHttpContext
        {
            Connection =
            {
                RemoteIpAddress = new System.Net.IPAddress(1)
            }
        };
        accessor.HttpContext = context;
        var appSettings = new AppSettingsKey { SiteDbServer = new List<string> { "01", "02" } };
        var options = Options.Create(appSettings);
        service = new MoveTicketServiceImp(repository, logDirectlyService, options);
    }

    [Fact]
    public async Task MoveTicket_ValidateEventFailed_WriteLogFailAndStop()
    {
        await repository.ExecuteAsync(Arg.Do<ValidateEventCriteria>(c =>
        {
            c.ErrCode = 1;
            c.ErrMsg = "error";
        }));
        var result = await service.MoveTicket(new MoveTicketRequest { MatchId = 100 });

        await repository.Received().ExecuteAsync(Arg.Is<WriteLogCriteria>(c =>
            c.MatchId == 100 && c.Remark == "error" &&
            c.ActionId == 81026));
        Assert.Equal("error", result.ErrorMessage);
    }

    [Fact]
    public async Task MoveTicket_NotExistTicketMoved_WriteLogFailAndStop()
    {
        repository.GetAsync<MoveTicketInfo>(Arg.Any<GetMoveTicketInfoCriteria>()).Returns(new MoveTicketInfo());
        await repository.ExecuteAsync(Arg.Any<MoveTicketCriteria>());
        var result = await service.MoveTicket(new MoveTicketRequest { MatchId = 100 });

        await repository.Received().ExecuteAsync(Arg.Is<WriteLogCriteria>(c =>
             c.MatchId == 100 && c.Remark == "WARNING! Cannot continue due to current has no valid ticket to move." &&
             c.ActionId == 81026));
        Assert.Equal("WARNING! Cannot continue due to current has no valid ticket to move.", result.ErrorMessage);
    }

    [Fact]
    public async Task MoveTicket_MoveFailed_WriteLogFailAndStop()
    {
        repository.GetAsync<MoveTicketInfo>(Arg.Any<GetMoveTicketInfoCriteria>()).Returns(new MoveTicketInfo());
        await repository.ExecuteAsync(Arg.Do<MoveTicketCriteria>(c =>
        {
            c.ErrCode = 1;
            c.ErrMsg = "error";
        }));
        var result = await service.MoveTicket(new MoveTicketRequest { MatchId = 100 });

        await repository.Received().ExecuteAsync(Arg.Is<WriteLogCriteria>(c =>
            c.MatchId == 100 && c.Remark == "error" &&
            c.ActionId == 81026));
        Assert.Equal("error", result.ErrorMessage);
    }

    [Fact]
    public async Task MoveTicket_ValidatePassed_ExecuteAll()
    {
        repository.GetAsync<MoveTicketInfo>(Arg.Any<GetMoveTicketInfoCriteria>()).Returns(new MoveTicketInfo());
        await repository.ExecuteAsync(Arg.Do<MoveTicketCriteria>(c =>
        {
            c.TicketCount = 1;
        }));
        var result = await service.MoveTicket(new MoveTicketRequest());

        await logDirectlyService.Received().WriteLog(Arg.Any<int>(), Arg.Any<string>());
        await repository.Received().ExecuteAsync(Arg.Any<WriteLogCriteria>());
        await repository.Received(2).ExecuteAsync(Arg.Any<MoveTicketCriteria>());
        await repository.Received(1).ExecuteAsync(Arg.Any<UpdateChildMatchCriteria>());
        await repository.Received(1).ExecuteAsync(Arg.Any<UpdateChildMatchRemarkCriteria>());
        Assert.Equal(0, result.ErrorCode);
    }
}