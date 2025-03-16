using System.ComponentModel.DataAnnotations;

namespace SWGIndustries.Data;

public class ApplicationUser
{
    [Key]
    public int Id { get; set; }
    
    [MaxLength(30)]
    public string CorrelationId { get; set; }
}