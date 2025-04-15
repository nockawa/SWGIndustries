using System.Xml.Serialization;

namespace SWGIndustries.Services;

public class XmlPlanet
{
    [XmlAttribute("swgaide_id")]
    public string SwgAideId { get; set; }

    [XmlElement("name")]
    public string Name { get; set; }
}