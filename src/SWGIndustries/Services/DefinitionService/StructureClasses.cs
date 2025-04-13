using JetBrains.Annotations;

namespace SWGIndustries.Services;

[PublicAPI]
public static class StructureClasses
{
    public const string House = "House";
    public const string Factory = "Factory";
    public const string Harvester = "Harvester";
    public const string HarvesterRegular = "Harvester.Regular";
    public const string HarvesterRegularPersonal = "Harvester.Regular.Personal";
    public const string HarvesterRegularMedium = "Harvester.Regular.Medium";
    public const string HarvesterRegularHeavy = "Harvester.Regular.Heavy";
    public const string HarvesterRegularElite = "Harvester.Regular.Elite";
    public const string HarvesterEnergy = "Harvester.Energy";
    
    public static bool IsEnergy(this Harvester harvester) => harvester.FullClass.StartsWith(HarvesterEnergy);
}