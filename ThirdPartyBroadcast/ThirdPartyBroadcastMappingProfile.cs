using AutoMapper;
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
        }
    }
}
