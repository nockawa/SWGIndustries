using AspNet.Security.OAuth.Discord;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using MudBlazor.Services;
using SWGIndustries.Components;
using SWGIndustries.Components.Account;
using SWGIndustries.Data;
using SWGIndustries.Services;

namespace SWGIndustries;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        var services = builder.Services;

        // MudBlazor services
        services.AddMudServices(config =>
        {
            config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomLeft;
            config.SnackbarConfiguration.PreventDuplicates = true;
            config.SnackbarConfiguration.NewestOnTop = true;
            config.SnackbarConfiguration.ShowCloseIcon = true;
            config.SnackbarConfiguration.VisibleStateDuration = 5000;
        });

        services.AddRazorComponents()
            .AddInteractiveServerComponents();

        // Razor/Blazor services
        services.AddRazorPages();
        services.AddCascadingAuthenticationState();
        services.AddHttpContextAccessor();
        
        services.AddSingleton<UserManager>();

        //Configure authentication for the user using Discord OAuth2
        services.AddAuthentication(opt =>
            {
                opt.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                opt.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                opt.DefaultChallengeScheme = DiscordAuthenticationDefaults.AuthenticationScheme;
            })
            .AddDiscord(options =>
            {
                var section  = builder.Configuration.GetSection("Authentication:Discord");
                options.ClientId = section["ClientId"]!;
                options.ClientSecret = section["ClientSecret"]!;
                
                options.CorrelationCookie.SameSite = SameSiteMode.Lax;
                options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;
                    
                //Required for accessing the oauth2 token in order to make requests on the user's behalf, ie. accessing the user's guild list
                options.SaveTokens = true;
                options.Scope.Add("identify");
                options.Scope.Add("email");
            })
            .AddGoogle(options =>
            {
                var section  = builder.Configuration.GetSection("Authentication:Google");
                options.ClientId = section["ClientId"]!;
                options.ClientSecret = section["ClientSecret"]!;

                options.SaveTokens = true;
                options.Scope.Add("profile");
            })
            .AddCookie();

        // Database and EF setup
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                               throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite(connectionString));
        builder.Services.AddDatabaseDeveloperPageExceptionFilter();
        
        // Build the application
        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            // app.UseDeveloperExceptionPage();
            app.UseExceptionHandler("/Error", createScopeForErrors: true);
            app.UseMigrationsEndPoint();
            app.UseHsts();
        }
        else
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        //app.UseHttpsRedirection();

        app.UseRouting();
        
        app.UseAntiforgery();

        app.MapStaticAssets();
        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        app.UseAuthentication();
        app.UseAuthorization();


        app.MapAdditionalAccountEndpoints();
        
        app.Run();
    }
}