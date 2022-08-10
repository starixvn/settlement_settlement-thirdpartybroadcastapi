using System.Threading.Tasks;
using ThirdPartyBroadcast.Models;
using ThirdPartyBroadcast.Requests;

namespace ThirdPartyBroadcast.Services
{
    public interface FastMarketService
    {
        Task<Result> Settle(FastMarketRequest request);
    }

    public class FastMarketServiceImp : FastMarketService
    {
        public async Task<Result> Settle(FastMarketRequest request)
        {
            return new Result();
        }
    }
}
