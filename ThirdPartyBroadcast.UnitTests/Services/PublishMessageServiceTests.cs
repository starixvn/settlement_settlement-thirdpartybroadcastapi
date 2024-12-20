using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using ONEbook.UM.Services;

namespace ThirdPartyBroadcast.UnitTests.Services;

public class PublishMessageServiceTests
{
    private readonly FlagService flagService;

    private readonly ManageQueueService manageQueueService;

    private readonly IMaintenanceService maintenanceService;

    private readonly PublishMessageService service;

    public PublishMessageServiceTests()
    {
        flagService = Substitute.For<FlagService>();
        manageQueueService = Substitute.For<ManageQueueService>();
        maintenanceService = Substitute.For<IMaintenanceService>();
        var accessor = Substitute.For<IHttpContextAccessor>();
        var context = new DefaultHttpContext
        {
            Connection =
            {
                RemoteIpAddress = new System.Net.IPAddress(1)
            }
        };
        accessor.HttpContext = context;
        var appSettingsKey = new AppSettingsKey { SiteDbServer = ["01", "02"], RetryCount = 1 };
        var options = Options.Create(appSettingsKey);
        service = new PublishMessageServiceImp(flagService, manageQueueService, options, maintenanceService);
    }

    [Fact]
    public async Task Publisher_IsMaintenance_CallPublishToWaiting()
    {
        maintenanceService.IsUnderMaintenanceAsync().Returns(true);
        flagService.IsDeployedFromCache(CacheKeys.BroadcastFlagCacheKey).Returns(false);
        flagService.GetProductTypeFromCache(CacheKeys.BroadcastProductCacheKey).Returns(string.Empty);
        manageQueueService.PublishToWaiting(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>()).Returns(new Result());

        var result = await service.Publisher(string.Empty, string.Empty);

        Assert.True(result.IsSuccessful);
        await manageQueueService.Received().PublishToWaiting(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>());
        await manageQueueService.DidNotReceive().PublishToProcessing(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>());
        await manageQueueService.DidNotReceive().ProcessWaitingQueue(Arg.Any<string>());
    }

    [Fact]
    public async Task Publisher_ProductAndIsDeployed_CallPublishToWaiting()
    {
        const string product = "1,3,4,5";
        maintenanceService.IsUnderMaintenanceAsync().Returns(false);
        flagService.IsDeployedFromCache(CacheKeys.BroadcastFlagCacheKey).Returns(true);
        flagService.GetProductTypeFromCache(CacheKeys.BroadcastProductCacheKey).Returns(product);
        manageQueueService.PublishToWaiting(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>()).Returns(new Result());

        var result = await service.Publisher("3", string.Empty);

        Assert.True(result.IsSuccessful);
        await manageQueueService.Received().PublishToWaiting(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>());
        await manageQueueService.DidNotReceive().PublishToProcessing(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>());
        await manageQueueService.DidNotReceive().ProcessWaitingQueue(Arg.Any<string>());
    }

    [Fact]
    public async Task Publisher_IsNotDeployed_CallProcessWaitingQueue()
    {
        maintenanceService.IsUnderMaintenanceAsync().Returns(false);
        flagService.IsDeployedFromCache(CacheKeys.BroadcastFlagCacheKey).Returns(false);
        flagService.GetProductTypeFromCache(CacheKeys.BroadcastProductCacheKey).Returns(string.Empty);
        manageQueueService.IsExistedItemInWaitingQueue(Arg.Any<string>()).Returns(true);
        manageQueueService.PublishToProcessing(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>()).Returns(new Result());

        var result = await service.Publisher(string.Empty, string.Empty);

        Assert.True(result.IsSuccessful);
        await manageQueueService.Received(2).ProcessWaitingQueue(Arg.Any<string>());
        await manageQueueService.DidNotReceive().PublishToWaiting(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>());
        await manageQueueService.Received().PublishToProcessing(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>());
    }

    [Fact]
    public async Task Publisher_IsDeployedAndNotInListProduct_CallPublishToProcessing()
    {
        const string product = "1,3,4,5";
        maintenanceService.IsUnderMaintenanceAsync().Returns(false);
        flagService.IsDeployedFromCache(CacheKeys.BroadcastFlagCacheKey).Returns(true);
        flagService.GetProductTypeFromCache(CacheKeys.BroadcastProductCacheKey).Returns(product);
        manageQueueService.IsExistedItemInWaitingQueue(Arg.Any<string>()).Returns(true);
        manageQueueService.PublishToProcessing(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>()).Returns(new Result());

        var result = await service.Publisher("9", string.Empty);

        Assert.True(result.IsSuccessful);
        await manageQueueService.Received().PublishToProcessing(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>());
        await manageQueueService.DidNotReceive().ProcessWaitingQueue(Arg.Any<string>());
        await manageQueueService.DidNotReceive().PublishToWaiting(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>());
    }
}