using System.Xml.Serialization;

namespace SWGIndustries.Services;

public interface IHarvesterClassNode
{
    int PowerConsumption { get; }
    int MinBER { get; }
    int MaxBER { get; }
    int MinHopperSizeK { get; }
    int MaxHopperSizeK { get; }
    int ReDeedCost { get; }
}

public class HarvesterClassNode : StructureNode, IHarvesterClassNode
{
    [XmlAttribute("PowerConsumption")]
    public int PowerConsumption { get; set; }
    
    [XmlAttribute("MinBER")]
    public int MinBER { get; set; }
    
    [XmlAttribute("MaxBER")]
    public int MaxBER { get; set; }
    
    [XmlAttribute("MinHopperSizeK")]
    public int MinHopperSizeK { get; set; }
    
    [XmlAttribute("MaxHopperSizeK")]
    public int MaxHopperSizeK { get; set; }
    
    [XmlAttribute("ReDeedCost")]
    public int ReDeedCost { get; set; }
}