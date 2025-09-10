using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using ONEbook.UM.Services;

namespace ThirdPartyBroadcast.UnitTests.Services;

public class PublishMessageServiceTests
{
    private readonly FlagService flagService;

    private readonly ManageQueueService manageQueueService;

    private readonly IMaintenanceService maintenanceService;

    private readonly PublishMessageServiceImp service;

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
        flagService.GetStatusOnCache(CacheKeys.BroadcastFlagCacheKey).Returns(false);
        flagService.GetProductTypeOnCache(CacheKeys.BroadcastProductCacheKey).Returns(string.Empty);
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
        flagService.GetStatusOnCache(CacheKeys.BroadcastFlagCacheKey).Returns(true);
        flagService.GetProductTypeOnCache(CacheKeys.BroadcastProductCacheKey).Returns(product);
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
        flagService.GetStatusOnCache(CacheKeys.BroadcastFlagCacheKey).Returns(false);
        flagService.GetProductTypeOnCache(CacheKeys.BroadcastProductCacheKey).Returns(string.Empty);
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
        flagService.GetStatusOnCache(CacheKeys.BroadcastFlagCacheKey).Returns(true);
        flagService.GetProductTypeOnCache(CacheKeys.BroadcastProductCacheKey).Returns(product);
        manageQueueService.IsExistedItemInWaitingQueue(Arg.Any<string>()).Returns(true);
        manageQueueService.PublishToProcessing(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>()).Returns(new Result());

        var result = await service.Publisher("9", string.Empty);

