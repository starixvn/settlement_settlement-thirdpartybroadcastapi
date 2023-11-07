using ThirdPartyBroadcast.Criteria.NumberGame;

namespace ThirdPartyBroadcast;

public class ThirdPartyBroadcastMappingProfile : Profile
{
    public ThirdPartyBroadcastMappingProfile()
    {
        CreateMap<AutoNumberGameRequest, NumberGameCriteria>();
        CreateMap<AutoFastMarketRequest, FastMarketCriteria>();
        CreateMap<AutoPinGoalRequest, UpdateMatchInfoRequest>();
        CreateMap<AutoPinGoalRequest, PinGoalCriteria>();
        CreateMap<FastMarketCriteria, EventTicketFastMarketRequest>();
        CreateMap<AutoVirtualSoccerRequest, UpdateMatchInfo3RdRequest>().ForMember(dest => dest.Creator, opt => opt.MapFrom(src => src.AdminId));
    }
}