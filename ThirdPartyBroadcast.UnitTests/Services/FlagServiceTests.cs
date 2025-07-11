using Fanex.Caching;
using NSubstitute.ExceptionExtensions;

namespace ThirdPartyBroadcast.UnitTests.Services;

public class FlagServiceTests
{
    private readonly ICache cache;

    public FlagServiceTests()
    {
        cache = Substitute.For<ICache>();
    }

    [Fact]
    public async Task IsDeployedFromCache_NotExistedKey_ReturnFalse()
    {
        const string key = CacheKeys.BroadcastFlagCacheKey;
        cache.ContainAsync(key).Returns(false);
        var service = new FlagServiceImp(cache);

        var result = await service.GetStatusOnCache(key);

        Assert.False(result);
    }

    [Fact]
    public async Task IsDeployedFromCache_IsExistedKeyAndTrue_ReturnTrue()
    {
        const string key = CacheKeys.BroadcastFlagCacheKey;
        cache.ContainAsync(key).Returns(true);
        cache.TryGetAsync<bool>(key).Returns((false, true));

        var service = new FlagServiceImp(cache);

        var result = await service.GetStatusOnCache(key);

        Assert.True(result);
    }

    [Fact]
    public async Task IsDeployedFromCache_ExceptionGetCache_ReturnFalse()
    {
        cache.ContainAsync(Arg.Any<string>()).ThrowsAsync(new Exception());

        var service = new FlagServiceImp(cache);

        var result = await service.GetStatusOnCache(CacheKeys.BroadcastFlagCacheKey);

        Assert.False(result);
    }

    [Fact]
    public async Task GetProductTypeFromCache_NotExistedKey_ReturnEmpty()
    {
        const string key = CacheKeys.BroadcastFlagCacheKey;
        cache.ContainAsync(key).Returns(false);
        var service = new FlagServiceImp(cache);

        var result = await service.GetProductTypeOnCache(key);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetProductTypeFromCache_IsExistedKeyAndTrue_ReturnProductType()
    {
        const string key = CacheKeys.BroadcastFlagCacheKey;
        cache.ContainAsync(key).Returns(true);
        cache.TryGetAsync<string>(key).Returns((false, "1,2,3"));

        var service = new FlagServiceImp(cache);

        var result = await service.GetProductTypeOnCache(key);
        const string expectedProductType = "1,2,3";

        Assert.Equal(expectedProductType, result);
    }

    [Fact]
    public async Task GetProductTypeFromCache_ExceptionGetCache_ReturnEmpty()
    {
        cache.ContainAsync(Arg.Any<string>()).ThrowsAsync(new Exception());

        var service = new FlagServiceImp(cache);

        var result = await service.GetProductTypeOnCache(CacheKeys.BroadcastFlagCacheKey);

        Assert.Empty(result);
    }
}