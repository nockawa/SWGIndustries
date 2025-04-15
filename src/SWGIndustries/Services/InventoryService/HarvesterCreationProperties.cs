using JetBrains.Annotations;

namespace SWGIndustries.Services;

[PublicAPI]
public class HarvesterCreationProperties
{
    public HarvesterCreationProperties(int hopperSizeK, int ber, bool selfPowered, HarvestingResourceType harvestingResourceType)
    {
        HopperSizeK = hopperSizeK;
        BER = ber;
        SelfPowered = selfPowered;
        HarvestingResourceType = harvestingResourceType;
    }

    public HarvesterCreationProperties()
    {
    }

    public int HopperSizeK { get; set; }
    public int BER { get; set; }
    public bool SelfPowered { get; set; }
    public HarvestingResourceType HarvestingResourceType { get; set; }
}