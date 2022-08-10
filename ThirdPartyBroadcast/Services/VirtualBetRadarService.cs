using System.Threading.Tasks;
using ThirdPartyBroadcast.Models;
using ThirdPartyBroadcast.Requests;

namespace ThirdPartyBroadcast.Services
{
    public interface VirtualBetRadarService
    {
        Task<Result> Settle(VirtualBetRadarRequest request);
    }

    public class VirtualBetRadarServiceImp : VirtualBetRadarService
    {
        public async Task<Result> Settle(VirtualBetRadarRequest request)
        {
            return new Result();
        }
    }
}
