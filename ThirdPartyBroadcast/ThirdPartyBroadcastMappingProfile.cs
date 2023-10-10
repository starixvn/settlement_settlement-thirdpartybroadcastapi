using ThirdPartyBroadcast.Criteria.NumberGame;

namespace ThirdPartyBroadcast;

using Criteria.HappyFive;

public class ThirdPartyBroadcastMappingProfile : Profile
{
    public ThirdPartyBroadcastMappingProfile()
    {
        CreateMap<AutoNumberGameRequest, NumberGameCriteria>();
        CreateMap<AutoFastMarketRequest, FastMarketCriteria>();
        CreateMap<AutoHappyFiveRequest, HappyFiveCriteria>();
        CreateMap<AutoPinGoalRequest, UpdateMatchInfoRequest>();
        CreateMap<AutoPinGoalRequest, PinGoalCriteria>();
        CreateMap<FastMarketCriteria, EventTicketFastMarketRequest>();
        CreateMap<AutoBetBuilderRequest, UpdateMarketCriteria>();
        CreateMap<AutoBetBuilderRequest, BetBuilderCriteria>();
        CreateMap<AutoVirtualSoccerRequest, UpdateMatchInfo3RdRequest>().ForMember(dest => dest.Creator, opt => opt.MapFrom(src => src.AdminId));
        CreateMap<LeapGamingRequest, UpdateMatchInfoRequest>();
        CreateMap<LeapGamingRequest, UpdateMatchCriteria>();
    }
}