using System.Security.Claims;
using System.Text.Json;
using AspNet.Security.OAuth.Discord;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using SWGIndustries.Data;

namespace SWGIndustries.Services;

public class UserManager
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private ExternalUserInfo _externalUserInfo;
    private bool _profileFetched;

    public UserManager(IServiceProvider serviceProvider, IHttpContextAccessor httpContextAccessor)
    {
        _serviceProvider = serviceProvider;
        _httpContextAccessor = httpContextAccessor;
    }
    
    internal async Task<ExternalUserInfo> UserLoggedIn(HttpContext httpContext)
    {
        // Whether the user is logged or not, we won't attempt to load the profile again, until the user logs in again
        _profileFetched = true;

        if (!httpContext.User.Identity!.IsAuthenticated)
        {
            return null;
        }

        var eui = new ExternalUserInfo();
        var claims = httpContext.User.Claims.ToList();

        eui.UserId = claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value;
        eui.Name = claims.First(c => c.Type == ClaimTypes.Name).Value;
        eui.Email = claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;

        if (httpContext.User.Identity.AuthenticationType == "Discord")
        {
            eui.Discriminator = claims.First(x => x.Type == DiscordAuthenticationConstants.Claims.Discriminator).Value;
            var avatar= claims.First(x => x.Type == DiscordAuthenticationConstants.Claims.AvatarHash).Value;
            eui.AvatarUrl = $"https://cdn.discordapp.com/avatars/{eui.UserId}/{avatar}.{(avatar.StartsWith("a_") ? "gif" : "png")}";
        } 
        else if (httpContext.User.Identity.AuthenticationType == "Google")
        {
            var authResult = await httpContext.AuthenticateAsync(httpContext.User.Identity.AuthenticationType);
            var properties = authResult.Properties;
            if (properties != null && properties.Items.TryGetValue(".Token.access_token", out var token))
            {
                try
                {
                    var httpClient = new HttpClient();
                    var response = await httpClient.GetStringAsync($"https://www.googleapis.com/oauth2/v1/userinfo?access_token={token}");
                    var userInfo = JsonDocument.Parse(response);
                    eui.AvatarUrl = userInfo.RootElement.GetProperty("picture").GetString();
                }
                catch (Exception)
                {
                    // TODO, cookie expired to solve
                }
            }
        }

        using (var scope = _serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var user = await context.Users.FirstOrDefaultAsync(x => x.CorrelationId == eui.UserId);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    CorrelationId = eui.UserId,
                };
                context.Users.Add(user);
                await context.SaveChangesAsync();
                eui.UserDbId = user.Id;
            }
            else
            {
                eui.UserDbId = user.Id;
            }
        }
        
        _externalUserInfo = eui;
        return eui;
    }

    internal void UserLoggedOut()
    {
        _profileFetched = false;
        _externalUserInfo = null;
    }

    public async Task<ExternalUserInfo> GetExternalUserInfo()
    {
        if (_profileFetched == false)
        {
            _externalUserInfo = await UserLoggedIn(_httpContextAccessor.HttpContext);
        }
        return _externalUserInfo;
    }
}

public class ExternalUserInfo
{
    public int UserDbId { get; set; }
    public string UserId { get; set; }
    public string Name { get; set; }
    public string Discriminator { get; set; }
    public string AvatarUrl { get; set; }

    /// <summary>
    /// Will be null if the email scope is not provided
    /// </summary>
    public string Email { get; set; } = null;
}
