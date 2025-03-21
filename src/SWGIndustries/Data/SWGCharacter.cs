using System.ComponentModel.DataAnnotations;

namespace SWGIndustries.Data;

/// <summary>
/// Represents a Character owned by a SWG Restoration Account
/// </summary>
public class SWGCharacter
{
    /// <summary>
    /// Database Primary Key
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Name of the Character
    /// </summary>
    [MaxLength(30)]
    public string Name { get; set; }
    
    /// <summary>
    /// Owner SWG Restoration Account
    /// </summary>
    public SWGAccount SWGAccount { get; set; }

    /// <summary>
    /// <c>true</c> if this character is a member of a Crew (only meaningful if the SWGAccount is a crew member).
    /// </summary>
    public bool IsCrewMember { get; set; }
}