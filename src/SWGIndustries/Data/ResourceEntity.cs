using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace SWGIndustries.Data;

[DebuggerDisplay("Name: {Name}, since {AvailableSince}")]
public class ResourceEntity
{
    [Key]
    public int Id { get; set; }

    public int GameServerId { get; set; }
    
    [MaxLength(64)]    
    public string Name { get; set; }

    // Resource category index as defined in the resource_tree.xml file
    // Will also be present in one of the C0-C7 properties at the corresponding nested level
    public ushort CategoryIndex { get; set; }
    
    public int SWGAideId { get; set; }
    
    public Planet Planets { get; set; }

    // Resource category indices, we want a resource to be lookable not only in its "leaf" category but all the way up to the root
    // There are 8 possible categories, a value of 0 means no category from this point on
    public ushort CI0 { get; set; }
    public ushort CI1 { get; set; }
    public ushort CI2 { get; set; }
    public ushort CI3 { get; set; }
    public ushort CI4 { get; set; }
    public ushort CI5 { get; set; }
    public ushort CI6 { get; set; }
    public ushort CI7 { get; set; }
    
    public ushort CR { get; set; }
    public ushort CD { get; set; }
    public ushort DR { get; set; }
    public ushort ER { get; set; }
    public ushort FL { get; set; }
    public ushort HR { get; set; }
    public ushort MA { get; set; }
    public ushort OQ { get; set; }
    public ushort PE { get; set; }
    public ushort SR { get; set; }
    public ushort UT { get; set; }

    public DateTime AvailableSince { get; set; }
    public DateTime? DepletedSince { get; set; }
    
    public string ReportedBy { get; set; }
}