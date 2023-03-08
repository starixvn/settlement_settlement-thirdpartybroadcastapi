namespace ThirdPartyBroadcast;

using Criteria.BitCoin;
using Criteria.HappyFive;
using Criteria.OutRight;
using Criteria.VirtualBetRadar;
using Criteria.VirtualSoccer;

public class ThirdPartyBroadcastMappingProfile : Profile
{
    public ThirdPartyBroadcastMappingProfile()
    {
        CreateMap<AutoNumberGameRequest, NumberGameCriteria>();
        CreateMap<AutoFastMarketRequest, FastMarketCriteria>();
        CreateMap<AutoHappyFiveRequest, HappyFiveCriteria>();
        CreateMap<AutoPinGoalRequest, UpdateMatchInfoRequest>();
        CreateMap<AutoPinGoalRequest, PinGoalCriteria>();
        CreateMap<AutoBitCoinRequest, BitCoinUpdateMatchCriteria>();
        CreateMap<AutoBitCoinRequest, BitCoinCriteria>();
        CreateMap<FastMarketCriteria, EventTicketFastMarketRequest>();
        CreateMap<AutoVirtualBetRadarRequest, BetRadarUpdateMatchCriteria>();
        CreateMap<AutoVirtualBetRadarRequest, VirtualBetRadarCriteria>();
        CreateMap<AutoVirtualSoccerRequest, VirtualSoccerCriteria>();
        CreateMap<AutoVirtualSoccerRequest, VRSoccerUpdateMatchCriteria>();
        CreateMap<AutoBetBuilderRequest, UpdateMarketCriteria>();
        CreateMap<AutoBetBuilderRequest, BetBuilderCriteria>();
        CreateMap<AutoOutrightRequest, OutRightCriteria>();
        CreateMap<WriteLogCriteria, SendMailCriteria>().ForMember(dest => dest.AdminId, opt => opt.MapFrom(src => src.UserId));
        CreateMap<AutoVirtualSoccerRequest, UpdateMatchInfo3rdRequest>().ForMember(dest => dest.Creator, opt => opt.MapFrom(src => src.AdminId));
        CreateMap<AutoVirtualBetRadarRequest, BetRadarValidationCriteria>();
    }
}