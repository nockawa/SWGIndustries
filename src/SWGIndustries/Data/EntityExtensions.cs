namespace SWGIndustries.Data;

public static class EntityExtensions
{
    public static int GetBuildingLotRequirement(this BuildingEntity building)
    {
        // We assume the building Type is coherent with the subtype.
        switch (building.SubType)
        {
            case BuildingSubType.HouseMerchantTent:
            case BuildingSubType.HouseNabooSmallHouse:
            case BuildingSubType.FactoryEquipment:
            case BuildingSubType.FactoryFood:
            case BuildingSubType.FactoryStructure:
            case BuildingSubType.FactoryWearables:
            case BuildingSubType.HarvesterPersonal:
            case BuildingSubType.HarvesterMedium:
            case BuildingSubType.HarvesterHeavy:
            case BuildingSubType.HarvesterWind:
            case BuildingSubType.HarvesterSolar:
            case BuildingSubType.HarvesterRadioactive:
            case BuildingSubType.HarvesterGeothermal:
                return 1;
            case BuildingSubType.HouseMediumHouse:
            case BuildingSubType.HarvesterElite:
                return 3;
            case BuildingSubType.HouseLargeHouse:
                return 5;
            default:
                return 0;
        } 
    }
}