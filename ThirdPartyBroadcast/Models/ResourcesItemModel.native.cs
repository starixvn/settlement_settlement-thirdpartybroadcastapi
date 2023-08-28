using System.Xml.Serialization;

namespace ThirdPartyBroadcast.Models;

[XmlRoot("Resources")]
public class ResourcesModel
{
    [XmlElement("Resource")]
    public List<ResourceItemModel> Items { get; set; }
}