using System.Xml.Serialization;

namespace ThirdPartyBroadcast.Models;

[XmlRoot("Resources")]
public class ResourcesModel
{
    [XmlElement("rsc")]
    public List<ResourceItemModel> Items { get; set; }
}