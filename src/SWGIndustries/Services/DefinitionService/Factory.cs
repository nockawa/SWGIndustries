using System.Xml.Serialization;

namespace SWGIndustries.Services;

public class Factory : StructureNode
{
    [XmlAttribute("PowerConsumption")]
    public int PowerConsumption { get; set; }
}