using Microsoft.EntityFrameworkCore;

namespace SWGIndustries.Data;

public class ApplicationDbContext : DbContext
{
    public DbSet<NamedSeriesEntity>     NamedSeries     { get; set; }
    public DbSet<AppAccountEntity>      AppAccounts     { get; set; }
    public DbSet<GameAccountEntity>     GameAccounts    { get; set; }
    public DbSet<CharacterEntity>       Characters      { get; set; }
    public DbSet<CrewEntity>            Crews           { get; set; }
    public DbSet<CrewInvitationEntity>  CrewInvitations { get; set; }
    public DbSet<BuildingEntity>        Buildings       { get; set; }
    public DbSet<ClusterEntity>         Clusters        { get; set; }
    
    public DbSet<ResourceEntity> Resources { get; set; }
    
    public ApplicationDbContext()
    {
        
    }
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Named series Entity
        modelBuilder.Entity<NamedSeriesEntity>().ToTable("NamedSeries");
        modelBuilder.Entity<NamedSeriesEntity>().HasIndex(p => p.Name).IsUnique();
        modelBuilder.Entity<NamedSeriesEntity>().Property(p => p.Counter).IsConcurrencyToken();
        
        // Application User Entity
        modelBuilder.Entity<AppAccountEntity>().ToTable("AppAccounts");
        modelBuilder.Entity<AppAccountEntity>()
            .HasMany(e => e.GameAccounts).WithOne(e => e.OwnerAppAccount);
        modelBuilder.Entity<AppAccountEntity>().Navigation(a => a.GameAccounts).AutoInclude();

        // Resource Entity
        modelBuilder.Entity<ResourceEntity>().ToTable("Resources");
        modelBuilder.Entity<ResourceEntity>().HasIndex(r => r.Name).IsUnique();
        modelBuilder.Entity<ResourceEntity>().HasIndex(r => r.DepletedSince);
        modelBuilder.Entity<ResourceEntity>().HasIndex(r => r.CategoryIndex);
        modelBuilder.Entity<ResourceEntity>().HasIndex(r => r.CI0);
        modelBuilder.Entity<ResourceEntity>().HasIndex(r => r.CI1);
        modelBuilder.Entity<ResourceEntity>().HasIndex(r => r.CI2);
        modelBuilder.Entity<ResourceEntity>().HasIndex(r => r.CI3);
        modelBuilder.Entity<ResourceEntity>().HasIndex(r => r.CI4);
        modelBuilder.Entity<ResourceEntity>().HasIndex(r => r.CI5);
        modelBuilder.Entity<ResourceEntity>().HasIndex(r => r.CI6);
        modelBuilder.Entity<ResourceEntity>().HasIndex(r => r.CI7);
        modelBuilder.Entity<ResourceEntity>().Property(r => r.Name).IsRequired();
        
        // Game Account Entity
        modelBuilder.Entity<GameAccountEntity>().ToTable("GameAccounts");
        modelBuilder.Entity<GameAccountEntity>()
            .HasMany(a => a.Characters)
            .WithOne(c => c.GameAccount)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<GameAccountEntity>()
            .HasMany(a => a.Clusters)
            .WithOne(c => c.GameAccount)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<GameAccountEntity>()
            .HasMany(a => a.Buildings)
            .WithOne(b => b.Owner)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<GameAccountEntity>().Navigation(a => a.Characters).AutoInclude();

        // Character Entity
        modelBuilder.Entity<CharacterEntity>().ToTable("Characters");
        modelBuilder.Entity<CharacterEntity>()
            .HasMany(c => c.PutDownBuildings)
            .WithOne(b => b.PutDownBy)
            .OnDelete(DeleteBehavior.NoAction);
        modelBuilder.Entity<CharacterEntity>().Navigation(c => c.PutDownBuildings).AutoInclude();
        
        // Building Entity
        modelBuilder.Entity<BuildingEntity>().ToTable("Buildings");
        modelBuilder.Entity<BuildingEntity>().HasIndex(b => b.FullClass);
        
        // Cluster Entity
        modelBuilder.Entity<ClusterEntity>().ToTable("Clusters");
        modelBuilder.Entity<ClusterEntity>().Property(p => p.CreationDateTime).HasDefaultValueSql("CURRENT_TIMESTAMP");
        modelBuilder.Entity<ClusterEntity>()
            .HasMany(c => c.Buildings)
            .WithOne(b => b.Cluster)
            .OnDelete(DeleteBehavior.NoAction);
        
        // Crew navigation customization
        modelBuilder.Entity<CrewEntity>().ToTable("Crews");
        modelBuilder.Entity<CrewEntity>()
            .HasMany(c => c.Members)
            .WithOne(m => m.Crew);
        modelBuilder.Entity<CrewEntity>()
            .HasMany(a => a.Clusters)
            .WithOne(c => c.Crew)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<CrewEntity>().Navigation(c => c.CrewLeader).AutoInclude();
        modelBuilder.Entity<CrewEntity>().Navigation(c => c.Members).AutoInclude();
        modelBuilder.Entity<CrewEntity>().HasIndex(c => c.Name).IsUnique();
        
        // Crew Invitation navigation customization
        modelBuilder.Entity<CrewInvitationEntity>().ToTable("CrewInvitations");
        modelBuilder.Entity<CrewInvitationEntity>().Navigation(ci => ci.FromAccount).AutoInclude();
        modelBuilder.Entity<CrewInvitationEntity>().Navigation(ci => ci.ToAccount).AutoInclude();
    }
}