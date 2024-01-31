using Fanex.Logging;

namespace ThirdPartyBroadcast.Services;

public interface LogDirectlyService
{
    Task WriteLog(int matchId, string message = null);
}

public class LogDirectlyServiceImp(IDynamicRepository dynamicRepository) : LogDirectlyService
{
    public async Task WriteLog(int matchId, string message = null)
    {
        try
        {
            var messageDoing = string.IsNullOrWhiteSpace(message) ? $"Settlement for matchId: {matchId}" : message;
            var criteria = new LogDirectlyCriteria
            {
                Extend = matchId,
                Doing = messageDoing
            };

            await dynamicRepository.ExecuteAsync(criteria);
        }
        catch (Exception e)
        {
            await LogService.ErrorAsync("Settlement.ThirdPartyBroadcastAPI(Write Directly):", e);
        }
    }
}