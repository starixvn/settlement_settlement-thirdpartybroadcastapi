using System.Xml.Serialization;

namespace ThirdPartyBroadcast.Models
{
    public class ResourcesItem
    {
        [XmlAttribute("Id")]
        public byte Seq { get; set; }

        [XmlAttribute("Result")]
        public byte NumberBall { get; set; }
    }
}
