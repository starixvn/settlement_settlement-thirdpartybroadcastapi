using System.Collections.Generic;
using System.Xml.Serialization;

namespace ThirdPartyBroadcast.Models
{
    [XmlRoot("Balls")]
    public class BallResults
    {
        [XmlElement("Ball")]
        public List<BallModel> BallResultsItems { get; set; }
    }
}
