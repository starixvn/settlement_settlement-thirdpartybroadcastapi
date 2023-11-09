using ThirdPartyBroadcast.Criteria.NumberGame;

namespace ThirdPartyBroadcast;

public class ThirdPartyBroadcastMappingProfile : Profile
{
    public ThirdPartyBroadcastMappingProfile()
    {
        CreateMap<AutoNumberGameRequest, NumberGameCriteria>();
        CreateMap<AutoFastMarketRequest, FastMarketCriteria>();
        CreateMap<FastMarketCriteria, EventTicketFastMarketRequest>();
    }
}