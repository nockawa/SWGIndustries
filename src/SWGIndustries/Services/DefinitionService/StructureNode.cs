using System.Xml.Serialization;

namespace SWGIndustries.Services;

public abstract class StructureNode : BaseNode
{
    [XmlAttribute("LotTaken")]
    public virtual int LotTaken { get; set; }
    
    [XmlAttribute("MaintenanceCost")]
    public virtual int MaintenanceCost { get; set; }
}