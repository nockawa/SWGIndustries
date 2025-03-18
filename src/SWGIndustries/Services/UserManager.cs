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
    private UserInfo _userInfo;
    private bool _profileFetched;

    public UserManager(IServiceProvider serviceProvider, IHttpContextAccessor httpContextAccessor)
    {
        _serviceProvider = serviceProvider;
        _httpContextAccessor = httpContextAccessor;
    }
    
    internal async Task<UserInfo> UserLoggedIn(HttpContext httpContext)
    {
        // Whether the user is logged or not, we won't attempt to load the profile again, until the user logs in again
        _profileFetched = true;

        if (!httpContext.User.Identity!.IsAuthenticated)
        {
            return null;
        }

        _userInfo = await UserInfo.Create(_serviceProvider, httpContext);
        return _userInfo;
    }

    internal void UserLoggedOut()
    {
        _profileFetched = false;
        _userInfo = null;
    }

    public async Task<UserInfo> GetUserInfo()
    {
        if (_profileFetched == false)
        {
            _userInfo = await UserLoggedIn(_httpContextAccessor.HttpContext);
        }
        return _userInfo ?? UserInfo.Unauthenticated;
    }
}

public class UserInfo
{
    private IServiceProvider _serviceProvider;
    private ApplicationUser _applicationUser;

    internal static async Task<UserInfo> Create(IServiceProvider serviceProvider, HttpContext httpContext)
    {
        var userInfo = new UserInfo();
        await userInfo.Setup(httpContext, serviceProvider);
        return userInfo;
    }
    
    private async Task Setup(HttpContext httpContext, IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        var httpContextUser = httpContext.User;
        var claims = httpContextUser.Claims.ToList();

        if (httpContextUser.Identity == null)
        {
            return;
        }

        UserExternalId = claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value;
        Name = claims.First(c => c.Type == ClaimTypes.Name).Value;
        Email = claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;

        switch (httpContextUser.Identity.AuthenticationType)
        {
            case "Discord":
            {
                var avatar= claims.First(x => x.Type == DiscordAuthenticationConstants.Claims.AvatarHash).Value;
                AvatarUrl = $"https://cdn.discordapp.com/avatars/{UserExternalId}/{avatar}.{(avatar.StartsWith("a_") ? "gif" : "png")}";
                break;
            }
            case "Google":
            {
                var authResult = await httpContext.AuthenticateAsync(httpContextUser.Identity.AuthenticationType);
                var properties = authResult.Properties;
                if (properties != null && properties.Items.TryGetValue(".Token.access_token", out var token))
                {
                    try
                    {
                        var httpClient = new HttpClient();
                        var response = await httpClient.GetStringAsync($"https://www.googleapis.com/oauth2/v1/userinfo?access_token={token}");
                        var googleUserInfo = JsonDocument.Parse(response);
                        AvatarUrl = googleUserInfo.RootElement.GetProperty("picture").GetString();
                    }
                    catch (Exception)
                    {
                        // TODO, cookie expired to solve
                    }
                }
                break;
            }
        }
        
        // Create a new user in database if it doesn't exist, or load its settings
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var user = await context.Users.FirstOrDefaultAsync(x => x.CorrelationId == UserExternalId);
        if (user == null)
        {
            user = new ApplicationUser
            {
                CorrelationId = UserExternalId,
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();
        }
        _applicationUser = user;
    }

    public bool IsAuthenticatedUser => _applicationUser != null;

    public ThemeMode ThemeMode
    {
        get => _applicationUser?.ThemeMode ?? ThemeMode.Auto;
        set
        {
            if (_applicationUser == null || _applicationUser.ThemeMode == value)
            {
                return;
            }
            
            _applicationUser.ThemeMode = value;
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            context.Users.Update(_applicationUser);
            context.SaveChanges();
        }
    }

    public string UserExternalId { get; private set; }
    public string Name { get; private set; }
    public string AvatarUrl { get; private set; }
    public string Email { get; private set; }
    public static UserInfo Unauthenticated { get; } = new()
    {
        Name = "Guest"
    };
}
