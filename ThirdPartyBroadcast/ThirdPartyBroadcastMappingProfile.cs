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
            CreateMap<NumberGameRequest, NumberGameCriteria>();
            CreateMap<HappyFiveRequest, HappyFiveUpdateMatchCriteria>();
            CreateMap<FastMarketRequest, FastMarketCriteria>();
            CreateMap<HappyFiveRequest, HappyFiveUpdateMatchCriteria>();
            CreateMap<HappyFiveRequest, HappyFiveCriteria>();
            CreateMap<PinGoalRequest, UpdateMatchInfoRequest>();
            CreateMap<PinGoalRequest, PinGoalCriteria>();
            CreateMap<BitCoinRequest, BitCoinUpdateMatchCriteria>();
            CreateMap<BitCoinRequest, BitCoinCriteria>();
            CreateMap<FastMarketCriteria, EventTicketFastMarketRequest>().ForMember(dest => dest.SelectedResources, opt => opt.MapFrom(src => src.Results));
            CreateMap<VirtualBetRadarRequest, BetRadarUpdateMatchCriteria>().ForMember(dest => dest.Resources, opt => opt.MapFrom(src => src.ResourceResult));
            CreateMap<VirtualBetRadarRequest, VirtualBetRadarCriteria>();
            CreateMap<VirtualSoccerRequest, VirtualSoccerCriteria>();
            CreateMap<VirtualSoccerRequest, VRSoccerUpdateMatchCriteria>();
            CreateMap<NumberGameRequest, NumberGameUpdateMatchCriteria>();
            CreateMap<BetBuilderRequest, UpdateMarketCriteria>();
            CreateMap<BetBuilderRequest, BetBuilderCriteria>();
            CreateMap<OutrightRequest, OutRightCriteria>();
        }
    }
}