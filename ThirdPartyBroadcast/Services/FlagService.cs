using Fanex.Caching;

namespace ThirdPartyBroadcast.Services;

public interface FlagService
{
    Task<bool> IsDeployedFromCache(string key);

    Task<string> GetProductTypeFromCache(string key);
}

public class FlagServiceImp : FlagService
{
    private readonly ICache cache;

    public FlagServiceImp(ICache cache)
    {
        this.cache = cache;
    }

    public async Task<bool> IsDeployedFromCache(string key)
    {
        try
        {
            var isExistedKey = await cache.ContainAsync(key);
            if (isExistedKey)
            {
                var (_, value) = await cache.TryGetAsync<bool>(key);
                return value;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    public async Task<string> GetProductTypeFromCache(string key)
    {
        try
        {
            var isExistedKey = await cache.ContainAsync(key);
            if (isExistedKey)
            {
                var (_, value) = await cache.TryGetAsync<string>(key);
                return value;
            }

            return string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }
}