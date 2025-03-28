using JetBrains.Annotations;
using SWGIndustries.Data;

namespace SWGIndustries.Services;

[PublicAPI]
public interface INode
{
    string Name { get; }
    IEnumerable<INode> Children { get; }
}

[PublicAPI]
public interface IStructure : INode
{
    BuildingType Type { get; }
    BuildingSubType SubType { get; }
    int LotTaken { get; }
    int MaintenanceCost { get; }
    int? PowerConsumption { get; }
}

[PublicAPI]
public enum HarvestingResourceType
{
    Mineral,
    Chemical,
    Gas,
    Flora,
    Water,
}

[PublicAPI]
public interface IHarvester : IStructure
{
    bool IsEnergy { get; }
    HarvestingResourceType HarvestingResourceType { get; }
    int MinBER { get; }
    int MaxBER { get; }
    int MinHopperSizeK { get; }
    int MaxHopperSizeK { get; }
    int ReDeedCost { get; }
}

[PublicAPI]
public abstract class StructuresNode : INode
{
    protected readonly List<IStructure> Structures;
    protected StructuresNode(string name)
    {
        Name = name;
        Structures = [];
    }
    public string Name { get; }
    public IEnumerable<INode> Children => Structures;
}

[PublicAPI]
public class House : IStructure
{
    public House(string name, BuildingSubType subType, int lotTaken, int maintenanceCost)
    {
        Name = name;
        SubType = subType;
        LotTaken = lotTaken;
        MaintenanceCost = maintenanceCost;
    }
    
    public BuildingType Type => BuildingType.House;
    public BuildingSubType SubType { get; }

    public string Name { get; }
    public int LotTaken { get; }
    public int MaintenanceCost { get; }

    public IEnumerable<INode> Children => [];
    public int? PowerConsumption => null;
}

[PublicAPI]
public class Factory : IStructure
{
    public Factory(string name, BuildingSubType subType)
    {
        SubType = subType;
        Name = name;
        LotTaken = 1;
        MaintenanceCost = 50;
        PowerConsumption = 50;
    }

    public BuildingType Type => BuildingType.Factory;
    public BuildingSubType SubType { get; }

    public string Name { get; }
    public int LotTaken { get; }
    public int MaintenanceCost { get; }
    public int? PowerConsumption { get; }

    public IEnumerable<INode> Children => [];
}

[PublicAPI]
public class HousesNode : StructuresNode
{
    public HousesNode() : base("Houses")
    {
        Structures.Add(new House("Merchant Tent", BuildingSubType.HouseMerchantTent, 1, 6));
        Structures.Add(new House("Naboo Small House (type 2)", BuildingSubType.HouseNabooSmallHouse, 1, 8));
        Structures.Add(new House("Small House", BuildingSubType.HouseSmallHouse, 2, 8));
        Structures.Add(new House("Medium House", BuildingSubType.HouseMediumHouse, 3, 18));
        Structures.Add(new House("Large House", BuildingSubType.HouseLargeHouse, 5, 26));
    }
}

[PublicAPI]
public class FactoriesNode : StructuresNode
{
    public FactoriesNode() : base("Factories")
    {
        Structures.Add(new Factory("Equipment Factory", BuildingSubType.FactoryEquipment));
        Structures.Add(new Factory("Food Factory", BuildingSubType.FactoryFood));
        Structures.Add(new Factory("Structure Factory", BuildingSubType.FactoryStructure));
        Structures.Add(new Factory("Wearables Factory", BuildingSubType.FactoryWearables));
    }
}

[PublicAPI]
public class Harvester : IHarvester
{
    public Harvester(string name, BuildingSubType subType, int lotTaken, int maintenanceCost, int powerConsumption, int minBER, int maxBER, int minHopperSizeK, 
        int maxHopperSizeK, int reDeedCost)
    {
        IsEnergy = false;
        Name = name;
        SubType = subType;
        LotTaken = lotTaken;
        MaintenanceCost = maintenanceCost;
        PowerConsumption = powerConsumption;
        MinBER = minBER;
        MaxBER = maxBER;
        MinHopperSizeK = minHopperSizeK;
        MaxHopperSizeK = maxHopperSizeK;
        ReDeedCost = reDeedCost;
    }
    public Harvester(string name, BuildingSubType subType, int minBER, int maxBER, int minHopperSizeK, int maxHopperSizeK, int reDeedCost)
    {
        IsEnergy = true;
        Name = name;
        SubType = subType;
        LotTaken = 1;
        MaintenanceCost = 30;
        PowerConsumption = null;
        MinBER = minBER;
        MaxBER = maxBER;
        MinHopperSizeK = minHopperSizeK;
        MaxHopperSizeK = maxHopperSizeK;
        ReDeedCost = reDeedCost;
    }
    
    public BuildingType Type => BuildingType.Harvester;
    public BuildingSubType SubType { get; }

    public string Name { get; }
    public int LotTaken { get; }
    public int MaintenanceCost { get; }
    public int? PowerConsumption { get; }

    public IEnumerable<INode> Children => [];
    public bool IsEnergy { get; }
    public HarvestingResourceType HarvestingResourceType { get; }
    public int MinBER { get; }
    public int MaxBER { get; }
    public int MinHopperSizeK { get; }
    public int MaxHopperSizeK { get; }
    public int ReDeedCost { get; }
}


[PublicAPI]
public class RegularHarvestersNode : StructuresNode
{
    public RegularHarvestersNode() : base("Regular")
    {
        Structures.Add(new Harvester("Personal", BuildingSubType.HarvesterPersonal, 1, 16, 25, 2, 5, 20, 50, 1500));
        Structures.Add(new Harvester("Medium", BuildingSubType.HarvesterMedium, 1, 30, 50, 8, 11, 50, 60, 3000));
        Structures.Add(new Harvester("Heavy", BuildingSubType.HarvesterHeavy, 1, 90, 75, 11, 14, 110, 130, 4500));
        Structures.Add(new Harvester("Elite", BuildingSubType.HarvesterElite, 3, 126, 206, 40, 44, 360, 410, 12375));
    }
}

[PublicAPI]
public class EnergyHarvestersNode : StructuresNode
{
    public EnergyHarvestersNode() : base("Energy")
    {
        Structures.Add(new Harvester("Wind", BuildingSubType.HarvesterWind, 6, 10, 20, 50, 0));
        Structures.Add(new Harvester("Solar", BuildingSubType.HarvesterSolar, 10, 15, 50, 70, 0));
        Structures.Add(new Harvester("Radioactive", BuildingSubType.HarvesterRadioactive, 14, 19, 110, 130, 4500));
        Structures.Add(new Harvester("Geothermal", BuildingSubType.HarvesterGeothermal, 10, 15, 50, 70, 0));
    }
}

[PublicAPI]
public class HarvestersNode : INode
{
    private readonly List<StructuresNode> _children;
    public HarvestersNode()
    {
        _children =
        [
            new RegularHarvestersNode(),
            new EnergyHarvestersNode()
        ];
    }

    public string Name => "Harvesters";
    public IEnumerable<INode> Children => _children;
}

[PublicAPI]
public class StructuresService : INode
{
    public StructuresNode Houses     { get; } 
    public StructuresNode Factories  { get; }
    public INode Harvesters { get; }

    public StructuresService()
    {
        Houses = new HousesNode();
        Factories = new FactoriesNode();
        Harvesters = new HarvestersNode();
    }

    public string Name => "Structures";
    public IEnumerable<INode> Children => [ Houses, Factories, Harvesters];
}