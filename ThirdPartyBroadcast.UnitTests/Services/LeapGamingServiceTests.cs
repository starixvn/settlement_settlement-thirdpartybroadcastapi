using Fanex.Settlement.ScaleOut.Models;
using ThirdPartyBroadcast.Criteria.LeapGaming;
using ThirdPartyBroadcast.Criteria.VirtualSoccer;

namespace ThirdPartyBroadcast.UnitTests.Services;

public class LeapGamingServiceTests
{
    private readonly ScaleOutSettleService scaleOutSettleService;

    private readonly CalculateService calculateService;

    private readonly LogDirectlyService logDirectlyService;

    private readonly IDynamicRepository repository;

    private readonly LeapGamingService service;

    public LeapGamingServiceTests()
    {
        scaleOutSettleService = Substitute.For<ScaleOutSettleService>();
        calculateService = Substitute.For<CalculateService>();
        logDirectlyService = Substitute.For<LogDirectlyService>();
        repository = Substitute.For<IDynamicRepository>();
        var mapperConfig = new MapperConfigurationExpression();
        mapperConfig.AddProfile<ThirdPartyBroadcastMappingProfile>();
        var config = new MapperConfiguration(mapperConfig);
        var mapper = new Mapper(config);

        service =
            new LeapGamingServiceImp(scaleOutSettleService, mapper, calculateService, logDirectlyService, repository);
    }

    [Fact]
    public async Task Settle_ValidateFailed_StopProcessAndReturn()
    {
        var resultFailed = new ResultMessage
        {
            ErrorCode = 1
        };
        await repository.ExecuteAsync(Arg.Do<LeapGamingValidationCriteria>(criteria => criteria.ErrCode = 1));

        var result = await service.Settle(new LeapGamingRequest());

        Assert.Equal(resultFailed.ErrorCode, result.ErrorCode);

        await scaleOutSettleService.DidNotReceive().UpdateMatchInfo(Arg.Any<UpdateMatchInfoRequest>());
    }

    [Fact]
    public async Task Settle_DeployFlagTurnOn_StopProcessAndReturn()
    {
        var resultFailed = new Result
        {
            ErrorCode = 1
        };

        await repository.ExecuteAsync(Arg.Do<LeapGamingValidationCriteria>(criteria => criteria.ErrCode = 0));

        calculateService.ValidationDeployFlag(SpName.LeapGaming).Returns(resultFailed);

        var result = await service.Settle(new LeapGamingRequest());

        Assert.Equal(resultFailed.ErrorCode, result.ErrorCode);
        await scaleOutSettleService.DidNotReceive().UpdateMatchInfo(Arg.Any<UpdateMatchInfoRequest>());
    }

    [Theory]
    [InlineData("CAL", "C")]
    [InlineData("REFUND", "F")]
    public async Task Settle_ValidationPassed_UpdateAndSettleMatch(string action, string commamd)
    {
        await repository.ExecuteAsync(Arg.Do<LeapGamingValidationCriteria>(criteria => criteria.ErrCode = 0));

        calculateService.ValidationDeployFlag(SpName.LeapGaming).Returns(new Result());

        await service.Settle(new LeapGamingRequest { Action = action, MatchId = 1 });

        await scaleOutSettleService.Received().UpdateMatchInfo(Arg.Any<UpdateMatchInfoRequest>());
        await repository.Received().ExecuteAsync(Arg.Is<VrSoccerUpdateMatchCriteria>(c => c.Command == commamd));
        logDirectlyService.Received().WriteLog(1);
        await calculateService.Received().Execute(Arg.Any<LeapGamingCriteria>());
    }
}