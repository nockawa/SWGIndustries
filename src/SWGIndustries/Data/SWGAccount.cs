using System.ComponentModel.DataAnnotations;

namespace SWGIndustries.Data;

public class SWGAccount
{
    public SWGAccount()
    {
        SWGCharacters = new List<SWGCharacter>();
    }
    
    [Key]
    public int Id { get; set; }
    
    [MaxLength(30)]
    public string Name { get; set; }
    
    public IList<SWGCharacter> SWGCharacters { get; set; }
}