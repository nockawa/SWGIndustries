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
            config.SnackbarConfiguration.PreventDuplicates = false;
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
        
        services.AddSingleton<UserService>();
        services.AddScoped<DataAccessService>();
        services.AddSingleton<StructuresService>();
        services.AddScoped<NamedSeriesService>();
        services.AddScoped<DataScopeService>();

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
        {
            options.UseSqlite(connectionString, sqliteOptions => sqliteOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
            //options.UseSeeding((context, _) => { GenTestData(context); });
            //options.EnableSensitiveDataLogging();
        });
        
        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        services.AddSingleton<ResourceManagerService>();
        services.AddHostedService<ResourceUpdateBackgroundService>();
        
        if (builder.Environment.IsDevelopment())
        {
            builder.Services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.AddSeq(builder.Configuration.GetSection("Seq"));
            });
        }
        
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

    private static readonly string[] AccountTestNames =
    [
        "Luke Skywalker", "Darth Vader", "Leia Organa", "Han Solo", "Obi-Wan Kenobi", "Yoda", "Anakin Skywalker", "Padmé Amidala", 
        "Palpatine", "Chewbacca", "R2-D2", "C-3PO", "Lando Calrissian", "Boba Fett", "Jabba the Hutt", "Qui-Gon Jinn", "Mace Windu", 
        "Count Dooku", "Rey", "Kylo Ren", "Finn", "Poe Dameron", "Maz Kanata", "Snoke", "Jyn Erso", "Cassian Andor", "K-2SO", 
        "Chirrut Îmwe", "Baze Malbus", "Orson Krennic", "Saw Gerrera", "Galen Erso", "Bodhi Rook", "Mon Mothma", "Admiral Ackbar", 
        "Wedge Antilles", "Biggs Darklighter", "Greedo", "Nien Nunb", "Admiral Thrawn", "Ahsoka Tano", "Ezra Bridger", "Kanan Jarrus", 
        "Hera Syndulla", "Sabine Wren", "Garazeb Orrelios", "Agent Kallus", "Grand Inquisitor", "Asajj Ventress", "Savage Opress", 
        "Mother Talzin", "Cad Bane", "Hondo Ohnaka", "Bo-Katan Kryze", "Pre Vizsla", "Satine Kryze", "Plo Koon", "Kit Fisto", 
        "Shaak Ti", "Aayla Secura", "Barriss Offee", "Luminara Unduli", "Ki-Adi-Mundi", "Saesee Tiin", "Eeth Koth", "Adi Gallia", 
        "Depa Billaba", "Quinlan Vos", "Jocasta Nu", "Bail Organa", "Wicket W. Warrick", "Nute Gunray", "Wat Tambor", "Poggle the Lesser", 
        "San Hill", "Shmi Skywalker", "Cliegg Lars", "Owen Lars", "Beru Whitesun Lars", "Dexter Jettster", "Zam Wesell", "Jango Fett", 
        "Bossk", "Dengar", "IG-88", "4-LOM", "Zuckuss", "Embo", "Aurra Sing", "Rex", "Fives", "Echo", "Jesse", "Gregor", "Wolffe", 
        "Cody", "Fox", "Gree", "Bly"
    ];

    /*
    private static void GenTestData(DbContext context)
    {
        foreach (var name in AccountTestNames)
        {
            var appUser = new AppAccountEntity
            {
                CorrelationId = $"{name}TestData",
                Name = name
            };
            context.Set<AppAccountEntity>().Add(appUser);
        }
        context.SaveChanges();
        var testAppUser = context.Set<AppAccountEntity>().FirstOrDefault(a => a.CorrelationId == "This is test data");
        if (testAppUser != null)
        {
            var testAccounts = context.Set<GameAccountEntity>().Where(a => a.OwnerAppAccount == testAppUser).ToList();
            context.Set<GameAccountEntity>().RemoveRange(testAccounts);
            context.SaveChanges();
                    
            context.Set<AppAccountEntity>().Remove(testAppUser);
            context.SaveChanges();
        }
        foreach (var applicationUser in context.Set<AppAccountEntity>().Where(a => a.CorrelationId == "This is test data").ToList())
        {
            context.Set<AppAccountEntity>().Remove(applicationUser);
        }
        context.SaveChanges();
                
        var appUser = new AppAccountEntity
        {
            CorrelationId = "This is test data"
        };
        context.Set<AppAccountEntity>().Add(appUser);

        foreach (var name in AccountTestNames)
        {
            var account = new GameAccountEntity
            {
                Name = name,
                OwnerAppAccount = appUser
            };
            context.Set<GameAccountEntity>().Add (account);
                    
            context.Set<CharacterEntity>().Add(new CharacterEntity
            {
                Name = name,
                GameAccount = account,
            });
        }
        context.SaveChanges();
    }
*/
}