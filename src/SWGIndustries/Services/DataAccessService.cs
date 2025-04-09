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
    private readonly NamedSeriesService _namedSeriesService;
    private readonly Task<AppAccountEntity> _appAccountTask;

    /// <summary>
    /// Should be used for read-only operations, or very carefully...
    /// </summary>
    public ApplicationDbContext DbContext { get; }

    public DataAccessService(ApplicationDbContext dbContext, UserService userService, NamedSeriesService namedSeriesService)
    {
        DbContext = dbContext;
        _appAccountTask = userService.BuildAppAccount(DbContext);
        _namedSeriesService = namedSeriesService;
    }

    public void Dispose()
    {
        DbContext.Dispose();
    }

    public async ValueTask DisposeAsync() => await DbContext.DisposeAsync();

    #region AppAcount operations

    public async Task<AppAccountEntity> GetAppAccountAsync() => await _appAccountTask;
    public AppAccountEntity GetAppAccount() => _appAccountTask.Result;
    
    public async Task<AppAccountEntity> GetAppAccountByName(string name, bool caseInsensitive)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }
        
        return await DbContext
            .AppAccounts.Include(a => a.Crew)
            .FirstOrDefaultAsync(u => caseInsensitive ? 
                u.Name.ToLower() == name.ToLower() : 
                u.Name == name);
    }

    public async Task<IEnumerable<AppAccountEntity>> GetAppAccountsByName(string nameFragment, bool caseInsensitive, CancellationToken cancellationToken)
    {
        return await DbContext.AppAccounts.Where(u => EF.Functions.Like(u.Name, $"%{nameFragment}%"))
            .ToListAsync(cancellationToken);
    }

    #endregion


    #region GameAccount operations

    public async Task<IList<GameAccountEntity>> GetUserGameAccounts()
    {
        var user = await GetAppAccountAsync();
        return user.GameAccounts;
        
    }

    public async Task<bool> AddGameAccount(GameAccountEntity gameAccount, bool fullRefresh)
    {
        var user = await GetAppAccountAsync();
        if (user.GameAccounts.Contains(gameAccount))
        {
            return false;
        }

        user.GameAccounts.Add(gameAccount);
        var res = await DbContext.SaveChangesAsync() > 0;
        
        if (fullRefresh)
        {
            await DbContext.Entry(user).Collection(u => u.GameAccounts).LoadAsync();
        }

        return res;
    }
    
    public async Task<bool> RemoveGameAccount(GameAccountEntity gameAccount, bool fullRefresh)
    {
        var user = await GetAppAccountAsync();
        if (!user.GameAccounts.Contains(gameAccount))
        {
            return false;
        }

        user.GameAccounts.Remove(gameAccount);
        DbContext.GameAccounts.Remove(gameAccount);
        
        var res = await DbContext.SaveChangesAsync() > 0;
        if (fullRefresh)
        {
            await DbContext.Entry(user).Collection(u => u.GameAccounts).LoadAsync();
        }
        return res;
    }
    
    public async Task RefreshGameAccounts() 
    {
        var user = await GetAppAccountAsync();
        await DbContext.Entry(user).Collection(u => u.GameAccounts).LoadAsync();
    }

    #endregion

    #region Character operations

    public async Task<bool> AddCharacter(GameAccountEntity gameAccount, CharacterEntity character, bool fullRefresh)
    {
        gameAccount.Characters.Add(character);
        var res = await DbContext.SaveChangesAsync() > 0;
        if (fullRefresh)
        {
            await DbContext.Entry(gameAccount).Collection(u => u.Characters).LoadAsync();
        }
        return res;
    }
    
    public async Task<bool> RemoveCharacter(CharacterEntity character, bool fullRefresh)
    {
        var gameAccount = character.GameAccount;
        gameAccount?.Characters.Remove(character);
        DbContext.Characters.Remove(character);
        
        var res = await DbContext.SaveChangesAsync() > 0;
        if (gameAccount!=null && fullRefresh)
        {
            await DbContext.Entry(gameAccount).Collection(u => u.Characters).LoadAsync();
        }
        return res;
    }
    
    public async Task RefreshCharacters(GameAccountEntity gameAccount)
    {
        await DbContext.Entry(gameAccount).Collection(u => u.Characters).LoadAsync();
    }

    #endregion

    #region Crew

    /// <summary>
    /// Create a new crew with the given leader
    /// </summary>
    /// <param name="leader"></param>
    /// <returns></returns>
    public async Task<(CrewEntity, string)> CreateCrew(AppAccountEntity leader, string crewName)
    {
        // Check if the leader is already a member of another crew, which can't be
        if (leader.Crew != null)
        {
            return (null, $"Can't create a crew, {leader.Name} is already a member of another one");
        }
        
        // Check if the crew name too long
        if (crewName.Length > CrewEntity.CrewNameMaxLength)
        {
            return (null, $"Crew name too long, max length is {CrewEntity.CrewNameMaxLength}");
        }
        
        // Create the crew, set its leader and add it to the database
        var crew = new CrewEntity
        {
            CrewLeader = leader,
            Name = crewName
        };
        DbContext.Crews.Add(crew);
        
        // Set the leader as a member of the crew
        leader.Crew = crew;
        DbContext.AppAccounts.Update(leader);

        try
        {
            // Save the changes to the database
            await DbContext.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return (null, "Failed to create the crew, most likely the name already exists");
        }
        
        return (crew, $"Crew '{crewName}' created with {leader.Name} as its leader");
    }

    /// <summary>
    /// Add a member to the crew
    /// </summary>
    /// <param name="member">Member to add</param>
    /// <param name="crew">Crew the member will join</param>
    /// <returns>Success flag and corresponding message</returns>
    public async Task<(bool, string)> AddCrewMember(AppAccountEntity member, CrewEntity crew)
    {
        // Check if the member is already a member of another crew
        if (member.Crew != null)
        {
            return (false, $"{member.Name} is already a member of another crew");
        }
        
        // Add the crew to the member, update and save
        member.Crew = crew;
        DbContext.AppAccounts.Update(member);
        var res = await DbContext.SaveChangesAsync() > 0;
        if (res == false)
        {
            return (false, $"Failed to add {member.Name} to crew {crew.Name}");
        }
        
        // Refresh the crew members list
        await DbContext.Entry(crew).Collection(c => c.Members).LoadAsync();
        
        return (true, $"{member.Name} added to crew {crew.Name}");
    }

    public async Task<(bool, string)> LeaveCrew(AppAccountEntity member)
    {
        if (member.Crew == null)
        {
            return (true, $"{member.Name} is not a member of any crew");
        }
        
        // Remove the member from the crew
        var crew = member.Crew;
        member.Crew = null;
        DbContext.AppAccounts.Update(member);
        var res = await DbContext.SaveChangesAsync() > 0;
        if (res == false)
        {
            return (false, $"Failed to remove {member.Name} from crew");
        }
        
        // Refresh the crew members list
        await DbContext.Entry(crew).Collection(c => c.Members).LoadAsync();
        return (true, $"{member.Name} removed from crew {crew.Name}");
    }
    
    public async Task<CrewEntity> GetCrewByName(string crewName, bool caseInsensitive)
    {
        if (string.IsNullOrEmpty(crewName))
        {
            return null;
        }
        return await DbContext.Crews.FirstOrDefaultAsync(c => caseInsensitive ? 
            c.Name.ToLower() == crewName.ToLower() : 
            c.Name == crewName);
    }

    public async Task<CrewEntity> GetCrewByLeaderAccountName(string leaderAccountName, bool caseInsensitive)
    {
        return (await GetAppAccountByName(leaderAccountName, caseInsensitive))?.Crew;
    }
    
    #endregion

    #region Crew Invitations
    
    public async Task<(CrewInvitationEntity, string)> CreateRequestToJoinCrew(AppAccountEntity leader, AppAccountEntity requester)
    {
        var crewInvitation = new CrewInvitationEntity
        {
            FromAccount = requester,
            ToAccount = leader,
            InviteOrRequestToJoin = false,
            Status = InvitationStatus.Pending
        };
        
        if (await DbContext.CrewInvitations.AnyAsync(
                          ci => (ci.InviteOrRequestToJoin == false && ci.FromAccount == crewInvitation.FromAccount) ||
                                (ci.InviteOrRequestToJoin == true && ci.ToAccount == crewInvitation.FromAccount)))
        {
            return (null, $"There is already an invitation pending for {crewInvitation.FromAccount.Name}");
        }

        DbContext.CrewInvitations.Add(crewInvitation);
        var res = await DbContext.SaveChangesAsync() > 0;
        return res ? 
            (crewInvitation, $"Crew invitation sent to {leader.Name}") : 
            (null, $"Failed to send crew invitation to {leader.Name}");
    }
    
    public async Task<(CrewInvitationEntity, string)> CreateInvitationToJoinCrew(AppAccountEntity leader, AppAccountEntity invitedAccount)
    {
        await DbContext.Entry(invitedAccount).Reference(i => i.Crew).LoadAsync();
        if (invitedAccount.Crew != null)
        {
            return (null, $"{invitedAccount.Name} is already a member of another crew");
        }
        
        var crewInvitation = new CrewInvitationEntity
        {
            FromAccount = leader,
            ToAccount = invitedAccount,
            InviteOrRequestToJoin = true,
            Status = InvitationStatus.Pending
        };
        
        if (await DbContext.CrewInvitations.AnyAsync(
                ci => (ci.InviteOrRequestToJoin == false && ci.FromAccount == crewInvitation.ToAccount) ||
                      (ci.InviteOrRequestToJoin == true && ci.ToAccount == crewInvitation.ToAccount)))
        {
            return (null, $"There is already an invitation pending for {invitedAccount.Name}");
        }

        DbContext.CrewInvitations.Add(crewInvitation);
        var res = await DbContext.SaveChangesAsync() > 0;
        return res ? 
            (crewInvitation, $"Crew invitation sent to {invitedAccount.Name}") : 
            (null, $"Failed to send crew invitation to {invitedAccount.Name}");
    }
    
    public async Task<List<CrewInvitationEntity>> GetPendingCrewRequests(AppAccountEntity appAccount)
    {
        return await DbContext.CrewInvitations
            .Where(
                // Invitation must be in pending state
                ci => ci.Status==InvitationStatus.Pending && 
                      
                // And either at the request of the appAccount or an invitation of the crew leader       
                ((ci.InviteOrRequestToJoin==false && ci.FromAccount == appAccount) || (ci.InviteOrRequestToJoin && ci.ToAccount == appAccount)))
                .ToListAsync();
    }

    public async Task<bool> DeleteCrewInvitation(CrewInvitationEntity crewInvitation)
    {
        DbContext.CrewInvitations.Remove(crewInvitation);
        return await DbContext.SaveChangesAsync() > 0;
    }

    public async Task<IList<CrewInvitationEntity>> GetCrewRequestForLeader(AppAccountEntity crewLeader)
    {
        return await DbContext.CrewInvitations
            .Where(ci => ci.Status == InvitationStatus.Pending && ci.ToAccount == crewLeader && ci.InviteOrRequestToJoin==false)
            .ToListAsync();
    }

    public async Task<(bool, string)> ProcessRequestToJoinCrew(CrewInvitationEntity invitation, bool grantOrReject)
    {
        var crewLeader = invitation.ToAccount;
        var crewMember = invitation.FromAccount;

        await DbContext.Entry(crewLeader).Reference(c => c.Crew).LoadAsync();
        await DbContext.Entry(crewMember).Reference(c => c.Crew).LoadAsync();
        
        if (crewMember.Crew == crewLeader.Crew)
        {
            return (true, $"{crewMember.Name} is already a member of the crew");
        }

        if (grantOrReject && crewMember.Crew != null)
        {
            return (false, $"Can't grant the request, {crewMember.Name} is already a member of another crew, he must first leave it.");
        }

        invitation.Status = grantOrReject ? InvitationStatus.Accepted : InvitationStatus.Rejected;
        DbContext.CrewInvitations.Update(invitation);
        if (await DbContext.SaveChangesAsync() == 1 == false)
        {
            return (false, "Failed to update the invitation status");
        }

        var res = true;
        if (grantOrReject)
        {
            crewMember.Crew = crewLeader.Crew;
            DbContext.AppAccounts.Update(crewMember);
            res = await DbContext.SaveChangesAsync() > 0;
        }

        var actionText = grantOrReject ? "accepted" : "rejected";
        return (res, res ? $"Invitation from {invitation.FromAccount.Name} is {actionText}." : $"Failed to {actionText} the invitation");
    }

    public async Task<(bool, string)> ProcessInvitationToJoinCrew(CrewInvitationEntity invitation, bool grantOrReject)
    {
        var crewLeader = invitation.FromAccount;
        var crewMember = invitation.ToAccount;

        await DbContext.Entry(crewLeader).Reference(c => c.Crew).LoadAsync();
        await DbContext.Entry(crewMember).Reference(c => c.Crew).LoadAsync();
        
        if (crewMember.Crew == crewLeader.Crew)
        {
            return (true, $"{crewMember.Name} is already a member of the crew");
        }

        if (grantOrReject && crewMember.Crew != null)
        {
            return (false, $"Can't grant the request, {crewMember.Name} is already a member of another crew, he must first leave it.");
        }

        invitation.Status = grantOrReject ? InvitationStatus.Accepted : InvitationStatus.Rejected;
        DbContext.CrewInvitations.Update(invitation);
        if (await DbContext.SaveChangesAsync() == 1 == false)
        {
            return (false, "Failed to update the invitation status");
        }

        bool res = true;
        if (grantOrReject)
        {
            crewMember.Crew = crewLeader.Crew;
            DbContext.AppAccounts.Update(crewMember);
            res = await DbContext.SaveChangesAsync() > 0;
        }

        var actionText = grantOrReject ? "accepted" : "rejected";
        return (res, res ? $"Invitation from {invitation.FromAccount.Name} is {actionText}." : $"Failed to {actionText} the invitation");
    }

    public async Task<CrewInvitationEntity> GetAnsweredCrewRequest()
    {
        var appAccount = await GetAppAccountAsync();
        return await DbContext.CrewInvitations
            .FirstOrDefaultAsync(ci => ci.Status!=InvitationStatus.Pending && ci.InviteOrRequestToJoin==false && ci.FromAccount==appAccount);
    }
    
    public async Task<IList<CrewInvitationEntity>> GetAnsweredCrewInvitations()
    {
        var appAccount = await GetAppAccountAsync();
        return await DbContext.CrewInvitations
            .Where(ci => ci.Status!=InvitationStatus.Pending && ci.InviteOrRequestToJoin && ci.FromAccount==appAccount).ToListAsync();
    }
    
    public async Task<bool> CloseCrewInvitation(CrewInvitationEntity crewInvitation)
    {
        DbContext.CrewInvitations.Remove(crewInvitation);
        return await DbContext.SaveChangesAsync() > 0;
    }

    public async Task<IList<CharacterEntity>> GetCrewCharacters(CrewEntity crew)
    {
        return await DbContext.AppAccounts
            .Where(a => a.Crew == crew)
            .SelectMany(a => a.GameAccounts)
            .SelectMany(c => c.Characters).Where(c => c.IsCrewMember).Include(c => c.GameAccount).ToListAsync();
    }

    public async Task<IList<CharacterEntity>> GetUserCharacters(AppAccountEntity account)
    {
        return await DbContext.AppAccounts
            .Where(a => a == account)
            .SelectMany(a => a.GameAccounts)
            .SelectMany(c => c.Characters).Include(c => c.GameAccount).ToListAsync();
    }
    
    #endregion

    public async Task<(bool, string)> AddCharacterToCrew(CharacterEntity character, int? lotToLend)
    {
        if (character.IsCrewMember)
        {
            return (true, $"{character.Name} is already a crew member");
        }
        
        character.IsCrewMember = true;
        if (lotToLend.HasValue)
        {
            character.MaxLotsForCrew = lotToLend.Value;
        }
        DbContext.Characters.Update(character);
        return await DbContext.SaveChangesAsync() > 0 ? (true, $"{character.Name} added to crew.") : (false, $"Failed to add {character.Name} to crew.");
    }
    
    public async Task<(bool, string)> RemoveCharacterFromCrew(CharacterEntity character)
    {
        if (!character.IsCrewMember)
        {
            return (true, $"{character.Name} is not a crew member");
        }
        
        character.IsCrewMember = false;
        DbContext.Characters.Update(character);
        return await DbContext.SaveChangesAsync() > 0 ? (true, $"{character.Name} removed from crew.") : (false, $"Failed to remove {character.Name} from crew.");
    }

    public async Task<(bool, string)> SetCharacterMaxLotToLend(CharacterEntity character, int lotToLend)
    {
        character.MaxLotsForCrew = lotToLend;
        DbContext.Characters.Update(character);
        return await DbContext.SaveChangesAsync() > 0 ? 
            (true, $"{character.Name} max lot to lend set to {lotToLend}.") : 
            (false, $"Failed to set {character.Name} max lot to lend.");
    }

    public Task<bool> CanRemoveCharacterFromCrew(CharacterEntity character)
    {
        return Task.FromResult(true);
    }

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
        appAccount ??= await GetAppAccountAsync();
        
        return await DbContext.Buildings
            .Include(b => b.Owner)
            .Include(b => b.PutDownBy)
            .Include(b => b.Cluster)
            .Where(b => b.Owner.OwnerAppAccount == appAccount && b.Type==BuildingType.Harvester && b.Cluster == null && b.PutDownBy == null && b.HarvestingResourceType == resourceType)
            .ToListAsync();
    }
}