        Assert.True(result.IsSuccessful);
        await manageQueueService.Received().PublishToProcessing(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>());
        await manageQueueService.DidNotReceive().ProcessWaitingQueue(Arg.Any<string>());
        await manageQueueService.DidNotReceive().PublishToWaiting(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>());
    }

    [Fact]
    public async Task Publisher_IsMaintenanceAndFlagUnderMaintenance_CallPublishToProcessing()
    {
        maintenanceService.IsUnderMaintenanceAsync().Returns(true);
        flagService.GetStatusOnCache(CacheKeys.BroadcastFlagCacheKey).Returns(false);
        flagService.GetStatusOnCache(CacheKeys.BroadcastSettleUnderMaintenanceCacheKey).Returns(true);
        flagService.GetProductTypeOnCache(CacheKeys.BroadcastProductCacheKey).Returns(string.Empty);
        manageQueueService.IsExistedItemInWaitingQueue(Arg.Any<string>()).Returns(false);
        manageQueueService.PublishToProcessing(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>()).Returns(new Result());

        var result = await service.Publisher("1", "test data");

        Assert.True(result.IsSuccessful);
        await manageQueueService.Received().PublishToProcessing(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>());
        await manageQueueService.DidNotReceive().PublishToWaiting(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>());
    }

    [Fact]
    public async Task Publisher_IsMaintenanceAndNotFlagUnderMaintenance_CallPublishToWaiting()
    {
        maintenanceService.IsUnderMaintenanceAsync().Returns(true);
        flagService.GetStatusOnCache(CacheKeys.BroadcastFlagCacheKey).Returns(false);
        flagService.GetStatusOnCache(CacheKeys.BroadcastSettleUnderMaintenanceCacheKey).Returns(false);
        flagService.GetProductTypeOnCache(CacheKeys.BroadcastProductCacheKey).Returns(string.Empty);
        manageQueueService.PublishToWaiting(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>()).Returns(new Result());

        var result = await service.Publisher("1", "test data");

        Assert.True(result.IsSuccessful);
        await manageQueueService.Received().PublishToWaiting(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>());
        await manageQueueService.DidNotReceive().PublishToProcessing(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>());
    }

    [Fact]
    public async Task Publisher_ProductTypeOnCacheIsNull_ThrowsException()
    {
        maintenanceService.IsUnderMaintenanceAsync().Returns(false);
        flagService.GetStatusOnCache(CacheKeys.BroadcastFlagCacheKey).Returns(true);
        flagService.GetProductTypeOnCache(CacheKeys.BroadcastProductCacheKey).Returns((string)null);

        await Assert.ThrowsAsync<ArgumentNullException>(() => service.Publisher("1", "test data"));
    }

    [Fact]
    public async Task Publisher_ProductTypeOnCacheIsEmpty_CallPublishToProcessing()
    {
        maintenanceService.IsUnderMaintenanceAsync().Returns(false);
        flagService.GetStatusOnCache(CacheKeys.BroadcastFlagCacheKey).Returns(true);
        flagService.GetProductTypeOnCache(CacheKeys.BroadcastProductCacheKey).Returns(string.Empty);
        manageQueueService.IsExistedItemInWaitingQueue(Arg.Any<string>()).Returns(false);
        manageQueueService.PublishToProcessing(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>()).Returns(new Result());

        var result = await service.Publisher("1", "test data");

        Assert.True(result.IsSuccessful);
        await manageQueueService.Received().PublishToProcessing(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>());
        await manageQueueService.DidNotReceive().PublishToWaiting(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>());
    }

    [Fact]
    public async Task Publisher_IsNotDeployedAndNoWaitingItems_CallPublishToProcessing()
    {
        maintenanceService.IsUnderMaintenanceAsync().Returns(false);
        flagService.GetStatusOnCache(CacheKeys.BroadcastFlagCacheKey).Returns(false);
        flagService.GetProductTypeOnCache(CacheKeys.BroadcastProductCacheKey).Returns(string.Empty);
        manageQueueService.IsExistedItemInWaitingQueue(Arg.Any<string>()).Returns(false);
        manageQueueService.PublishToProcessing(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>()).Returns(new Result());

        var result = await service.Publisher("1", "test data");

        Assert.True(result.IsSuccessful);
        await manageQueueService.DidNotReceive().ProcessWaitingQueue(Arg.Any<string>());
        await manageQueueService.Received().PublishToProcessing(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>());
        await manageQueueService.DidNotReceive().PublishToWaiting(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>());
    }

    [Fact]
    public async Task Publisher_IsNotDeployedAndOnlyServiceWaitingItems_CallProcessWaitingQueueForService()
    {
        maintenanceService.IsUnderMaintenanceAsync().Returns(false);
        flagService.GetStatusOnCache(CacheKeys.BroadcastFlagCacheKey).Returns(false);
        flagService.GetProductTypeOnCache(CacheKeys.BroadcastProductCacheKey).Returns(string.Empty);
        manageQueueService.IsExistedItemInWaitingQueue(CacheKeys.BroadcastServiceProductWaitingCacheKey).Returns(true);
        manageQueueService.IsExistedItemInWaitingQueue(CacheKeys.BroadcastApiProductWaitingCacheKey).Returns(false);
        manageQueueService.PublishToProcessing(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>()).Returns(new Result());

        var result = await service.Publisher("1", "test data");

        Assert.True(result.IsSuccessful);
        await manageQueueService.Received().ProcessWaitingQueue(CacheKeys.BroadcastServiceProductWaitingCacheKey);
        await manageQueueService.DidNotReceive().ProcessWaitingQueue(CacheKeys.BroadcastApiProductWaitingCacheKey);
        await manageQueueService.Received().PublishToProcessing(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>());
    }

    [Fact]
    public async Task Publisher_IsNotDeployedAndOnlyApiWaitingItems_CallProcessWaitingQueueForApi()
    {
        maintenanceService.IsUnderMaintenanceAsync().Returns(false);
        flagService.GetStatusOnCache(CacheKeys.BroadcastFlagCacheKey).Returns(false);
        flagService.GetProductTypeOnCache(CacheKeys.BroadcastProductCacheKey).Returns(string.Empty);
        manageQueueService.IsExistedItemInWaitingQueue(CacheKeys.BroadcastServiceProductWaitingCacheKey).Returns(false);
        manageQueueService.IsExistedItemInWaitingQueue(CacheKeys.BroadcastApiProductWaitingCacheKey).Returns(true);
        manageQueueService.PublishToProcessing(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>()).Returns(new Result());

        var result = await service.Publisher("1", "test data");

        Assert.True(result.IsSuccessful);
        await manageQueueService.DidNotReceive().ProcessWaitingQueue(CacheKeys.BroadcastServiceProductWaitingCacheKey);
        await manageQueueService.Received().ProcessWaitingQueue(CacheKeys.BroadcastApiProductWaitingCacheKey);
        await manageQueueService.Received().PublishToProcessing(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>());
    }

    [Fact]
    public async Task Publisher_RetryAndPublishToWaiting_SuccessOnFirstAttempt()
    {
        maintenanceService.IsUnderMaintenanceAsync().Returns(true);
        flagService.GetStatusOnCache(CacheKeys.BroadcastFlagCacheKey).Returns(false);
        flagService.GetStatusOnCache(CacheKeys.BroadcastSettleUnderMaintenanceCacheKey).Returns(false);
        flagService.GetProductTypeOnCache(CacheKeys.BroadcastProductCacheKey).Returns(string.Empty);
        manageQueueService.PublishToWaiting(Arg.Any<string>(), Arg.Any<string>(), 1).Returns(new Result());

        var result = await service.Publisher("1", "test data");

        Assert.True(result.IsSuccessful);
        await manageQueueService.Received(1).PublishToWaiting(Arg.Any<string>(), Arg.Any<string>(), 1);
        await manageQueueService.DidNotReceive().PublishToWaiting(Arg.Any<string>(), Arg.Any<string>(), 2);
    }

    [Fact]
    public async Task Publisher_RetryAndPublishToWaiting_SuccessOnSecondAttempt()
    {
        var appSettingsKey = new AppSettingsKey { SiteDbServer = ["01", "02"], RetryCount = 2 };
        var options = Options.Create(appSettingsKey);
        var retryService = new PublishMessageServiceImp(flagService, manageQueueService, options, maintenanceService);
        
        maintenanceService.IsUnderMaintenanceAsync().Returns(true);
        flagService.GetStatusOnCache(CacheKeys.BroadcastFlagCacheKey).Returns(false);
        flagService.GetStatusOnCache(CacheKeys.BroadcastSettleUnderMaintenanceCacheKey).Returns(false);
        flagService.GetProductTypeOnCache(CacheKeys.BroadcastProductCacheKey).Returns(string.Empty);
        manageQueueService.PublishToWaiting(Arg.Any<string>(), Arg.Any<string>(), 1).Returns(new Result { ErrorMessage = "Error", ErrorCode = 1 });
        manageQueueService.PublishToWaiting(Arg.Any<string>(), Arg.Any<string>(), 2).Returns(new Result());

        var result = await retryService.Publisher("1", "test data");

        Assert.True(result.IsSuccessful);
        await manageQueueService.Received().PublishToWaiting(Arg.Any<string>(), Arg.Any<string>(), 1);
        await manageQueueService.Received().PublishToWaiting(Arg.Any<string>(), Arg.Any<string>(), 2);
        await manageQueueService.DidNotReceive().PublishToWaiting(Arg.Any<string>(), Arg.Any<string>(), 3);
    }

    [Fact]
    public async Task Publisher_RetryAndPublishToWaiting_FailsAllAttempts()
    {
        maintenanceService.IsUnderMaintenanceAsync().Returns(true);
        flagService.GetStatusOnCache(CacheKeys.BroadcastFlagCacheKey).Returns(false);
        flagService.GetStatusOnCache(CacheKeys.BroadcastSettleUnderMaintenanceCacheKey).Returns(false);
        flagService.GetProductTypeOnCache(CacheKeys.BroadcastProductCacheKey).Returns(string.Empty);
        manageQueueService.PublishToWaiting(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>()).Returns(new Result { ErrorMessage = "Error", ErrorCode = 1 });

        var result = await service.Publisher("1", "test data");

        Assert.False(result.IsSuccessful);
        await manageQueueService.Received(1).PublishToWaiting(Arg.Any<string>(), Arg.Any<string>(), 1);
    }

    [Fact]
    public async Task Publisher_RetryAndPublishToProcessing_SuccessOnFirstAttempt()
    {
        maintenanceService.IsUnderMaintenanceAsync().Returns(false);
        flagService.GetStatusOnCache(CacheKeys.BroadcastFlagCacheKey).Returns(true);
        flagService.GetProductTypeOnCache(CacheKeys.BroadcastProductCacheKey).Returns("1,2,3");
        manageQueueService.IsExistedItemInWaitingQueue(Arg.Any<string>()).Returns(false);
        manageQueueService.PublishToProcessing(Arg.Any<string>(), Arg.Any<string>(), 1).Returns(new Result());

        var result = await service.Publisher("9", "test data");

        Assert.True(result.IsSuccessful);
        await manageQueueService.Received(1).PublishToProcessing(Arg.Any<string>(), Arg.Any<string>(), 1);
        await manageQueueService.DidNotReceive().PublishToProcessing(Arg.Any<string>(), Arg.Any<string>(), 2);
    }

    [Fact]
    public async Task Publisher_RetryAndPublishToProcessing_SuccessOnSecondAttempt()
    {
        var appSettingsKey = new AppSettingsKey { SiteDbServer = ["01", "02"], RetryCount = 2 };
        var options = Options.Create(appSettingsKey);
        var retryService = new PublishMessageServiceImp(flagService, manageQueueService, options, maintenanceService);
        
        maintenanceService.IsUnderMaintenanceAsync().Returns(false);
        flagService.GetStatusOnCache(CacheKeys.BroadcastFlagCacheKey).Returns(true);
        flagService.GetProductTypeOnCache(CacheKeys.BroadcastProductCacheKey).Returns("1,2,3");
        manageQueueService.IsExistedItemInWaitingQueue(Arg.Any<string>()).Returns(false);
        manageQueueService.PublishToProcessing(Arg.Any<string>(), Arg.Any<string>(), 1).Returns(new Result { ErrorMessage = "Error", ErrorCode = 1 });
        manageQueueService.PublishToProcessing(Arg.Any<string>(), Arg.Any<string>(), 2).Returns(new Result());

        var result = await retryService.Publisher("9", "test data");

        Assert.True(result.IsSuccessful);
        await manageQueueService.Received().PublishToProcessing(Arg.Any<string>(), Arg.Any<string>(), 1);
        await manageQueueService.Received().PublishToProcessing(Arg.Any<string>(), Arg.Any<string>(), 2);
        await manageQueueService.DidNotReceive().PublishToProcessing(Arg.Any<string>(), Arg.Any<string>(), 3);
    }

    [Fact]
    public async Task Publisher_RetryAndPublishToProcessing_FailsAllAttempts()
    {
        maintenanceService.IsUnderMaintenanceAsync().Returns(false);
        flagService.GetStatusOnCache(CacheKeys.BroadcastFlagCacheKey).Returns(true);
        flagService.GetProductTypeOnCache(CacheKeys.BroadcastProductCacheKey).Returns("1,2,3");
        manageQueueService.IsExistedItemInWaitingQueue(Arg.Any<string>()).Returns(false);
        manageQueueService.PublishToProcessing(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>()).Returns(new Result { ErrorMessage = "Error", ErrorCode = 1 });

        var result = await service.Publisher("9", "test data");

        Assert.False(result.IsSuccessful);
        await manageQueueService.Received(1).PublishToProcessing(Arg.Any<string>(), Arg.Any<string>(), 1);
    }

    [Fact]
    public async Task Publisher_ProductTypeListContainsProductType_CallPublishToWaiting()
    {
        const string product = "1,3,4,5";
        maintenanceService.IsUnderMaintenanceAsync().Returns(false);
        flagService.GetStatusOnCache(CacheKeys.BroadcastFlagCacheKey).Returns(true);
        flagService.GetProductTypeOnCache(CacheKeys.BroadcastProductCacheKey).Returns(product);
        manageQueueService.PublishToWaiting(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>()).Returns(new Result());

        var result = await service.Publisher("4", "test data");

        Assert.True(result.IsSuccessful);
        await manageQueueService.Received().PublishToWaiting(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>());
        await manageQueueService.DidNotReceive().PublishToProcessing(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>());
    }

    [Fact]
    public async Task Publisher_ProductTypeListDoesNotContainProductType_CallPublishToProcessing()
    {
        const string product = "1,3,4,5";
        maintenanceService.IsUnderMaintenanceAsync().Returns(false);
        flagService.GetStatusOnCache(CacheKeys.BroadcastFlagCacheKey).Returns(true);
        flagService.GetProductTypeOnCache(CacheKeys.BroadcastProductCacheKey).Returns(product);
        manageQueueService.IsExistedItemInWaitingQueue(Arg.Any<string>()).Returns(false);
        manageQueueService.PublishToProcessing(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>()).Returns(new Result());

        var result = await service.Publisher("6", "test data");

        Assert.True(result.IsSuccessful);
        await manageQueueService.Received().PublishToProcessing(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>());
        await manageQueueService.DidNotReceive().PublishToWaiting(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>());
    }

    [Fact]
    public async Task Publisher_ProductTypeOnCacheWithSpaces_CallPublishToProcessing()
    {
        const string product = " 1 , 3 , 4 , 5 ";
        maintenanceService.IsUnderMaintenanceAsync().Returns(false);
        flagService.GetStatusOnCache(CacheKeys.BroadcastFlagCacheKey).Returns(true);
        flagService.GetProductTypeOnCache(CacheKeys.BroadcastProductCacheKey).Returns(product);
        manageQueueService.IsExistedItemInWaitingQueue(Arg.Any<string>()).Returns(false);
        manageQueueService.PublishToProcessing(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>()).Returns(new Result());

        var result = await service.Publisher("3", "test data");

        Assert.True(result.IsSuccessful);
        await manageQueueService.Received().PublishToProcessing(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>());
        await manageQueueService.DidNotReceive().PublishToWaiting(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>());
    }

    [Fact]
    public async Task Publisher_ProductTypeOnCacheWithEmptyElements_CallPublishToWaiting()
    {
        const string product = "1,,3,4,5";
        maintenanceService.IsUnderMaintenanceAsync().Returns(false);
        flagService.GetStatusOnCache(CacheKeys.BroadcastFlagCacheKey).Returns(true);
        flagService.GetProductTypeOnCache(CacheKeys.BroadcastProductCacheKey).Returns(product);
        manageQueueService.PublishToWaiting(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>()).Returns(new Result());

        var result = await service.Publisher("", "test data");

        Assert.True(result.IsSuccessful);
        await manageQueueService.Received().PublishToWaiting(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>());
        await manageQueueService.DidNotReceive().PublishToProcessing(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>());
    }

    [Fact]
    public async Task Publisher_ProductTypeOnCacheSingleElement_CallPublishToWaiting()
    {
        const string product = "1";
        maintenanceService.IsUnderMaintenanceAsync().Returns(false);
        flagService.GetStatusOnCache(CacheKeys.BroadcastFlagCacheKey).Returns(true);
        flagService.GetProductTypeOnCache(CacheKeys.BroadcastProductCacheKey).Returns(product);
        manageQueueService.PublishToWaiting(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>()).Returns(new Result());

        var result = await service.Publisher("1", "test data");

        Assert.True(result.IsSuccessful);
        await manageQueueService.Received().PublishToWaiting(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>());
        await manageQueueService.DidNotReceive().PublishToProcessing(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>());
    }

    [Fact]
    public async Task Publisher_ProductTypeOnCacheSingleElementNotMatch_CallPublishToProcessing()
    {
        const string product = "1";
        maintenanceService.IsUnderMaintenanceAsync().Returns(false);
        flagService.GetStatusOnCache(CacheKeys.BroadcastFlagCacheKey).Returns(true);
        flagService.GetProductTypeOnCache(CacheKeys.BroadcastProductCacheKey).Returns(product);
        manageQueueService.IsExistedItemInWaitingQueue(Arg.Any<string>()).Returns(false);
        manageQueueService.PublishToProcessing(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>()).Returns(new Result());

        var result = await service.Publisher("2", "test data");

        Assert.True(result.IsSuccessful);
        await manageQueueService.Received().PublishToProcessing(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>());
        await manageQueueService.DidNotReceive().PublishToWaiting(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>());
    }

    [Fact]
    public async Task Publisher_ComplexMaintenanceScenario_CallPublishToWaiting()
    {
        maintenanceService.IsUnderMaintenanceAsync().Returns(true);
        flagService.GetStatusOnCache(CacheKeys.BroadcastFlagCacheKey).Returns(true);
        flagService.GetStatusOnCache(CacheKeys.BroadcastSettleUnderMaintenanceCacheKey).Returns(false);
        flagService.GetProductTypeOnCache(CacheKeys.BroadcastProductCacheKey).Returns("1,2,3");
        manageQueueService.PublishToWaiting(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>()).Returns(new Result());

        var result = await service.Publisher("2", "test data");

        Assert.True(result.IsSuccessful);
        await manageQueueService.Received().PublishToWaiting(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>());
        await manageQueueService.DidNotReceive().PublishToProcessing(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>());
    }

    [Fact]
    public async Task Publisher_ComplexMaintenanceScenarioWithFlag_CallPublishToWaiting()
    {
        maintenanceService.IsUnderMaintenanceAsync().Returns(true);
        flagService.GetStatusOnCache(CacheKeys.BroadcastFlagCacheKey).Returns(true);
        flagService.GetStatusOnCache(CacheKeys.BroadcastSettleUnderMaintenanceCacheKey).Returns(true);
        flagService.GetProductTypeOnCache(CacheKeys.BroadcastProductCacheKey).Returns("1,2,3");
        manageQueueService.PublishToWaiting(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>()).Returns(new Result());

        var result = await service.Publisher("2", "test data");

        Assert.True(result.IsSuccessful);
        await manageQueueService.Received().PublishToWaiting(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>());
        await manageQueueService.DidNotReceive().PublishToProcessing(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>());
    }
}