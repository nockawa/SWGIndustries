using System.ComponentModel.DataAnnotations;

namespace SWGIndustries.Data;

public class NamedSeriesEntity
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string Name { get; set; }

    public int Counter { get; set; }
    
}