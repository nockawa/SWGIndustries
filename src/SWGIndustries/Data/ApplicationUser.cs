using System.ComponentModel.DataAnnotations;

namespace SWGIndustries.Data;

public enum ThemeMode
{
    Auto,
    Light,
    Dark
}

public class ApplicationUser
{
    [Key]
    public int Id { get; set; }
    
    [MaxLength(30)]
    public string CorrelationId { get; set; }

    public ThemeMode ThemeMode { get; set; }
}