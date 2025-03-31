using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;
using SWGIndustries.Services;

namespace SWGIndustries.Data;

public enum BuildingType
{
    Undefined,
    House,
    Factory,
    Harvester,
}

public enum BuildingSubType
{
    Undefined,
    HouseMerchantTent,
    HouseNabooSmallHouse,
    HouseSmallHouse,
    HouseMediumHouse,
    HouseLargeHouse,
    FactoryEquipment,
    FactoryFood,
    FactoryStructure,
    FactoryWearables,
    HarvesterPersonal,
    HarvesterMedium,
    HarvesterHeavy,
    HarvesterElite,
    HarvesterWind,
    HarvesterSolar,
    HarvesterRadioactive,
    HarvesterGeothermal,
}

public static class EnumsExtensions
{
    public static string GetName(this BuildingType type)
    {
        return type switch
        {
            BuildingType.House => "House",
            BuildingType.Factory => "Factory",
            BuildingType.Harvester => "Harvester",
            _ => "Unknown"
        };
    }

    public static string GetName(this BuildingSubType type)
    {
        return type switch
        {
            BuildingSubType.HouseMerchantTent => "Merchant Tent",
            BuildingSubType.HouseNabooSmallHouse => "Naboo Small House (type 2)",
            BuildingSubType.HouseSmallHouse => "Small House",
            BuildingSubType.HouseMediumHouse => "Medium House",
            BuildingSubType.HouseLargeHouse => "Large House",
            BuildingSubType.FactoryEquipment => "Equipment Factory",
            BuildingSubType.FactoryFood => "Food Factory",
            BuildingSubType.FactoryStructure => "Structure Factory",
            BuildingSubType.FactoryWearables => "Wearables Factory",
            BuildingSubType.HarvesterPersonal => "Personal Harvester",
            BuildingSubType.HarvesterMedium => "Medium Harvester",
            BuildingSubType.HarvesterHeavy => "Heavy Harvester",
            BuildingSubType.HarvesterElite => "Elite Harvester",
            BuildingSubType.HarvesterWind => "Wind Harvester",
            BuildingSubType.HarvesterSolar => "Solar Harvester",
            BuildingSubType.HarvesterRadioactive => "Radioactive Harvester",
            BuildingSubType.HarvesterGeothermal => "Geothermal Harvester",
            _ => "Unknown"
        };
    }
}

[PublicAPI]
[Flags]
public enum Planet
{
    Undefined   = 0,
    Corellia    = 0x0001,
    Dantooine   = 0x0002,
    Dathomir    = 0x0004,
    Endor       = 0x0008,
    Kashyyyk    = 0x0010,
    Lok         = 0x0020,
    Mustafar    = 0x0040,
    Naboo       = 0x0080,
    Rori        = 0x0100,
    Talus       = 0x0200,
    Tatooine    = 0x0400,
    Yavin4      = 0x0800
}

[PublicAPI]
public class BuildingEntity
{
    /// <summary>
    /// Database Primary Key
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Owner Game Account (SWG Restoration Account)
    /// </summary>
    public GameAccountEntity Owner { get; set; }

    public BuildingType Type { get; set; }

    public BuildingSubType SubType { get; set; }

    public CharacterEntity PutDownBy { get; set; }
    public Planet PutDownPlanet { get; set; }
    
    public ClusterEntity Cluster { get; set; }

    [MaxLength(64)]
    public string Name { get; set; }

    [MaxLength(256)]
    public string Comments { get; set; }

    public int MaintenanceAmount { get; set; }
    public DateTime? MaintenanceLastUpdate { get; set; }

    public int PowerAmount { get; set; }
    public DateTime? PowerLastUpdate { get; set; }

    public bool IsRunning { get; set; }
    public DateTime? LastRunningDateTime { get; set; }
    public DateTime? LastStoppedDateTime { get; set; }

    public bool HarvesterSelfPowered { get; set; }
    public int HarvesterBER { get; set; }
    public int HarvesterHopperSize { get; set; }
    public HarvestingResourceType HarvestingResourceType { get; set; }
}