using System.ComponentModel.DataAnnotations;

namespace SWGIndustries.Data;

public class NamedSeries
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string Name { get; set; }

    public int Counter { get; set; }
    
}