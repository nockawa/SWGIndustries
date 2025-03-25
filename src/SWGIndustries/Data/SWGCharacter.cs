using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;

namespace SWGIndustries.Data;

/// <summary>
/// Represents a Character owned by a SWG Restoration Account
/// </summary>
[PublicAPI]
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

    /// <summary>
    /// The maximum number of lots this character lend to the crew.
    /// </summary>
    public int MaxLotsForCrew { get; set; }

    /// <summary>
    /// Building the character put down.
    /// </summary>
    public IList<SWGBuilding> PutDownBuildings { get; set; } = new List<SWGBuilding>();
}