using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;

namespace SWGIndustries.Data;

/// <summary>
/// Represents a cluster of a given resource to harvest.
/// </summary>
/// <remarks>
/// The cluster is owned by either a crew or a game account, its primary purpose is to track the harvesters that are put down for this.
/// The secondary purpose is to give more information about where, who and why.
/// </remarks>
[PublicAPI]
public class ClusterEntity
{
    /// <summary>
    /// Database Primary Key
    /// </summary>
    [Key]
    public int Id { get; set; }

    public DateTime CreationDateTime { get; set; }
    
    /// <summary>
    /// The crew that owns this cluster.
    /// </summary>
    /// <remarks>
    /// Is <c>null</c> if the cluster is owned by a game account.
    /// </remarks>
    public CrewEntity Crew { get; set; }
    
    /// <summary>
    /// Game account that owns this cluster.
    /// </summary>
    /// <remarks>
    /// Is <c>null</c> if the cluster is owned by a crew.
    /// </remarks>
    public GameAccountEntity GameAccount { get; set; }

    /// <summary>
    /// The resource to harvest from this cluster.
    /// </summary>
    public ResourceEntity Resource { get; set; }

    /// <summary>
    /// The planet on which the cluster is located
    /// </summary>
    /// <remarks>
    /// Is required to be set, even if <see cref="Waypoint"/> is set too.
    /// </remarks>
    public Planet Planet { get; set; }

    /// <summary>
    /// Location of the cluster in the game.
    /// </summary>
    /// <remarks>
    /// Is optional.
    /// </remarks>
    [MaxLength(128)]
    public string Waypoint { get; set; }

    /// <summary>
    /// Friendly name of the cluster.
    /// </summary>
    [MaxLength(32)]
    public string Name { get; set; }

    /// <summary>
    /// Comment about the cluster.
    /// </summary>
    [MaxLength(1024)]
    public string Comments { get; set; }

    /// <summary>
    /// Harvesters that are placed in this cluster.
    /// </summary>
    public IList<BuildingEntity> Buildings { get; set; } = new List<BuildingEntity>();
}