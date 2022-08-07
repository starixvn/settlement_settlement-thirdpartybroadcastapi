using System.Collections.Generic;
using System.Xml.Serialization;

namespace ThirdPartyBroadcast.Models
{
    [XmlRoot("Resources")]
    public class Resources
    {
        [XmlElement("Resource")]
        public List<ResourcesItem> ResourceItems { get; set; }
    }
}
