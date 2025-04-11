using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SWGIndustries.Data;

namespace SWGIndustries.Services;

[PublicAPI]
public class HarvesterCreationProperties
{
    public HarvesterCreationProperties(int hopperSizeK, int ber, bool selfPowered, HarvestingResourceType harvestingResourceType)
    {
        HopperSizeK = hopperSizeK;
        BER = ber;
        SelfPowered = selfPowered;
        HarvestingResourceType = harvestingResourceType;
    }

    public HarvesterCreationProperties()
    {
    }

    public int HopperSizeK { get; set; }
    public int BER { get; set; }
    public bool SelfPowered { get; set; }
    public HarvestingResourceType HarvestingResourceType { get; set; }
}

[PublicAPI]
public sealed class DataAccessService : IDisposable, IAsyncDisposable
{
    private readonly AdminService _adminService;
    private readonly NamedSeriesService _namedSeriesService;

    /// <summary>
    /// Should be used for read-only operations, or very carefully...
    /// </summary>
    public ApplicationDbContext DbContext { get; }

    public DataAccessService(ApplicationDbContext dbContext, UserService userService, AdminService adminService, NamedSeriesService namedSeriesService)
    {
        DbContext = dbContext;
        _adminService = adminService;
        _namedSeriesService = namedSeriesService;
    }

    public void Dispose()
    {
        DbContext.Dispose();
    }

    public async ValueTask DisposeAsync() => await DbContext.DisposeAsync();

    public async Task<(bool, string)> CreateHouse(GameAccountEntity owner, House house, int count)
    {
        for (int i = 0; i < count; i++)
        {
            var building = new BuildingEntity()
            {
                Name = $"{house.Name} #{await _namedSeriesService.GetNextValueAsync(house.Name)}",
                Owner = owner,
                Type = house.Type,
                SubType = house.SubType
            };
            
            DbContext.Buildings.Add(building);
        }

        return await DbContext.SaveChangesAsync() > 0 ? (true, $"{count} {house.Name} created.") : (false, "error creating the house objects");
    }

    public async Task<(bool res, string info)> CreateFactory(GameAccountEntity owner, Factory factory, int count)
    {
        for (int i = 0; i < count; i++)
        {
            var building = new BuildingEntity()
            {
                Name = $"{factory.Name} #{await _namedSeriesService.GetNextValueAsync(factory.Name)}",
                Owner = owner,
                Type = factory.Type,
                SubType = factory.SubType
            };
            
            DbContext.Buildings.Add(building);
        }

        return await DbContext.SaveChangesAsync() > 0 ? (true, $"{count} {factory.Name} created.") : (false, "error creating the factory objects");
    }

    public async Task<(bool res, string info)> CreateHarvester(GameAccountEntity owner, Harvester harvester, HarvesterCreationProperties properties, int count)
    {
        var hrt = properties.HarvestingResourceType.ToString();
        for (var i = 0; i < count; i++)
        {
            var building = new BuildingEntity()
            {
                Name = $"{harvester.Name} {hrt} #{await _namedSeriesService.GetNextValueAsync($"{harvester.Name}-{hrt}")}",
                Owner = owner,
                Type = harvester.Type,
                SubType = harvester.SubType,
                HarvesterHopperSize = properties.HopperSizeK,
                HarvesterBER = properties.BER,
                HarvesterSelfPowered = properties.SelfPowered,
                HarvestingResourceType = properties.HarvestingResourceType
            };
            
            DbContext.Buildings.Add(building);
        }

        return await DbContext.SaveChangesAsync() > 0 ? (true, $"{count} {harvester.Name} {hrt} created.") : (false, "error creating the factory objects");
    }

    public async Task<List<BuildingEntity>> GetBuildings(DataScope scope, BuildingType filterBy=BuildingType.Undefined, 
        BuildingSubType filterBySubType=BuildingSubType.Undefined, bool? putDown = null)
    {
        // Return everything, shouldn't be used in the WebApp
        if (scope == null)
        {
            IQueryable<BuildingEntity> q = DbContext.Buildings
                .Include(b => b.Owner).Include(b => b.PutDownBy).Include(b => b.Cluster);
            if (filterBy != BuildingType.Undefined)
            {
                q = q.Where(b => filterBy == b.Type);
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
            
            if (filterBy != BuildingType.Undefined)
            {
                q = q.Where(b => b.Type == filterBy);                
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

    public IDbContextTransaction BeginTransaction()
    {
        return DbContext.Database.BeginTransaction();
    }

    public void UpdateBuilding(BuildingEntity editingBuilding)
    {
        DbContext.Buildings.Update(editingBuilding);
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

    public void ReloadBuilding(BuildingEntity building)
    {
        DbContext.Entry(building).Reload();
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

    public async Task<IList<BuildingEntity>> GetAvailableHarvesters(AppAccountEntity appAccount, HarvestingResourceType resourceType)
    {
        appAccount ??= await _adminService.GetAppAccountAsync();
        
        return await DbContext.Buildings
            .Include(b => b.Owner)
            .Include(b => b.PutDownBy)
            .Include(b => b.Cluster)
            .Where(b => b.Owner.OwnerAppAccount == appAccount && b.Type==BuildingType.Harvester && b.Cluster == null && b.PutDownBy == null && b.HarvestingResourceType == resourceType)
            .ToListAsync();
    }
}