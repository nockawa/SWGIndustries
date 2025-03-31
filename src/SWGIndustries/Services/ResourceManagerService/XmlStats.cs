using System.Xml.Serialization;

namespace SWGIndustries.Services;

public class XmlStats
{
    [XmlElement("cr")]
    public ushort CR { get; set; }

    [XmlElement("cd")]
    public ushort CD { get; set; }

    [XmlElement("dr")]
    public ushort DR { get; set; }

    [XmlElement("er")]
    public ushort ER { get; set; }

    [XmlElement("fl")]
    public ushort FL { get; set; }

    [XmlElement("hr")]
    public ushort HR { get; set; }

    [XmlElement("ma")]
    public ushort MA { get; set; }

    [XmlElement("oq")]
    public ushort OQ { get; set; }

    [XmlElement("pe")]
    public ushort PE { get; set; }

    [XmlElement("sr")]
    public ushort SR { get; set; }

    [XmlElement("ut")]
    public ushort UT { get; set; }
}