namespace ThirdPartyBroadcast.Constants;

public static class CacheKeys
{
    public const string BroadcastFlagCacheKey = "Settlement-Broadcast-API-Flag";

    public const string BroadcastProductCacheKey = "Settlement-Broadcast-API-Product-Type";

    public const string BroadcastApiProductWaitingCacheKey = "Broadcast-API-Product-Waiting";

    public const string BroadcastServiceProductWaitingCacheKey = "Broadcast-Service-Product-Waiting";

    public const string BroadcastSettleUnderMaintenanceCacheKey = "Settlement-Broadcast-Settle-UnderMaintenance";
}

public static class ActionName
{
    public const string MoveTicket = "MoveTicket";

    public const string Calculate = "CAL";
}