using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;

namespace SWGIndustries.Data;

[PublicAPI]
public class Cluster
{
    /// <summary>
    /// Database Primary Key
    /// </summary>
    [Key]
    public int Id { get; set; }

    public SWGAccount Owner { get; set; }

    public bool IsDefault { get; set; }

    [MaxLength(32)]
    public string Name { get; set; }

    [MaxLength(128)]
    public string Comment { get; set; }

    public IList<SWGBuilding> Buildings { get; set; } = new List<SWGBuilding>();
}