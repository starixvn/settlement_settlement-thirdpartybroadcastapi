namespace ThirdPartyBroadcast.Criteria.BitCoin
{
    internal class BitCoinCriteria : CalculateCriteria
    {
        public string GameResult { get; set; }

        public int ResultId { get; set; }

        public override string GetSettingKey() => $"NAP_Admin_Settlement_s175_{DbServer}";
    }
}