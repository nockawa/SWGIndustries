using System.Xml.Serialization;

namespace SWGIndustries.Services;

[XmlRoot(ElementName = "resource", Namespace = "")]
public class XmlResource
{
    [XmlAttribute("swgaide_id")]
    public string SwgAideId { get; set; }

    [XmlElement("name")]
    public string Name { get; set; }

    [XmlElement("type")]
    public string Type { get; set; }

    [XmlElement("swgaide_type_id")]
    public string SwgAideTypeId { get; set; }

    [XmlElement("stats")]
    public XmlStats Stats { get; set; }

    [XmlArray("planets")]
    [XmlArrayItem("planet")]
    public List<XmlPlanet> Planets { get; set; }

    [XmlElement("waypoints")]
    public string Waypoints { get; set; }

    [XmlElement("available_timestamp")]
    public long AvailableTimestamp { get; set; }

    [XmlElement("available_by")]
    public string AvailableBy { get; set; }
}