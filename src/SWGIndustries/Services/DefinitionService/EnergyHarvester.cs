using System.Xml.Serialization;

namespace SWGIndustries.Services;

public interface IEnergyHarvester
{
    HarvestingResourceType ResourceType { get; }
    int MinBER { get; }
    int MaxBER { get; }
    int MinHopperSizeK { get; }
    int MaxHopperSizeK { get; }
    int ReDeedCost { get; }
}

public class EnergyHarvester : StructureNode, IEnergyHarvester
{
    [XmlAttribute("ResourceType")]
    public HarvestingResourceType ResourceType { get; set; }

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