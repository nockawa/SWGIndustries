using JetBrains.Annotations;

namespace SWGIndustries.Services;

/// <summary>
/// Different types of resources that can be harvested.
/// </summary>
[PublicAPI]
public enum HarvestingResourceType
{
    Mineral,
    Chemical,
    Gas,
    Flora,
    Water,
    Wind,
    Solar,
    Geothermal,
    Radioactive,
    Creature,
    Unknown,
}