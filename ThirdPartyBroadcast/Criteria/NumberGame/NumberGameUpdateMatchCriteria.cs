using Fanex.Data.Repository;

namespace ThirdPartyBroadcast.Criteria.NumberGame
{
    public class NumberGameUpdateMatchCriteria : NonQueryCommand
    {
        public int MatchId { get; set; }

        public int AdminId { get; set; }

        public string Action { get; set; }

        public bool IsAuto { get; set; }

        public override string GetSettingKey() => "NAP_Admin_Match_NumberGame_Upd";

        public override bool IsValid() => MatchId > 0;
    }
}