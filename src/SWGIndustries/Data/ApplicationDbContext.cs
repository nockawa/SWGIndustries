using Microsoft.EntityFrameworkCore;

namespace SWGIndustries.Data;

public class ApplicationDbContext : DbContext
{
    public DbSet<ApplicationUser> ApplicationUsers { get; set; }
    public DbSet<SWGAccount> SWGAccounts { get; set; }
    public DbSet<SWGCharacter> SWGCharacters { get; set; }
    public DbSet<Crew> Crews { get; set; }
    public DbSet<CrewInvitation> CrewInvitations { get; set; }
    
    public ApplicationDbContext()
    {
        
    }
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Application User navigation customization
        modelBuilder.Entity<ApplicationUser>().HasMany(e => e.SWGAccounts).WithOne(e => e.OwnerApplicationUser);
        
        modelBuilder.Entity<ApplicationUser>().Navigation(a => a.SWGAccounts).AutoInclude();

        // SWG Account navigation customization
        modelBuilder.Entity<SWGAccount>()
            .HasMany(a => a.SWGCharacters)
            .WithOne(c => c.SWGAccount)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<SWGAccount>()
            .HasMany(a => a.Clusters)
            .WithOne(c => c.Owner)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<SWGAccount>()
            .HasMany(a => a.SWGBuildings)
            .WithOne(b => b.Owner)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<SWGAccount>().Navigation(a => a.SWGCharacters).AutoInclude();

        modelBuilder.Entity<SWGCharacter>()
            .HasMany(c => c.PutDownBuildings)
            .WithOne(b => b.PutDownBy)
            .OnDelete(DeleteBehavior.NoAction);
        
        modelBuilder.Entity<Cluster>()
            .HasMany(c => c.Buildings)
            .WithOne(b => b.Cluster)
            .OnDelete(DeleteBehavior.NoAction);
        
        // Crew navigation customization
        modelBuilder.Entity<Crew>().HasMany(c => c.Members).WithOne(m => m.Crew);
        modelBuilder.Entity<Crew>().Navigation(c => c.CrewLeader).AutoInclude();
        modelBuilder.Entity<Crew>().Navigation(c => c.Members).AutoInclude();
        
        // Crew Invitation navigation customization
        modelBuilder.Entity<CrewInvitation>().Navigation(ci => ci.FromUser).AutoInclude();
        modelBuilder.Entity<CrewInvitation>().Navigation(ci => ci.ToUser).AutoInclude();
    }
}