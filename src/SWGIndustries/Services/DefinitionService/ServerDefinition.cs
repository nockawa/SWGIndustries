﻿using System.Diagnostics;
using System.Xml.Serialization;

namespace SWGIndustries.Services;

[DebuggerDisplay("{Name}")]
[XmlRoot(ElementName = "Server", Namespace = "http://swgindustries.com/swgserver")]
public class ServerDefinition
{
    [XmlElement(IsNullable = false)]
    public string Name { get; set; }
    
    [XmlElement(IsNullable = false)]
    public string SWGAideResourceFileURL { get; set; }
    
    [XmlElement(IsNullable = false)]
    public int LotCountPerCharacter { get; set; }
    
    [XmlElement(IsNullable = false)]
    public int MaxCharacterCountPerAccount { get; set; }
    
    [XmlElement(IsNullable = false)]
    public float HarvesterExtractionFactor { get; set; }
}