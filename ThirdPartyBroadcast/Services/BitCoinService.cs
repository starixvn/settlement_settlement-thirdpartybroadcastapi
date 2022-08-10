using System.Threading.Tasks;
using ThirdPartyBroadcast.Models;
using ThirdPartyBroadcast.Requests;

namespace ThirdPartyBroadcast.Services
{
    public interface BitCoinService
    {
        Task<Result> Settle(BitCoinRequest request);
    }

    public class BitCoinServiceImp : BitCoinService
    {
        public async Task<Result> Settle(BitCoinRequest request)
        {
            return new Result();
        }
    }
}
