using System.Xml.Serialization;

namespace SWGIndustries.Services;

[XmlRoot(ElementName = "resource_data", Namespace = "")]
public class XmlResourceData
{
    [XmlAttribute("server_id")]
    public string ServerId { get; set; }

    [XmlAttribute("server_name")]
    public string ServerName { get; set; }

    [XmlAttribute("generated")]
    public string GeneratedDateTime { get; set; }

    [XmlArray("resources")]
    [XmlArrayItem("resource")]
    public List<XmlResource> Resources { get; set; }
}