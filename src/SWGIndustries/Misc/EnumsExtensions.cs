using System.Drawing;

namespace SWGIndustries;

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
    public static void AllPlanets(this ref Planet planet) =>
        planet = Planet.Corellia | Planet.Dantooine | Planet.Dathomir | Planet.Endor |
                 Planet.Kashyyyk | Planet.Lok | Planet.Mustafar | Planet.Naboo |
                 Planet.Rori | Planet.Talus | Planet.Tatooine | Planet.Yavin4;

    public static Color GetPlanetColor(this Planet planet)
    {
        switch (planet)
        {
            case Planet.Corellia:   return Color.FromArgb(unchecked((int)0xff2190ac));
            case Planet.Dantooine:  return Color.FromArgb(unchecked((int)0xff6a006a));
            case Planet.Dathomir:   return Color.FromArgb(unchecked((int)0xff8f5120));
            case Planet.Endor:      return Color.FromArgb(unchecked((int)0xff6f9975));
            case Planet.Kashyyyk:   return Color.FromArgb(unchecked((int)0xff146e54));
            case Planet.Lok:        return Color.FromArgb(unchecked((int)0xffa84909));
            case Planet.Mustafar:   return Color.FromArgb(unchecked((int)0xff67120d));
            case Planet.Naboo:      return Color.FromArgb(unchecked((int)0xff415c71));
            case Planet.Rori:       return Color.FromArgb(unchecked((int)0xff827660));
            case Planet.Talus:      return Color.FromArgb(unchecked((int)0xff677449));
            case Planet.Tatooine:   return Color.FromArgb(unchecked((int)0xffe0c37b));
            case Planet.Yavin4:     return Color.FromArgb(unchecked((int)0xff71bb9a));
            
            default: return Color.FromArgb(unchecked((int)0xff000000));
        }
    }
    
    public static string GetFriendlyName(this Planet planet) =>
        planet switch
        {
            Planet.Corellia     => "Corellia",
            Planet.Dantooine    => "Dantooine",
            Planet.Dathomir     => "Dathomir",
            Planet.Endor        => "Endor",
            Planet.Kashyyyk     => "Kashyyyk",
            Planet.Lok          => "Lok",
            Planet.Mustafar     => "Mustafar",
            Planet.Naboo        => "Naboo",
            Planet.Rori         => "Rori",
            Planet.Talus        => "Talus",
            Planet.Tatooine     => "Tatooine",
            Planet.Yavin4       => "Yavin 4",
            _                   => "None"
        };
    
    public static void All(ref this Planet planet) => 
        planet = Planet.Corellia | Planet.Dantooine | Planet.Dathomir | Planet.Endor |
                 Planet.Kashyyyk | Planet.Lok | Planet.Mustafar | Planet.Naboo |
                 Planet.Rori | Planet.Talus | Planet.Tatooine | Planet.Yavin4;

    public static string GetMapFilePathName(this Planet planet) =>
        planet switch
        {
            Planet.Corellia     => "Resources/Maps/map_corellia.jpg",
            Planet.Dantooine    => "Resources/Maps/map_dantooine.jpg",
            Planet.Dathomir     => "Resources/Maps/map_dathomir.jpg",
            Planet.Endor        => "Resources/Maps/map_endor.jpg",
            Planet.Kashyyyk     => null,
            Planet.Lok          => "Resources/Maps/map_lok.jpg",
            Planet.Mustafar     => "Resources/Maps/map_mustafar.jpg",
            Planet.Naboo        => "Resources/Maps/map_naboo.jpg",
            Planet.Rori         => "Resources/Maps/map_rori.jpg",
            Planet.Talus        => "Resources/Maps/map_talus.jpg",
            Planet.Tatooine     => "Resources/Maps/map_tatooine.jpg",
            Planet.Yavin4       => "Resources/Maps/map_yavin4.jpg",
            _                   => null
        };
}