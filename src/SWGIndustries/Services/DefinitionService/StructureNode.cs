using System.Xml.Serialization;

namespace SWGIndustries.Services;

public interface IStructureNode
{
    int LotTaken { get; }
    int MaintenanceCost { get; }
}

public abstract class StructureNode : BaseNode, IStructureNode
{
    [XmlAttribute("LotTaken")]
    public virtual int LotTaken { get; set; }
    
    [XmlAttribute("MaintenanceCost")]
    public virtual int MaintenanceCost { get; set; }
}