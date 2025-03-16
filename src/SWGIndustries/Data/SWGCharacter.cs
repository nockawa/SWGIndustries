using System.ComponentModel.DataAnnotations;

namespace SWGIndustries.Data;

public class SWGCharacter
{
    [Key]
    public int Id { get; set; }
    
    [MaxLength(30)]
    public string Name { get; set; }
    
    public SWGAccount Account { get; set; }
}