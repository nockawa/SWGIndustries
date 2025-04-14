using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SWGIndustries.Data;

namespace SWGIndustries.Services;

[PublicAPI]
public class InventoryService : IDisposable, IAsyncDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly UserService _userService;
    private readonly AdminService _adminService;
    private readonly NamedSeriesService _namedSeriesService;
    private readonly ILogger<InventoryService> _logger;
    
    public ApplicationDbContext DbContext => _context;

    public InventoryService(ApplicationDbContext context, UserService userService, AdminService adminService, NamedSeriesService namedSeriesService, 
        ILogger<InventoryService> logger)
    {
        _context = context;
        _userService = userService;
        _adminService = adminService;
        _namedSeriesService = namedSeriesService;
        _logger = logger;
    }

    public void Dispose()
    {
        _context?.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        if (_context != null) await _context.DisposeAsync();
    }
    
    public IDbContextTransaction BeginTransaction()
    {
        return DbContext.Database.BeginTransaction();
    }

    public bool SaveChanges(IDbContextTransaction transaction) 
    {
        var res = DbContext.SaveChanges() > 0;
        if (transaction != null)
        {
            transaction.Commit();
            transaction.Dispose();
        }

        return res;
    }

    public async Task<(bool, string)> CreateHouse(GameAccountEntity owner, House house, int count)
    {
        for (int i = 0; i < count; i++)
        {
            var building = new BuildingEntity()
            {
                Name = $"{house.Class} #{await _namedSeriesService.GetNextValueAsync(house.Class)}",
                Owner = owner,
                FullClass = house.FullClass,
            };
            
            DbContext.Buildings.Add(building);
        }

        return await DbContext.SaveChangesAsync() > 0 ? (true, $"{count} {house.Class} created.") : (false, "error creating the house objects");
    }

    public async Task<(bool res, string info)> CreateFactory(GameAccountEntity owner, Factory factory, int count)
    {
        for (int i = 0; i < count; i++)
        {
            var building = new BuildingEntity()
            {
                Name = $"{factory.Class} #{await _namedSeriesService.GetNextValueAsync(factory.Class)}",
                Owner = owner,
                FullClass = factory.FullClass,
            };
            
            DbContext.Buildings.Add(building);
        }

        return await DbContext.SaveChangesAsync() > 0 ? (true, $"{count} {factory.Class} created.") : (false, "error creating the factory objects");
    }

    public async Task<(bool res, string info)> CreateHarvester(GameAccountEntity owner, Harvester harvester, HarvesterCreationProperties properties, int count)
    {
        var hrt = properties.HarvestingResourceType.ToString();
        for (var i = 0; i < count; i++)
        {
            var building = new BuildingEntity()
            {
                Name = $"{harvester.Class} {hrt} #{await _namedSeriesService.GetNextValueAsync($"{harvester.Class}-{hrt}")}",
                Owner = owner,
                FullClass = harvester.FullClass,
                HarvesterHopperSize = properties.HopperSizeK,
                HarvesterBER = properties.BER,
                HarvesterSelfPowered = properties.SelfPowered,
                HarvestingResourceType = properties.HarvestingResourceType
            };
            
            DbContext.Buildings.Add(building);
        }

        return await DbContext.SaveChangesAsync() > 0 ? (true, $"{count} {harvester.Class} {hrt} created.") : (false, "error creating the factory objects");
    }

    public async Task<(bool res, string info)> CreateEnergyHarvester(GameAccountEntity owner, EnergyHarvester harvester, HarvesterCreationProperties properties, int count)
    {
        var hrt = properties.HarvestingResourceType.ToString();
        for (var i = 0; i < count; i++)
        {
            var building = new BuildingEntity()
            {
                Name = $"{harvester.Class} {hrt} #{await _namedSeriesService.GetNextValueAsync($"{harvester.Class}-{hrt}")}",
                Owner = owner,
                FullClass = harvester.FullClass,
                HarvesterHopperSize = properties.HopperSizeK,
                HarvesterBER = properties.BER,
                HarvestingResourceType = properties.HarvestingResourceType
            };
            
            DbContext.Buildings.Add(building);
        }

        return await DbContext.SaveChangesAsync() > 0 ? (true, $"{count} {harvester.Class} {hrt} created.") : (false, "error creating the factory objects");
    }

    public async Task<(bool res, string info)> PutDownBuilding(BuildingEntity building, CharacterEntity character, bool isForCrew, Planet planet, ClusterEntity cluster)
    {
        building.PutDownBy = character;
        building.BuildingForCrew = isForCrew;
        building.PutDownPlanet = planet;
        building.PutDownDateTime = DateTime.UtcNow;
        building.Cluster = cluster;

        DbContext.Buildings.Update(building);
        var res = await DbContext.SaveChangesAsync() > 0;
        if (!res)
        {
            return (false, $"Failed to put down building {building} by {character}");
        }
        
        return (true, $"Building {building} put down by {character}");
    }
    
    public bool StartHarvester(BuildingEntity harvester, DateTime? startTime = null)
    {
        if (harvester.IsRunning)
        {
            return false;
        }
        
        harvester.IsRunning = true;
        harvester.LastRunningDateTime = startTime ?? DateTime.UtcNow;
        DbContext.Buildings.Update(harvester);
        return true;
    }
    
    public async Task<List<BuildingEntity>> GetBuildings(DataScope scope, string classFilter=null, bool? putDown = null)
    {
        // Return everything, shouldn't be used in the WebApp
        if (scope == null)
        {
            IQueryable<BuildingEntity> q = DbContext.Buildings
                .Include(b => b.Owner).Include(b => b.PutDownBy).Include(b => b.Cluster);
            if (classFilter != null)
            {
                q = q.Where(b => b.FullClass.StartsWith(classFilter));
            }

            if (putDown != null)
            {
                q = q.Where(b => putDown == (b.PutDownBy!=null));
            }
            return await q.ToListAsync();
        }
        
        if (scope.IsCrew)
        {
            IQueryable<BuildingEntity> q = DbContext.Buildings.Include(b => b.Owner)
                .Include(b => b.PutDownBy).Include(b => b.Cluster)
                .Where(b => b.BuildingForCrew && scope.Crew.Members.Contains(b.Owner.OwnerAppAccount));
            
            if (classFilter != null)
            {
                q = q.Where(b => b.FullClass.StartsWith(classFilter));
            }
            if (putDown != null)
            {
                q = q.Where(b => putDown == (b.PutDownBy!=null));
            }
                
            return await q.ToListAsync();
        }

        if (scope.IsAppAccount)
        {
            IQueryable<BuildingEntity> q = DbContext.Buildings.Include(b => b.Owner)
                .Include(b => b.PutDownBy).Include(b => b.Cluster)
                .Where(b => b.Owner.OwnerAppAccount == scope.AppAccount);
                
            if (putDown != null)
            {
                q = q.Where(b => putDown == (b.PutDownBy!=null));
            }
            
            return await q.ToListAsync();
        }
        
        if (scope.IsGameAccount)
        {
            IQueryable<BuildingEntity> q = DbContext.Buildings
                .Include(b => b.Owner).Include(b => b.PutDownBy).Include(b => b.Cluster)
                .Where(b => b.Owner == scope.GameAccount);

            if (putDown != null)
            {
                q = q.Where(b => putDown == (b.PutDownBy!=null));
            }
            
            return await q.ToListAsync();
        }

        throw new InvalidOperationException();
    }

    public void UpdateBuilding(BuildingEntity editingBuilding)
    {
        DbContext.Buildings.Update(editingBuilding);
    }
    
    public void ReloadBuilding(BuildingEntity building)
    {
        DbContext.Entry(building).Reload();
    }
    
    public async Task<IList<BuildingEntity>> GetAvailableHarvesters(GameAccountEntity gameAccount, HarvestingResourceType resourceType)
    {
        if (gameAccount == null)
        {
            var appAccount = await _adminService.GetAppAccountAsync();
        
            return await DbContext.Buildings
                .Include(b => b.Owner)
                .Include(b => b.PutDownBy)
                .Include(b => b.Cluster)
                .Where(b => b.Owner.OwnerAppAccount == appAccount && b.FullClass.StartsWith(StructureClasses.Harvester) && b.Cluster == null && 
                            b.PutDownBy == null && b.HarvestingResourceType == resourceType)
                .ToListAsync();
        }
        else
        {
            return await DbContext.Buildings
                .Include(b => b.Owner)
                .Include(b => b.PutDownBy)
                .Include(b => b.Cluster)
                .Where(b => b.Owner == gameAccount && b.FullClass.StartsWith(StructureClasses.Harvester) && b.Cluster == null && 
                            b.PutDownBy == null && b.HarvestingResourceType == resourceType)
                .ToListAsync();
        }
    }
    
    public async Task<ResourceEntity> GetResource(int swgAideId)
    {
        return await DbContext.Resources.FirstOrDefaultAsync(r => r.SWGAideId == swgAideId);
    }

    public async Task<bool> CreateCluster(ClusterEntity cluster)
    {
        DbContext.Clusters.Add(cluster);
        return await DbContext.SaveChangesAsync() > 0;
    }

    public async Task<ClusterEntity> GetCluster(int id)
    {
        return await DbContext.Clusters.Where(c => c.Id == id)
            .Include(c => c.Resource)
            .Include(c => c.Crew)
            .Include(c => c.GameAccount).FirstOrDefaultAsync();
    }

    public async Task<IList<ClusterEntity>> GetClusters(AppAccountEntity appAccount)
    {
        // Return everything, shouldn't be used in the WebApp
        if (appAccount == null)
        {
            return await DbContext.Clusters
                .Include(c => c.Resource)
                .Include(c => c.Crew)
                .Include(c => c.GameAccount)
                .ToListAsync();
        }
        
        return await DbContext.Clusters
            .Where(c => 
                (c.GameAccount!=null && appAccount.GameAccounts.Contains(c.GameAccount)) ||
                (c.Crew!=null && c.Crew.Members.Contains(appAccount)))
            .Include(c => c.Resource)
            .Include(c => c.Crew)
            .Include(c => c.GameAccount)
            .ToListAsync();
    }

    public void UpdateCluster(ClusterEntity cluster)
    {
        DbContext.Clusters.Update(cluster);        
    }
}