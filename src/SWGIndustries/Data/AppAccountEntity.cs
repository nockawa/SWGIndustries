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
public class AppAccountEntity
{
    /// <summary>
    /// Database Primary Key
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Name of the user, as given by the authentication provider.
    /// </summary>
    [MaxLength(32)]
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
    public CrewEntity Crew { get; set; }
    
    /// <summary>
    /// List of GameAccounts owned by this SWG Industries user.
    /// </summary>
    public IList<GameAccountEntity> GameAccounts { get; } = new List<GameAccountEntity>();

    public bool IsCrewLeader => Crew != null && Crew.CrewLeader == this;

    #endregion
    
    public static AppAccountEntity Guest => new AppAccountEntity
    {
        CorrelationId = "Guest",
        ThemeMode = ThemeMode.Auto
    };

}