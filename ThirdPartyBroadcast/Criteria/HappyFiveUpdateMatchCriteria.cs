using Fanex.Data.Repository;

namespace ThirdPartyBroadcast.Criteria
{
    public class HappyFiveUpdateMatchCriteria : NonQueryCommand
    {
        public int MatchId { get; set; }

        public int AdminId { get; set; }

        public string Action { get; set; }

        public bool IsAuto { get; set; }

        public string Resources { get; set; }


        public override string GetSettingKey() => "NAP_Admin_Match_s164_Upd";

        public override bool IsValid() => MatchId > 0;
    }
}
