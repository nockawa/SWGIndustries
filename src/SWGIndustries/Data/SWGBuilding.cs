using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;

namespace SWGIndustries.Data;

[PublicAPI]
public enum BuildingType
{
    House,
    Harvester,
    Factory
}

[PublicAPI]
public enum BuildingSubType
{
    SmallHouse,
    MediumHouse,
    LargeHouse,
    
    PersonalHarvester,
    MediumHarvester,
    LargeHarvester,
    EliteHarvester,
    
    FoodFactory,
    EquipmentFactory,
    StructureFactory,
    WearableFactory,
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
public class SWGBuilding
{
    /// <summary>
    /// Database Primary Key
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Owner SWG Restoration Account
    /// </summary>
    public SWGAccount Owner { get; set; }

    public BuildingType Type { get; set; }

    public BuildingSubType SubType { get; set; }

    public SWGCharacter PutDownBy { get; set; }
    public Planet PutDownPlanet { get; set; }
    
    public Cluster Cluster { get; set; }

    [MaxLength(64)]
    public string Name { get; set; }
    
}