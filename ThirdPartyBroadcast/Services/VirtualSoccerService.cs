using System.Threading.Tasks;
using ThirdPartyBroadcast.Models;
using ThirdPartyBroadcast.Requests;

namespace ThirdPartyBroadcast.Services
{
    public interface VirtualSoccerService
    {
        Task<Result> Settle(VirtualSoccerRequest request);
    }

    public class VirtualSoccerServiceImp : VirtualSoccerService
    {
        public async Task<Result> Settle(VirtualSoccerRequest request)
        {
            return new Result();
        }
    }
}
