using System.Xml.Serialization;

namespace SWGIndustries.Services;

public class XmlWaypoint
{
    [XmlAttribute("swgaide_id")]
    public string SWGAideId { get; set; }

    [XmlAttribute("timestamp")]
    public int Timestamp { get; set; }

    [XmlElement("planet")]
    public string Planet { get; set; }

    [XmlElement("x")]
    public int X { get; set; }

    [XmlElement("y")]
    public int Y { get; set; }

    [XmlElement("z")]
    public int Z { get; set; }

    [XmlElement("con")]
    public int Concentration { get; set; }

    [XmlElement("remarks")]
    public string Remarks { get; set; }

    [XmlElement("wptext")]
    public string WaypointAsText { get; set; }
}