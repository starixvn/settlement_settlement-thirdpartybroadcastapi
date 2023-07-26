using System.Xml.Serialization;

namespace ThirdPartyBroadcast.Models;

public class ResourceMarketItemModel
{
    [XmlAttribute("m")]
    public string MarketId { get; set; }
}