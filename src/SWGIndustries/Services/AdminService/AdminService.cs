using Microsoft.EntityFrameworkCore;
using SWGIndustries.Data;

namespace SWGIndustries.Services;

public class AdminService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly UserService _userService;
    private readonly ILogger<AdminService> _logger;

    private AppAccountEntity _appAccount;

    public AdminService(ApplicationDbContext dbContext, UserService userService, ILogger<AdminService> logger)
    {
        _dbContext = dbContext;
        _userService = userService;
        _logger = logger;
    }

    #region AppAccount operations

    public async Task<AppAccountEntity> GetAppAccountAsync()
    {
        if (_appAccount == null)
        {
            _appAccount = await _userService.BuildAppAccount(_dbContext);
        }
        return _appAccount;
    }

    public AppAccountEntity GetAppAccount()
    {
        if (_appAccount == null)
        {
            _appAccount = GetAppAccountAsync().GetAwaiter().GetResult();
        }
        return _appAccount;
    }

    public async Task<AppAccountEntity> GetAppAccountByName(string name, bool caseInsensitive)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }
        
        return await _dbContext
            .AppAccounts.Include(a => a.Crew)
            .FirstOrDefaultAsync(u => caseInsensitive ? 
                u.Name.ToLower() == name.ToLower() : 
                u.Name == name);
    }

    public async Task<IEnumerable<AppAccountEntity>> GetAppAccountsByName(string nameFragment, bool caseInsensitive, CancellationToken cancellationToken)
    {
        return await _dbContext.AppAccounts.Where(u => EF.Functions.Like(u.Name, $"%{nameFragment}%"))
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
        var res = await _dbContext.SaveChangesAsync() > 0;
        
        if (fullRefresh)
        {
            await _dbContext.Entry(user).Collection(u => u.GameAccounts).LoadAsync();
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
        _dbContext.GameAccounts.Remove(gameAccount);
        
        var res = await _dbContext.SaveChangesAsync() > 0;
        if (fullRefresh)
        {
            await _dbContext.Entry(user).Collection(u => u.GameAccounts).LoadAsync();
        }
        return res;
    }
    
    public async Task RefreshGameAccounts() 
    {
        var user = await GetAppAccountAsync();
        await _dbContext.Entry(user).Collection(u => u.GameAccounts).LoadAsync();
    }

    public async Task<GameAccountEntity> GetGameAccountByName(string name, bool caseInsensitive=false)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }
        
        return await _dbContext
            .GameAccounts.Include(a => a.Characters)
            .FirstOrDefaultAsync(u => caseInsensitive ? 
                u.Name.ToLower() == name.ToLower() : 
                u.Name == name);
    }
    
    #endregion

    #region Crew operations

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
        _dbContext.Crews.Add(crew);
        
        // Set the leader as a member of the crew
        leader.Crew = crew;
        _dbContext.AppAccounts.Update(leader);

        try
        {
            // Save the changes to the database
            await _dbContext.SaveChangesAsync();
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
        _dbContext.AppAccounts.Update(member);
        var res = await _dbContext.SaveChangesAsync() > 0;
        if (res == false)
        {
            return (false, $"Failed to add {member.Name} to crew {crew.Name}");
        }
        
        // Refresh the crew members list
        await _dbContext.Entry(crew).Collection(c => c.Members).LoadAsync();
        
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
        _dbContext.AppAccounts.Update(member);
        var res = await _dbContext.SaveChangesAsync() > 0;
        if (res == false)
        {
            return (false, $"Failed to remove {member.Name} from crew");
        }
        
        // Refresh the crew members list
        await _dbContext.Entry(crew).Collection(c => c.Members).LoadAsync();
        return (true, $"{member.Name} removed from crew {crew.Name}");
    }
    
    public async Task<CrewEntity> GetCrewByName(string crewName, bool caseInsensitive)
    {
        if (string.IsNullOrEmpty(crewName))
        {
            return null;
        }
        return await _dbContext.Crews.FirstOrDefaultAsync(c => caseInsensitive ? 
            c.Name.ToLower() == crewName.ToLower() : 
            c.Name == crewName);
    }

    public async Task<CrewEntity> GetCrewByLeaderAccountName(string leaderAccountName, bool caseInsensitive)
    {
        return (await GetAppAccountByName(leaderAccountName, caseInsensitive))?.Crew;
    }

    #endregion
    
    #region Character operations

    public async Task<bool> AddCharacter(GameAccountEntity gameAccount, CharacterEntity character, bool fullRefresh)
    {
        gameAccount.Characters.Add(character);
        var res = await _dbContext.SaveChangesAsync() > 0;
        if (fullRefresh)
        {
            await _dbContext.Entry(gameAccount).Collection(u => u.Characters).LoadAsync();
        }
        return res;
    }
    
    public async Task<bool> RemoveCharacter(CharacterEntity character, bool fullRefresh)
    {
        var gameAccount = character.GameAccount;
        gameAccount?.Characters.Remove(character);
        _dbContext.Characters.Remove(character);
        
        var res = await _dbContext.SaveChangesAsync() > 0;
        if (gameAccount!=null && fullRefresh)
        {
            await _dbContext.Entry(gameAccount).Collection(u => u.Characters).LoadAsync();
        }
        return res;
    }
    
    public async Task RefreshCharacters(GameAccountEntity gameAccount)
    {
        await _dbContext.Entry(gameAccount).Collection(u => u.Characters).LoadAsync();
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
        
        if (await _dbContext.CrewInvitations.AnyAsync(
                          ci => (ci.InviteOrRequestToJoin == false && ci.FromAccount == crewInvitation.FromAccount) ||
                                (ci.InviteOrRequestToJoin == true && ci.ToAccount == crewInvitation.FromAccount)))
        {
            return (null, $"There is already an invitation pending for {crewInvitation.FromAccount.Name}");
        }

        _dbContext.CrewInvitations.Add(crewInvitation);
        var res = await _dbContext.SaveChangesAsync() > 0;
        return res ? 
            (crewInvitation, $"Crew invitation sent to {leader.Name}") : 
            (null, $"Failed to send crew invitation to {leader.Name}");
    }
    
    public async Task<(CrewInvitationEntity, string)> CreateInvitationToJoinCrew(AppAccountEntity leader, AppAccountEntity invitedAccount)
    {
        await _dbContext.Entry(invitedAccount).Reference(i => i.Crew).LoadAsync();
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
        
        if (await _dbContext.CrewInvitations.AnyAsync(
                ci => (ci.InviteOrRequestToJoin == false && ci.FromAccount == crewInvitation.ToAccount) ||
                      (ci.InviteOrRequestToJoin == true && ci.ToAccount == crewInvitation.ToAccount)))
        {
            return (null, $"There is already an invitation pending for {invitedAccount.Name}");
        }

        _dbContext.CrewInvitations.Add(crewInvitation);
        var res = await _dbContext.SaveChangesAsync() > 0;
        return res ? 
            (crewInvitation, $"Crew invitation sent to {invitedAccount.Name}") : 
            (null, $"Failed to send crew invitation to {invitedAccount.Name}");
    }
    
    public async Task<List<CrewInvitationEntity>> GetPendingCrewRequests(AppAccountEntity appAccount)
    {
        return await _dbContext.CrewInvitations
            .Where(
                // Invitation must be in pending state
                ci => ci.Status==InvitationStatus.Pending && 
                      
                // And either at the request of the appAccount or an invitation of the crew leader       
                ((ci.InviteOrRequestToJoin==false && ci.FromAccount == appAccount) || (ci.InviteOrRequestToJoin && ci.ToAccount == appAccount)))
                .ToListAsync();
    }

    public async Task<bool> DeleteCrewInvitation(CrewInvitationEntity crewInvitation)
    {
        _dbContext.CrewInvitations.Remove(crewInvitation);
        return await _dbContext.SaveChangesAsync() > 0;
    }

    public async Task<IList<CrewInvitationEntity>> GetCrewRequestForLeader(AppAccountEntity crewLeader)
    {
        return await _dbContext.CrewInvitations
            .Where(ci => ci.Status == InvitationStatus.Pending && ci.ToAccount == crewLeader && ci.InviteOrRequestToJoin==false)
            .ToListAsync();
    }

    public async Task<(bool, string)> ProcessRequestToJoinCrew(CrewInvitationEntity invitation, bool grantOrReject)
    {
        var crewLeader = invitation.ToAccount;
        var crewMember = invitation.FromAccount;

        await _dbContext.Entry(crewLeader).Reference(c => c.Crew).LoadAsync();
        await _dbContext.Entry(crewMember).Reference(c => c.Crew).LoadAsync();
        
        if (crewMember.Crew == crewLeader.Crew)
        {
            return (true, $"{crewMember.Name} is already a member of the crew");
        }

        if (grantOrReject && crewMember.Crew != null)
        {
            return (false, $"Can't grant the request, {crewMember.Name} is already a member of another crew, he must first leave it.");
        }

        invitation.Status = grantOrReject ? InvitationStatus.Accepted : InvitationStatus.Rejected;
        _dbContext.CrewInvitations.Update(invitation);
        if (await _dbContext.SaveChangesAsync() == 1 == false)
        {
            return (false, "Failed to update the invitation status");
        }

        var res = true;
        if (grantOrReject)
        {
            crewMember.Crew = crewLeader.Crew;
            _dbContext.AppAccounts.Update(crewMember);
            res = await _dbContext.SaveChangesAsync() > 0;
        }

        var actionText = grantOrReject ? "accepted" : "rejected";
        return (res, res ? $"Invitation from {invitation.FromAccount.Name} is {actionText}." : $"Failed to {actionText} the invitation");
    }

    public async Task<(bool, string)> ProcessInvitationToJoinCrew(CrewInvitationEntity invitation, bool grantOrReject)
    {
        var crewLeader = invitation.FromAccount;
        var crewMember = invitation.ToAccount;

        await _dbContext.Entry(crewLeader).Reference(c => c.Crew).LoadAsync();
        await _dbContext.Entry(crewMember).Reference(c => c.Crew).LoadAsync();
        
        if (crewMember.Crew == crewLeader.Crew)
        {
            return (true, $"{crewMember.Name} is already a member of the crew");
        }

        if (grantOrReject && crewMember.Crew != null)
        {
            return (false, $"Can't grant the request, {crewMember.Name} is already a member of another crew, he must first leave it.");
        }

        invitation.Status = grantOrReject ? InvitationStatus.Accepted : InvitationStatus.Rejected;
        _dbContext.CrewInvitations.Update(invitation);
        if (await _dbContext.SaveChangesAsync() == 1 == false)
        {
            return (false, "Failed to update the invitation status");
        }

        bool res = true;
        if (grantOrReject)
        {
            crewMember.Crew = crewLeader.Crew;
            _dbContext.AppAccounts.Update(crewMember);
            res = await _dbContext.SaveChangesAsync() > 0;
        }

        var actionText = grantOrReject ? "accepted" : "rejected";
        return (res, res ? $"Invitation from {invitation.FromAccount.Name} is {actionText}." : $"Failed to {actionText} the invitation");
    }

    public async Task<CrewInvitationEntity> GetAnsweredCrewRequest()
    {
        var appAccount = await GetAppAccountAsync();
        return await _dbContext.CrewInvitations
            .FirstOrDefaultAsync(ci => ci.Status!=InvitationStatus.Pending && ci.InviteOrRequestToJoin==false && ci.FromAccount==appAccount);
    }
    
    public async Task<IList<CrewInvitationEntity>> GetAnsweredCrewInvitations()
    {
        var appAccount = await GetAppAccountAsync();
        return await _dbContext.CrewInvitations
            .Where(ci => ci.Status!=InvitationStatus.Pending && ci.InviteOrRequestToJoin && ci.FromAccount==appAccount).ToListAsync();
    }
    
    public async Task<bool> CloseCrewInvitation(CrewInvitationEntity crewInvitation)
    {
        _dbContext.CrewInvitations.Remove(crewInvitation);
        return await _dbContext.SaveChangesAsync() > 0;
    }

    public async Task<IList<CharacterEntity>> GetCrewCharacters(CrewEntity crew)
    {
        return await _dbContext.AppAccounts
            .Where(a => a.Crew == crew)
            .SelectMany(a => a.GameAccounts)
            .SelectMany(c => c.Characters).Where(c => c.IsCrewMember).Include(c => c.GameAccount).ToListAsync();
    }

    public async Task<IList<CharacterEntity>> GetUserCharacters(AppAccountEntity account)
    {
        return await _dbContext.AppAccounts
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
        _dbContext.Characters.Update(character);
        return await _dbContext.SaveChangesAsync() > 0 ? (true, $"{character.Name} added to crew.") : (false, $"Failed to add {character.Name} to crew.");
    }
    
    public async Task<(bool, string)> RemoveCharacterFromCrew(CharacterEntity character)
    {
        if (!character.IsCrewMember)
        {
            return (true, $"{character.Name} is not a crew member");
        }
        
        character.IsCrewMember = false;
        _dbContext.Characters.Update(character);
        return await _dbContext.SaveChangesAsync() > 0 ? (true, $"{character.Name} removed from crew.") : (false, $"Failed to remove {character.Name} from crew.");
    }

    public async Task<(bool, string)> SetCharacterMaxLotToLend(CharacterEntity character, int lotToLend)
    {
        character.MaxLotsForCrew = lotToLend;
        _dbContext.Characters.Update(character);
        return await _dbContext.SaveChangesAsync() > 0 ? 
            (true, $"{character.Name} max lot to lend set to {lotToLend}.") : 
            (false, $"Failed to set {character.Name} max lot to lend.");
    }

    public Task<bool> CanRemoveCharacterFromCrew(CharacterEntity character)
    {
        return Task.FromResult(true);
    }
    
}