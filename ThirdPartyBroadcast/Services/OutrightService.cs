using System.Threading.Tasks;
using ThirdPartyBroadcast.Models;
using ThirdPartyBroadcast.Requests;


namespace ThirdPartyBroadcast.Services
{
    public interface OutrightService
    {
        Task<Result> Settle(OutrightRequest request);
    }

    public class OutrightServiceImp : OutrightService
    {
        public async Task<Result> Settle(OutrightRequest request)
        {
            return new Result();
        }
    }
}
