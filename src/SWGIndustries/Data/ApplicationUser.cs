using System.ComponentModel.DataAnnotations;

namespace SWGIndustries.Data;

/// <summary>
/// WebApp theme modes
/// </summary>
public enum ThemeMode
{
    /// <summary>
    /// System default theme mode
    /// </summary>
    Auto,
    
    /// <summary>
    /// Light mode
    /// </summary>
    Light,
    
    /// <summary>
    /// Dark mode
    /// </summary>
    Dark
}

/// <summary>
/// Represents a user of the SWG Industries web application.
/// </summary>
public class ApplicationUser
{
    /// <summary>
    /// Database Primary Key
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Name of the user, as given by the authentication provider.
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// Unique identifier given by the authentication provider.
    /// </summary>
    [MaxLength(30)]
    public string CorrelationId { get; set; }

    /// <summary>
    /// Theme Mode selected by the user.
    /// </summary>
    public ThemeMode ThemeMode { get; set; }

    #region Navigation Properties
    /// <summary>
    /// If non <c>null</c> the user is member of the given crew.
    /// </summary>
    public Crew Crew { get; set; }
    
    /// <summary>
    /// List of SWGAccounts owned by this SWG Industries user.
    /// </summary>
    public IList<SWGAccount> SWGAccounts { get; } = new List<SWGAccount>();

    #endregion
    
    public static ApplicationUser Guest => new ApplicationUser
    {
        CorrelationId = "Guest",
        ThemeMode = ThemeMode.Auto
    };

}