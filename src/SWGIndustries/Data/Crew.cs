using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace SWGIndustries.Data;

[Index(nameof(Name), IsUnique = true)]
public class Crew
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
    public ApplicationUser CrewLeader { get; set; }

    /// <summary>
    /// Name of the crew
    /// </summary>
    [MaxLength(CrewNameMaxLength)]
    public string Name { get; set; }
    
    /// <summary>
    /// Members of the crew (leader included)
    /// </summary>
    public IList<ApplicationUser> Members { get; } = new List<ApplicationUser>();
}