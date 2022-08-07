using System.Xml.Serialization;

namespace ThirdPartyBroadcast.Models
{
    public class BallModel
    {
        public BallModel()
        {
        }

        public BallModel(int matchId, byte seq, byte numberBall)
        {
            MatchId = matchId;
            Seq = seq;
            NumberBall = numberBall;
        }

        [XmlIgnore]
        public int MatchId { get; set; }
        
        [XmlAttribute("Seq")]
        public byte Seq { get; set; }

        [XmlAttribute("Number")]
        public byte NumberBall { get; set; }
    }
}
