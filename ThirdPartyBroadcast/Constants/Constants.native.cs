namespace ThirdPartyBroadcast.Constants;

public static class CacheKeys
{
    public const string BroadcastFlagCacheKey = "Settlement-Broadcast-API-Flag";

    public const string BroadcastProductCacheKey = "Settlement-Broadcast-API-Product-Type";

    public const string BroadcastApiProductWaitingCacheKey = "Broadcast-API-Product-Waiting";

    public const string BroadcastServiceProductWaitingCacheKey = "Broadcast-Service-Product-Waiting";
}

public static class SpName
{
    public const string MoveTicket = "NAP_Admin_Settlement_50_MoveTicket";
}

public static class ActionType
{
    public const string Calculate = "cal";
}

public static class ActionName
{
    public const string MoveTicket = "MoveTicket";

    public const string Calculate = "CAL";
}

public static class ActionId
{
    public const int MoveTicketFailed = 81026;

    public const int MoveTicket = 81025;
}