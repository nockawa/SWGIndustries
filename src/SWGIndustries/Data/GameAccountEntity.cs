using System.ComponentModel.DataAnnotations;

namespace SWGIndustries.Data;

/// <summary>
/// Represents a SWG Restoration Account owned by a SWG Industries user.
/// </summary>
public class GameAccountEntity
{
    /// <summary>
    /// Database Primary Key
    /// </summary>
    [Key]
    public int Id { get; set; }

    #region Navigation Properties

    /// <summary>
    /// SWG Industries user that owns this SWG Restoration Account.
    /// </summary>
    public AppAccountEntity OwnerAppAccount { get; set; }
    
    /// <summary>
    /// SWG Characters owned by this SWG Restoration Account.
    /// </summary>
    public IList<CharacterEntity> Characters { get; set; } = new List<CharacterEntity>();
    
    public IList<ClusterEntity> Clusters { get; set; } = new List<ClusterEntity>();
    
    public IList<BuildingEntity> Buildings { get; set; } = new List<BuildingEntity>();
    
    #endregion

    /// <summary>
    /// Name of the SWG Restoration Account.
    /// </summary>
    [MaxLength(30)]
    public string Name { get; set; }
}