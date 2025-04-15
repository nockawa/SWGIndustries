using System.Diagnostics;
using System.Xml.Serialization;

namespace SWGIndustries.Services;

[DebuggerDisplay("{Name} ({Index})")]
[XmlRoot(ElementName = "ResourceCategory")]
public class XmlResourceCategory
{
    [XmlAttribute("Name")]
    public string Name { get; set; }

    [XmlAttribute("Index")]
    public ushort Index { get; set; }

    [XmlElement("ResourceCategory")]
    public List<XmlResourceCategory> SubCategories { get; set; } = [];
}