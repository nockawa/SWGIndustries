using System.ComponentModel.DataAnnotations;

namespace SWGIndustries.Data;

/// <summary>
/// Represents a SWG Restoration Account owned by a SWG Industries user.
/// </summary>
public class SWGAccount
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
    public ApplicationUser OwnerApplicationUser { get; set; }
    
    /// <summary>
    /// SWG Characters owned by this SWG Restoration Account.
    /// </summary>
    public IList<SWGCharacter> SWGCharacters { get; set; } = new List<SWGCharacter>();
    
    public IList<Cluster> Clusters { get; set; } = new List<Cluster>();
    
    public IList<SWGBuilding> SWGBuildings { get; set; } = new List<SWGBuilding>();
    
    #endregion

    /// <summary>
    /// Name of the SWG Restoration Account.
    /// </summary>
    [MaxLength(30)]
    public string Name { get; set; }
}