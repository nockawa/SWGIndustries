using System.ComponentModel.DataAnnotations;

namespace SWGIndustries.Data;

/// <summary>
/// Represent a crew of players sharing their lots for housing, harvesting and factories.
/// </summary>
public class CrewEntity
{
    internal const int CrewNameMaxLength = 64;
    
    /// <summary>
    /// Database Primary Key
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Crew leader
    /// </summary>
    /// <remarks>
    /// A given <see cref="AppAccountEntity"/> can be the leader of just one crew at most.
    /// </remarks>
    public AppAccountEntity CrewLeader { get; set; }

    /// <summary>
    /// Name of the crew, must be unique
    /// </summary>
    /// <remarks>
    /// The name must be unique because people can make a request to join a crew from a name.
    /// </remarks>
    [MaxLength(CrewNameMaxLength)]
    public string Name { get; set; }
    
    /// <summary>
    /// Members of the crew (leader included)
    /// </summary>
    public IList<AppAccountEntity> Members { get; } = new List<AppAccountEntity>();

    /// <summary>
    /// Cluster of harvesters put down by this crew.
    /// </summary>
    public IList<ClusterEntity> Clusters { get; set; } = new List<ClusterEntity>();
}