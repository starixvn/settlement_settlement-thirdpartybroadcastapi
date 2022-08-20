using Fanex.Data.Repository;

namespace ThirdPartyBroadcast.Criteria.VirtualSoccer
{
    public class VirtualSoccerValidationEventCriteria : NonQueryCommand
    {
        public int MatchId { get; set; }

        public int ErrCode { get; set; }

        public string ErrMsg { get; set; }

        public override string GetSettingKey()
        {
            return "NAP_Admin_Settlement_VS1_Validation_Event";
        }

        public override bool IsValid()
        {
            return MatchId > 0;
        }
    }
}