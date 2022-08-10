using System.Threading.Tasks;
using ThirdPartyBroadcast.Models;
using ThirdPartyBroadcast.Requests;


namespace ThirdPartyBroadcast.Services
{
    public interface PinGoalService
    {
        Task<Result> Settle(PinGoalRequest request);
    }

    public class PinGoalServiceImp : PinGoalService
    {
        public async Task<Result> Settle(PinGoalRequest request)
        {
            return new Result();
        }
    }
}
