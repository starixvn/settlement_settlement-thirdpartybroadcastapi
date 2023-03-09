using System.Xml.Serialization;

namespace ThirdPartyBroadcast.Models;

public class ResourceItemModel
{
    [XmlAttribute("m")]
    public string MarketId { get; set; }
}