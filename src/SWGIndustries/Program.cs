using AspNet.Security.OAuth.Discord;
using Microsoft.AspNetCore.Authentication.Cookies;
using SWGIndustries.Components;
using SWGIndustries.Components.Account;
using SWGIndustries.data;

namespace SWGIndustries;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        var services = builder.Services;
        services.AddRazorComponents()
            .AddInteractiveServerComponents();

        services.AddRazorPages();
        services.AddCascadingAuthenticationState();
        services.AddHttpContextAccessor();
        services.AddSingleton<UserService>();

        //Configure authentication for the user
        services.AddAuthentication(opt =>
            {
                opt.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                opt.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                opt.DefaultChallengeScheme = DiscordAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie()
            .AddDiscord(options =>
            {
                var section  = builder.Configuration.GetSection("Authentication:Discord");
                options.ClientId = section["ClientId"];
                options.ClientSecret = section["ClientSecret"];
                
                options.CorrelationCookie.SameSite = SameSiteMode.Lax;
                options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;
                    
                //Required for accessing the oauth2 token in order to make requests on the user's behalf, ie. accessing the user's guild list
                options.SaveTokens = true;
                options.Scope.Add("identify");
                options.Scope.Add("email");
            });

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        //app.UseHttpsRedirection();
        

        app.MapStaticAssets();
        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        app.UseAuthentication();
        app.UseAuthorization();

        //app.UseRouting();
        app.UseAntiforgery();

        //app.MapBlazorHub();
        //app.MapDefaultControllerRoute();

        app.MapAdditionalAccountEndpoints();
        
        app.Run();
    }
}