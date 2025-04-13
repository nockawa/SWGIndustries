using System.Diagnostics;
using System.Xml.Serialization;

namespace SWGIndustries.Services;

[DebuggerDisplay("{FullClass}")]
[XmlRoot("Node", Namespace = "http://swgindustries.com/swgstructures")]
public class BaseNode
{
    private string _fullClass;
    private string[] _classes;

    [XmlAttribute("Class")] 
    public string Class { get; set; }

    [XmlIgnore]
    public string FullClass => _fullClass ??= ((Parent==null || Parent.FullClass == "Root") ? Class : $"{Parent.FullClass}.{Class}");

    public string GetClass(int index)
    {
        _classes ??= FullClass.Split('.');

        if (index < 0)
        {
            index = _classes.Length + index;
        }
        
        if (index < 0 || index >= _classes.Length)
            throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");
        
        return _classes[index];
    }

    [XmlIgnore] public BaseNode Parent { get; set; }
    
    [XmlArray("Children")]
    [XmlArrayItem("Node", typeof(BaseNode))]
    [XmlArrayItem("Structure", typeof(StructureNode))]
    [XmlArrayItem("House", typeof(House))]
    [XmlArrayItem("Factory", typeof(Factory))]
    [XmlArrayItem("HarvesterClass", typeof(HarvesterClassNode))]
    [XmlArrayItem("Harvester", typeof(Harvester))]
    [XmlArrayItem("EnergyHarvester", typeof(EnergyHarvester))]
    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    public List<BaseNode> Children { get; set; } = [];
    
}