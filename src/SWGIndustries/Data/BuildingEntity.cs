using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using JetBrains.Annotations;
using SWGIndustries.Services;

namespace SWGIndustries.Data;

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

    [MaxLength(128)]
    public string FullClass { get; set; }

    public CharacterEntity PutDownBy { get; set; }
    public DateTime? PutDownDateTime { get; set; }
    public bool BuildingForCrew { get; set; }
    public Planet PutDownPlanet { get; set; }
    
    public ClusterEntity Cluster { get; set; }

    [MaxLength(64)]
    public string Name { get; set; }

    [MaxLength(1024)]
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

    public float ResourceConcentration { get; set; }
}