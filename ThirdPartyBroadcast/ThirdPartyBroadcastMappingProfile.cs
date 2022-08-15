using AutoMapper;
using Fanex.Settlement.ScaleOut.Request;
using ThirdPartyBroadcast.Criteria;
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
            CreateMap<BitCoinRequest, BitCoinUpdateMatchCriteria>();
            CreateMap<BitCoinRequest, BitCoinCriteria>();
            CreateMap<FastMarketCriteria, EventTicketFastMarketRequest>().ForMember(dest => dest.SelectedResources, opt => opt.MapFrom(src => src.Results));
        }
    }
}