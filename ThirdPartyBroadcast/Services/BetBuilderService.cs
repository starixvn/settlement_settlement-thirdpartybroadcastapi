using System.Threading.Tasks;
using ThirdPartyBroadcast.Models;
using ThirdPartyBroadcast.Requests;


namespace ThirdPartyBroadcast.Services
{
    public interface BetBuilderService
    {
        Task<Result> Settle(BetBuilderRequest request);
    }

    public class BetBuilderServiceImp : BetBuilderService
    {
        public async Task<Result> Settle(BetBuilderRequest request)
        {
            return new Result();
        }
    }
}
