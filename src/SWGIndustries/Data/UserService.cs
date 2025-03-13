using System.Security.Claims;
using AspNet.Security.OAuth.Discord;
using Microsoft.AspNetCore.Authentication;

namespace SWGIndustries.data;

public class UserService
{
    private static HttpClient client = new HttpClient();

    /// <summary>
    /// Parses the user's discord claim for their `identify` information
    /// </summary>
    /// <param name="httpContext"></param>
    /// <returns></returns>
    public DiscordUserClaim GetInfo(HttpContext httpContext)
    {
        if (!httpContext.User.Identity.IsAuthenticated)
        {
            return null;
        }

        var claims = httpContext.User.Claims.ToList();

        var userId = ulong.Parse(claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value);
        var name = claims.First(c => c.Type == ClaimTypes.Name).Value;
        var discriminator = claims.First(x => x.Type == DiscordAuthenticationConstants.Claims.Discriminator).Value;
        var avatar = claims.First(x => x.Type == DiscordAuthenticationConstants.Claims.AvatarHash).Value;
        var value = claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;
        var avatarUrl = $"https://cdn.discordapp.com/avatars/{userId}/{avatar}.{(avatar.StartsWith("a_") ? "gif" : "png")}";
        var userClaim = new DiscordUserClaim
        {
            UserId = userId, 
            Name = name,
            Discriminator = discriminator,
            AvatarUrl = avatarUrl,
            Email = value,
        };

        return userClaim;
    }

    /// <summary>
    /// Gets the user's discord oauth2 access token
    /// </summary>
    /// <param name="httpContext"></param>
    /// <returns></returns>
    public async Task<string> GetTokenAsync(HttpContext httpContext)
    {
        if (!httpContext.User.Identity.IsAuthenticated)
        {
            return null;
        }

        var tk = await httpContext.GetTokenAsync("Discord", "access_token");
        return tk;
    }

    public class DiscordUserClaim
    {
        public ulong UserId { get; set; }
        public string Name { get; set; }
        public string Discriminator { get; set; }
        public string AvatarUrl { get; set; }

        /// <summary>
        /// Will be null if the email scope is not provided
        /// </summary>
        public string Email { get; set; } = null;
    }
}