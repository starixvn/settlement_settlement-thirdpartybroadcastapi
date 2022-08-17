using System.Xml.Serialization;

namespace ThirdPartyBroadcast.Models
{
    public class ResourcesItem
    {
        [XmlAttribute("m")]
        public int MarketId { get; set; }
    }
}