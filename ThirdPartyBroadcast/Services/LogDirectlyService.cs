namespace ThirdPartyBroadcast.Services;

public interface LogDirectlyService
{
    void WriteLog(int matchId, string message = null);
}

public class LogDirectlyServiceImp : LogDirectlyService
{
    private readonly IDynamicRepository dynamicRepository;

    public LogDirectlyServiceImp(IDynamicRepository dynamicRepository)
    {
        this.dynamicRepository = dynamicRepository;
    }

    public void WriteLog(int matchId, string message = null)
    {
        try
        {
            var criteria = new LogDirectlyCriteria
            {
                Extend = matchId,
                Doing = string.IsNullOrEmpty(message) ? $"Settlement for matchId: {matchId}" : message
            };

            dynamicRepository.Execute(criteria);
        }
        catch (Exception e)
        {
            Fanex.Logging.LogService.Error("Settlement.ThirdPartyBroadcastAPI(Write Directly):", e);
        }
    }
}