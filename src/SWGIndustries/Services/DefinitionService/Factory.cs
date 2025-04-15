using System.Xml.Serialization;

namespace SWGIndustries.Services;

public interface IFactory
{
    int PowerConsumption { get; }
}

public class Factory : StructureNode, IFactory
{
    [XmlAttribute("PowerConsumption")]
    public int PowerConsumption { get; set; }
}