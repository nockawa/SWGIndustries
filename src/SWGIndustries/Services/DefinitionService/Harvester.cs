﻿using System.Xml.Serialization;

namespace SWGIndustries.Services;

public interface IHarvesterBase : IStructureNode
{
    HarvestingResourceType ResourceType { get; }
    int MinBER { get; }
    int MaxBER { get; }
    int MinHopperSizeK { get; }
    int MaxHopperSizeK { get; }
    int ReDeedCost { get; }
}

public interface IHarvester : IHarvesterBase
{
    int PowerConsumption { get; }
}

public class Harvester : StructureNode, IHarvester
{
    [XmlIgnore] public override int LotTaken => ((HarvesterClassNode)Parent).LotTaken;
    [XmlIgnore] public override int MaintenanceCost => ((HarvesterClassNode)Parent).MaintenanceCost;
    [XmlIgnore] public int PowerConsumption => ((HarvesterClassNode)Parent).PowerConsumption;
    [XmlIgnore] public int MinBER => ((HarvesterClassNode)Parent).MinBER;
    [XmlIgnore] public int MaxBER => ((HarvesterClassNode)Parent).MaxBER;
    [XmlIgnore] public int MinHopperSizeK => ((HarvesterClassNode)Parent).MinHopperSizeK;
    [XmlIgnore] public int MaxHopperSizeK => ((HarvesterClassNode)Parent).MaxHopperSizeK;
    [XmlIgnore] public int ReDeedCost => ((HarvesterClassNode)Parent).ReDeedCost;

    [XmlAttribute("ResourceType")]
    public HarvestingResourceType ResourceType { get; set; }
}