using System.Security.Claims;
using System.Text.Json;
using AspNet.Security.OAuth.Discord;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using SWGIndustries.Data;

namespace SWGIndustries.Services;

public class UserService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly GameServersManager _gameServersManager;
    private readonly ILogger<UserService> _logger;
    private UserInfo _userInfo;

    public UserService(IServiceProvider serviceProvider, IHttpContextAccessor httpContextAccessor, GameServersManager gameServersManager, ILogger<UserService> logger)
    {
        _serviceProvider = serviceProvider;
        _httpContextAccessor = httpContextAccessor;
        _gameServersManager = gameServersManager;
        _logger = logger;
    }

    internal async Task<UserInfo> UserLoggedIn(HttpContext httpContext)
    {
        if (!httpContext.User.Identity!.IsAuthenticated)
        {
            return null;
        }

        _userInfo = await UserInfo.Create(_serviceProvider, httpContext);
        return _userInfo;
    }

    internal void UserLoggedOut()
    {
        _userInfo = null;
    }

    public async Task<UserInfo> GetUserInfo()
    {
        if ((_userInfo==null) || (_userInfo.IsGuest && _httpContextAccessor.HttpContext is { User.Identity.IsAuthenticated: true }))
        {
            _userInfo = await UserLoggedIn(_httpContextAccessor.HttpContext);
        }
        return _userInfo ?? UserInfo.GetUnauthenticated(_serviceProvider);
    }
    public async Task<GameServerDefinition> GetUserServerInfo()
    {
        var user = await GetUserInfo();
        return _gameServersManager.GetServer(user.SWGServerName);
    }
    
    public async Task<bool> SetUserServerInfo(GameServerDefinition gameServerDefinition)
    {
        try
        {
            var userInfo = await GetUserInfo();
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var appAccountEntity = await context.AppAccounts.FirstOrDefaultAsync(a => a.CorrelationId==userInfo.UserExternalId);
            appAccountEntity.SWGServerName = gameServerDefinition.Name;
            context.AppAccounts.Update(appAccountEntity);
            return await context.SaveChangesAsync() > 0;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error setting user server info");
            return false;
        }
    }


    public async Task<AppAccountEntity> BuildAppAccount(ApplicationDbContext dbContext) => (await GetUserInfo()).BuildAppAccount(dbContext);
}

public class UserInfo
{
    private readonly IServiceProvider _serviceProvider;
    private ThemeMode? _themeMode;
    
    private UserInfo(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    internal static async Task<UserInfo> Create(IServiceProvider serviceProvider, HttpContext httpContext)
    {
        var userInfo = new UserInfo(serviceProvider);
        await userInfo.Setup(httpContext);
        return userInfo;
    }
    
    private async Task Setup(HttpContext httpContext)
    {
        var httpContextUser = httpContext.User;
        var claims = httpContextUser.Claims.ToList();

        if (httpContextUser.Identity == null)
        {
            return;
        }

        UserExternalId = claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value;
        Name = claims.First(c => c.Type == ClaimTypes.Name).Value;
        Email = claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;

        // Create a new user in database if it doesn't exist, or load its settings
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var user = await context.AppAccounts.FirstOrDefaultAsync(x => x.CorrelationId == UserExternalId);
        if (user == null)
        {
            IsAccountJustCreated = true;
            user = new AppAccountEntity
            {
                CorrelationId = UserExternalId,
                Name = Name
            };
            context.AppAccounts.Add(user);
            await context.SaveChangesAsync();
        }

        SWGServerName = user.SWGServerName;

        // Get the avatar URL
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
    }

    public bool IsAuthenticatedUser => UserExternalId != null;
    public bool IsGuest => UserExternalId == null;
    public bool IsAccountJustCreated { get; set; }
    
    internal AppAccountEntity BuildAppAccount(ApplicationDbContext context)
    {
        return IsGuest ? AppAccountEntity.Guest : context.AppAccounts.Where(x => x.CorrelationId == UserExternalId)
            .Include(a => a.Crew)
            .Include(a => a.GameAccounts)
            .FirstOrDefault();    
    }

    public ThemeMode ThemeMode
    {
        get
        {
            if (IsGuest)
            {
                return ThemeMode.Auto;
            }

            if (_themeMode == null)
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var appAccount = BuildAppAccount(context);
                _themeMode = appAccount?.ThemeMode ?? ThemeMode.Auto;
            }
            return _themeMode.Value;
        }
        set
        {
            if (IsGuest)
            {
                return;
            }
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var appAccount = BuildAppAccount(context);
            if (appAccount == null || appAccount.ThemeMode == value)
            {
                return;
            }
            
            appAccount.ThemeMode = value;
            context.AppAccounts.Update(appAccount);
            context.SaveChanges();
            _themeMode = value;
        }
    }

    public string UserExternalId { get; private set; }
    public string Name { get; private set; }
    public string AvatarUrl { get; private set; }
    public string SWGServerName { get; private set; }
    
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public string Email { get; private set; }

    public static UserInfo GetUnauthenticated(IServiceProvider serviceProvider)
    {
        return new UserInfo(serviceProvider)
        {
            Name = "Guest"
        };
    }
}