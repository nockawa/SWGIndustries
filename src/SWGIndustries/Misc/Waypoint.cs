using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace SWGIndustries;

public enum WaypointColor
{
    Blue,
    White,
    Purple,
    Green,
    Yellow,
    Orange
}

/// <summary>
/// Represents a in-game waypoint.
/// </summary>
/// <remarks>
/// More info here: https://swg.fandom.com/wiki/Waypoint
/// </remarks>
[PublicAPI]
public class Waypoint
{
    /// <summary>
    /// The planet on which the waypoint is located
    /// </summary>
    public Planet Planet { get; }
    
    /// <summary>
    /// East/west axis, will be negative if it is west of the centre line of the map.
    /// </summary>
    public int X { get; }
    
    /// <summary>
    /// North/south axis, will be negative if it is south of the centre line of the map.
    /// </summary>
    public int Y { get; }
    
    /// <summary>
    /// Elevation above or below sea-level, will be negative if it is below sea-level.
    /// </summary>
    public int? Z { get; }

    /// <summary>
    /// Color of the visual representation of the waypoint in-game
    /// </summary>
    public WaypointColor Color { get; }
    
    /// <summary>
    /// Name of the waypoint
    /// </summary>
    public string Name { get; }

    public bool IsDefault => X == 0 && Y == 0 && Z == 0;
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="planet">Planet</param>
    /// <param name="x">X coordinate</param>
    /// <param name="y">Y coordinate</param>
    /// <param name="z">Z coordinate</param>
    /// <param name="name">Name of the waypoint</param>
    /// <param name="color">Color</param>
    public Waypoint(Planet planet, int x, int y, int? z, string name = null, WaypointColor color = WaypointColor.Blue)
    {
        Planet = planet;
        X = x;
        Y = y;
        Z = z;
        Name = name;
        Color = color;
    }

    public static bool TryParse(string inGameWaypoint, out Waypoint waypoint)
    {
        try
        {
            waypoint = Parse(inGameWaypoint);
            return true;
        }
        catch (ArgumentException)
        {
            waypoint = null;
            return false;
        }
    }
    
    public static Waypoint Parse(string inGameWaypoint)
    {
        //const string s = @"\/(wp|waypoint|way)\s+(?<planet>tatooine|naboo|corellia|rori|talus|yavin4|endor|lok|dantooine|dathomir|mustafar)?\s*?(?<a>-?\d+)\s+(?<b>-?\d+)\s*(?<c>-?\d+)?\s+?(?<color>white|purple|green|yellow|orange|blue)?\s*(?<name>.*)?";
        const string s = @"\/(?:wp|waypoint|way)(?:\s+(?<planet>tatooine|naboo|corellia|rori|talus|yavin4|endor|lok|dantooine|dathomir|mustafar))?(?:\s+(?<a>-?\d+))(?:\s+(?<b>-?\d+))(?:\s+(?<c>-?\d+))?(?:\s+(?<color>white|purple|green|yellow|orange|blue))?(?:\s+(?<name>.*))?";        
        var regex = new Regex(s, RegexOptions.IgnoreCase);
        var match = regex.Match(inGameWaypoint + " ");      // + " " because I don't know how to make good regex...

        if (!match.Success)
        {
            throw new ArgumentException("Invalid waypoint format", nameof(inGameWaypoint));
        }

        var planet = FromWaypoint(match.Groups["planet"].Value.ToLower());
        var a = int.Parse(match.Groups["a"].Value);
        var b = int.Parse(match.Groups["b"].Value);
        int x, y;
        int? z;
        
        if (match.Groups.TryGetValue("z", out var zGroup))
        {
            x = a;
            y = b;
            z = int.Parse(zGroup.Value);
        }
        else
        {
            x = a;
            y = b;
            z = null;
        }
        if (x is < -8192 or > 8192)
        {
            throw new ArgumentException("X coordinate out of range", nameof(inGameWaypoint));
        }
        if (y is < -8192 or > 8192)
        {
            throw new ArgumentException("Y coordinate out of range", nameof(inGameWaypoint));
        }
        var color = match.Groups.TryGetValue("color", out var cGroup) ? (Enum.TryParse<WaypointColor>(cGroup.Value, true, out var parsedColor) ? parsedColor : WaypointColor.Blue) : WaypointColor.Blue;
        var name = match.Groups.TryGetValue("name", out var nameGroup) ? nameGroup.Value : string.Empty;

        return new Waypoint(planet, x, y, z, name, color);
    }

    public override string ToString()
    {
        var planet = (Planet != Planet.Undefined) ? $" {Planet.ToString().ToLower()}" : string.Empty;
        var z = Z != null ? $" {Z}" : "";
        var col = Color == WaypointColor.Blue ? "" : $" {Color.ToString().ToLower()}";
        var name = string.IsNullOrEmpty(Name) ? string.Empty : $" {Name}";
        return $"/wp{planet} {X}{z} {Y} {col}{name}";
    }

    private static Planet FromWaypoint(string waypointPlanetName)
    {
        switch (waypointPlanetName)
        {
            case "tatooine":    return Planet.Tatooine;
            case "naboo":       return Planet.Naboo;    
            case "corellia":    return Planet.Corellia;
            case "rori":        return Planet.Rori;
            case "talus":       return Planet.Talus;    
            case "yavin4":      return Planet.Yavin4;      
            case "endor":       return Planet.Endor;
            case "lok":         return Planet.Lok;      
            case "dantooine":   return Planet.Dantooine;
            case "dathomir":    return Planet.Dathomir; 
            case "mustafar":    return Planet.Mustafar;
            default:            return Planet.Undefined;
        }
    }
}
