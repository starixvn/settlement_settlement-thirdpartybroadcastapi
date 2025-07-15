using Fanex.Caching;

namespace ThirdPartyBroadcast.Services;

public interface FlagService
{
    Task<bool> GetStatusOnCache(string key);

    Task<string> GetProductTypeOnCache(string key);
}

public class FlagServiceImp(ICache cache) : FlagService
{
    public async Task<bool> GetStatusOnCache(string key)
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

    public async Task<string> GetProductTypeOnCache(string key)
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