using AutoMapper;
using Fanex.Settlement.ScaleOut.Request;
using ThirdPartyBroadcast.Criteria;
using ThirdPartyBroadcast.Criteria.BetBuilder;
using ThirdPartyBroadcast.Criteria.BitCoin;
using ThirdPartyBroadcast.Criteria.HappyFive;
using ThirdPartyBroadcast.Criteria.NumberGame;
using ThirdPartyBroadcast.Criteria.OutRight;
using ThirdPartyBroadcast.Criteria.VirtualBetRadar;
using ThirdPartyBroadcast.Criteria.VirtualSoccer;
using ThirdPartyBroadcast.Requests;

namespace ThirdPartyBroadcast
{
    public class ThirdPartyBroadcastMappingProfile : Profile
    {
        public ThirdPartyBroadcastMappingProfile()
        {
            CreateMap<AutoNumberGameRequest, NumberGameCriteria>();
            CreateMap<AutoHappyFiveRequest, HappyFiveUpdateMatchCriteria>();
            CreateMap<AutoFastMarketRequest, FastMarketCriteria>();
            CreateMap<AutoHappyFiveRequest, HappyFiveUpdateMatchCriteria>();
            CreateMap<AutoHappyFiveRequest, HappyFiveCriteria>();
            CreateMap<AutoPinGoalRequest, UpdateMatchInfoRequest>();
            CreateMap<AutoPinGoalRequest, PinGoalCriteria>();
            CreateMap<AutoBitCoinRequest, BitCoinUpdateMatchCriteria>();
            CreateMap<AutoBitCoinRequest, BitCoinCriteria>();
            CreateMap<FastMarketCriteria, EventTicketFastMarketRequest>().ForMember(dest => dest.SelectedResources, opt => opt.MapFrom(src => src.Results));
            CreateMap<AutoVirtualBetRadarRequest, BetRadarUpdateMatchCriteria>().ForMember(dest => dest.Resources, opt => opt.MapFrom(src => src.ResourceResult));
            CreateMap<AutoVirtualBetRadarRequest, VirtualBetRadarCriteria>();
            CreateMap<AutoVirtualSoccerRequest, VirtualSoccerCriteria>();
            CreateMap<AutoVirtualSoccerRequest, VRSoccerUpdateMatchCriteria>();
            CreateMap<AutoNumberGameRequest, NumberGameUpdateMatchCriteria>();
            CreateMap<AutoBetBuilderRequest, UpdateMarketCriteria>();
            CreateMap<AutoBetBuilderRequest, BetBuilderCriteria>();
            CreateMap<AutoOutrightRequest, OutRightCriteria>();
        }
    }
}