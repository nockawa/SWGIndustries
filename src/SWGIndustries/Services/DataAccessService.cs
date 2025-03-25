using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using SWGIndustries.Data;

namespace SWGIndustries.Services;

[PublicAPI]
public sealed class DataAccessService : IDisposable, IAsyncDisposable
{
    private readonly Task<ApplicationUser> _applicationUserTask;

    /// <summary>
    /// Should be used for read-only operations, or very carefully...
    /// </summary>
    public ApplicationDbContext DbContext { get; }

    public DataAccessService(ApplicationDbContext dbContext, UserService userService)
    {
        DbContext = dbContext;
        _applicationUserTask = userService.BuildApplicationUser(DbContext);
    }

    public void Dispose()
    {
        DbContext.Dispose();
    }

    public async ValueTask DisposeAsync() => await DbContext.DisposeAsync();

    #region ApplicationUser

    public async Task<ApplicationUser> GetApplicationUserAsync() => await _applicationUserTask;
    public ApplicationUser GetApplicationUser() => _applicationUserTask.Result;
    
    public async Task<ApplicationUser> GetApplicationUserByName(string name, bool caseInsensitive)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }
        
        return await DbContext
            .ApplicationUsers.Include(a => a.Crew)
            .FirstOrDefaultAsync(u => caseInsensitive ? 
                u.Name.ToLower() == name.ToLower() : 
                u.Name == name);
    }

    public async Task<IEnumerable<ApplicationUser>> GetApplicationUsersByName(string nameFragment, bool caseInsensitive, CancellationToken cancellationToken)
    {
        return await DbContext.ApplicationUsers.Where(u => EF.Functions.Like(u.Name, $"%{nameFragment}%"))
            .ToListAsync(cancellationToken);
    }

    #endregion


    #region SWGAccount operations

    public async Task<IList<SWGAccount>> GetUserSWGAccounts()
    {
        var user = await GetApplicationUserAsync();
        return user.SWGAccounts;
        
    }

    public async Task<bool> AddSWGAccount(SWGAccount swgAccount, bool fullRefresh)
    {
        var user = await GetApplicationUserAsync();
        if (user.SWGAccounts.Contains(swgAccount))
        {
            return false;
        }

        user.SWGAccounts.Add(swgAccount);
        var res = await DbContext.SaveChangesAsync() > 0;
        
        if (fullRefresh)
        {
            await DbContext.Entry(user).Collection(u => u.SWGAccounts).LoadAsync();
        }

        return res;
    }
    
    public async Task<bool> RemoveSWGAccount(SWGAccount swgAccount, bool fullRefresh)
    {
        var user = await GetApplicationUserAsync();
        if (!user.SWGAccounts.Contains(swgAccount))
        {
            return false;
        }

        user.SWGAccounts.Remove(swgAccount);
        DbContext.SWGAccounts.Remove(swgAccount);
        
        var res = await DbContext.SaveChangesAsync() > 0;
        if (fullRefresh)
        {
            await DbContext.Entry(user).Collection(u => u.SWGAccounts).LoadAsync();
        }
        return res;
    }
    
    public async Task RefreshSWGAccounts() 
    {
        var user = await GetApplicationUserAsync();
        await DbContext.Entry(user).Collection(u => u.SWGAccounts).LoadAsync();
    }

    #endregion

    #region SWGCharacter operations

    public async Task<bool> AddSWGCharacter(SWGAccount swgAccount, SWGCharacter swgCharacter, bool fullRefresh)
    {
        swgAccount.SWGCharacters.Add(swgCharacter);
        var res = await DbContext.SaveChangesAsync() > 0;
        if (fullRefresh)
        {
            await DbContext.Entry(swgAccount).Collection(u => u.SWGCharacters).LoadAsync();
        }
        return res;
    }
    
    public async Task<bool> RemoveSWGCharacter(SWGCharacter swgCharacter, bool fullRefresh)
    {
        var swgAccount = swgCharacter.SWGAccount;
        swgAccount?.SWGCharacters.Remove(swgCharacter);
        DbContext.SWGCharacters.Remove(swgCharacter);
        
        var res = await DbContext.SaveChangesAsync() > 0;
        if (swgAccount!=null && fullRefresh)
        {
            await DbContext.Entry(swgAccount).Collection(u => u.SWGCharacters).LoadAsync();
        }
        return res;
    }
    
    public async Task RefreshSWGCharacters(SWGAccount swgAccount)
    {
        await DbContext.Entry(swgAccount).Collection(u => u.SWGCharacters).LoadAsync();
    }

    #endregion

    #region Crew

    /// <summary>
    /// Create a new crew with the given leader
    /// </summary>
    /// <param name="leader"></param>
    /// <returns></returns>
    public async Task<(Crew, string)> CreateCrew(ApplicationUser leader, string crewName)
    {
        // Check if the leader is already a member of another crew, which can't be
        if (leader.Crew != null)
        {
            return (null, $"Can't create a crew, {leader.Name} is already a member of another one");
        }
        
        // Check if the crew name too long
        if (crewName.Length > Crew.CrewNameMaxLength)
        {
            return (null, $"Crew name too long, max length is {Crew.CrewNameMaxLength}");
        }
        
        // Create the crew, set its leader and add it to the database
        var crew = new Crew
        {
            CrewLeader = leader,
            Name = crewName
        };
        DbContext.Crews.Add(crew);
        
        // Set the leader as a member of the crew
        leader.Crew = crew;
        DbContext.ApplicationUsers.Update(leader);

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
    public async Task<(bool, string)> AddCrewMember(ApplicationUser member, Crew crew)
    {
        // Check if the member is already a member of another crew
        if (member.Crew != null)
        {
            return (false, $"{member.Name} is already a member of another crew");
        }
        
        // Add the crew to the member, update and save
        member.Crew = crew;
        DbContext.ApplicationUsers.Update(member);
        var res = await DbContext.SaveChangesAsync() > 0;
        if (res == false)
        {
            return (false, $"Failed to add {member.Name} to crew {crew.Name}");
        }
        
        // Refresh the crew members list
        await DbContext.Entry(crew).Collection(c => c.Members).LoadAsync();
        
        return (true, $"{member.Name} added to crew {crew.Name}");
    }

    public async Task<(bool, string)> LeaveCrew(ApplicationUser member)
    {
        if (member.Crew == null)
        {
            return (true, $"{member.Name} is not a member of any crew");
        }
        
        // Remove the member from the crew
        var crew = member.Crew;
        member.Crew = null;
        DbContext.ApplicationUsers.Update(member);
        var res = await DbContext.SaveChangesAsync() > 0;
        if (res == false)
        {
            return (false, $"Failed to remove {member.Name} from crew");
        }
        
        // Refresh the crew members list
        await DbContext.Entry(crew).Collection(c => c.Members).LoadAsync();
        return (true, $"{member.Name} removed from crew {crew.Name}");
    }
    
    public async Task<Crew> GetCrewByName(string crewName, bool caseInsensitive)
    {
        if (string.IsNullOrEmpty(crewName))
        {
            return null;
        }
        return await DbContext.Crews.FirstOrDefaultAsync(c => caseInsensitive ? 
            c.Name.ToLower() == crewName.ToLower() : 
            c.Name == crewName);
    }

    public async Task<Crew> GetCrewByLeaderAccountName(string leaderAccountName, bool caseInsensitive)
    {
        return (await GetApplicationUserByName(leaderAccountName, caseInsensitive))?.Crew;
    }
    
    #endregion

    #region Crew Invitations
    
    public async Task<(CrewInvitation, string)> CreateRequestToJoinCrew(ApplicationUser leader, ApplicationUser requester)
    {
        var crewInvitation = new CrewInvitation
        {
            FromUser = requester,
            ToUser = leader,
            InviteOrRequestToJoin = false,
            Status = InvitationStatus.Pending
        };
        
        if (await DbContext.CrewInvitations.AnyAsync(
                          ci => (ci.InviteOrRequestToJoin == false && ci.FromUser == crewInvitation.FromUser) ||
                                (ci.InviteOrRequestToJoin == true && ci.ToUser == crewInvitation.FromUser)))
        {
            return (null, $"There is already an invitation pending for {crewInvitation.FromUser.Name}");
        }

        DbContext.CrewInvitations.Add(crewInvitation);
        var res = await DbContext.SaveChangesAsync() > 0;
        return res ? 
            (crewInvitation, $"Crew invitation sent to {leader.Name}") : 
            (null, $"Failed to send crew invitation to {leader.Name}");
    }
    
    public async Task<(CrewInvitation, string)> CreateInvitationToJoinCrew(ApplicationUser leader, ApplicationUser invitedUser)
    {
        await DbContext.Entry(invitedUser).Reference(i => i.Crew).LoadAsync();
        if (invitedUser.Crew != null)
        {
            return (null, $"{invitedUser.Name} is already a member of another crew");
        }
        
        var crewInvitation = new CrewInvitation
        {
            FromUser = leader,
            ToUser = invitedUser,
            InviteOrRequestToJoin = true,
            Status = InvitationStatus.Pending
        };
        
        if (await DbContext.CrewInvitations.AnyAsync(
                ci => (ci.InviteOrRequestToJoin == false && ci.FromUser == crewInvitation.ToUser) ||
                      (ci.InviteOrRequestToJoin == true && ci.ToUser == crewInvitation.ToUser)))
        {
            return (null, $"There is already an invitation pending for {invitedUser.Name}");
        }

        DbContext.CrewInvitations.Add(crewInvitation);
        var res = await DbContext.SaveChangesAsync() > 0;
        return res ? 
            (crewInvitation, $"Crew invitation sent to {invitedUser.Name}") : 
            (null, $"Failed to send crew invitation to {invitedUser.Name}");
    }
    
    public async Task<List<CrewInvitation>> GetPendingCrewRequests(ApplicationUser applicationUser)
    {
        return await DbContext.CrewInvitations
            .Where(
                // Invitation must be in pending state
                ci => ci.Status==InvitationStatus.Pending && 
                      
                // And either at the request of the applicationUser or a invitation of the crew leader       
                ((ci.InviteOrRequestToJoin==false && ci.FromUser == applicationUser) || (ci.InviteOrRequestToJoin && ci.ToUser == applicationUser)))
                .ToListAsync();
    }

    public async Task<bool> DeleteCrewInvitation(CrewInvitation crewInvitation)
    {
        DbContext.CrewInvitations.Remove(crewInvitation);
        return await DbContext.SaveChangesAsync() > 0;
    }

    public async Task<IList<CrewInvitation>> GetCrewRequestForLeader(ApplicationUser crewLeader)
    {
        return await DbContext.CrewInvitations
            .Where(ci => ci.Status == InvitationStatus.Pending && ci.ToUser == crewLeader && ci.InviteOrRequestToJoin==false)
            .ToListAsync();
    }

    public async Task<(bool, string)> ProcessRequestToJoinCrew(CrewInvitation invitation, bool grantOrReject)
    {
        var crewLeader = invitation.ToUser;
        var crewMember = invitation.FromUser;

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
            DbContext.ApplicationUsers.Update(crewMember);
            res = await DbContext.SaveChangesAsync() > 0;
        }

        var actionText = grantOrReject ? "accepted" : "rejected";
        return (res, res ? $"Invitation from {invitation.FromUser.Name} is {actionText}." : $"Failed to {actionText} the invitation");
    }

    public async Task<(bool, string)> ProcessInvitationToJoinCrew(CrewInvitation invitation, bool grantOrReject)
    {
        var crewLeader = invitation.FromUser;
        var crewMember = invitation.ToUser;

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
            DbContext.ApplicationUsers.Update(crewMember);
            res = await DbContext.SaveChangesAsync() > 0;
        }

        var actionText = grantOrReject ? "accepted" : "rejected";
        return (res, res ? $"Invitation from {invitation.FromUser.Name} is {actionText}." : $"Failed to {actionText} the invitation");
    }

    public async Task<CrewInvitation> GetAnsweredCrewRequest()
    {
        var applicationUser = await GetApplicationUserAsync();
        return await DbContext.CrewInvitations
            .FirstOrDefaultAsync(ci => ci.Status!=InvitationStatus.Pending && ci.InviteOrRequestToJoin==false && ci.FromUser==applicationUser);
    }
    
    public async Task<IList<CrewInvitation>> GetAnsweredCrewInvitations()
    {
        var applicationUser = await GetApplicationUserAsync();
        return await DbContext.CrewInvitations
            .Where(ci => ci.Status!=InvitationStatus.Pending && ci.InviteOrRequestToJoin && ci.FromUser==applicationUser).ToListAsync();
    }
    
    public async Task<bool> CloseCrewInvitation(CrewInvitation crewInvitation)
    {
        DbContext.CrewInvitations.Remove(crewInvitation);
        return await DbContext.SaveChangesAsync() > 0;
    }

    public async Task<IList<SWGCharacter>> GetCrewCharacters(Crew crew)
    {
        return await DbContext.ApplicationUsers
            .Where(a => a.Crew == crew)
            .SelectMany(a => a.SWGAccounts)
            .SelectMany(c => c.SWGCharacters).Where(c => c.IsCrewMember).Include(c => c.SWGAccount).ToListAsync();
    }

    public async Task<IList<SWGCharacter>> GetUserCharacters(ApplicationUser user)
    {
        return await DbContext.ApplicationUsers
            .Where(a => a == user)
            .SelectMany(a => a.SWGAccounts)
            .SelectMany(c => c.SWGCharacters).Include(c => c.SWGAccount).ToListAsync();
    }
    
    #endregion

    public async Task<(bool, string)> AddCharacterToCrew(SWGCharacter character, int? lotToLend)
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
        DbContext.SWGCharacters.Update(character);
        return await DbContext.SaveChangesAsync() > 0 ? (true, $"{character.Name} added to crew.") : (false, $"Failed to add {character.Name} to crew.");
    }
    
    public async Task<(bool, string)> RemoveCharacterFromCrew(SWGCharacter character)
    {
        if (!character.IsCrewMember)
        {
            return (true, $"{character.Name} is not a crew member");
        }
        
        character.IsCrewMember = false;
        DbContext.SWGCharacters.Update(character);
        return await DbContext.SaveChangesAsync() > 0 ? (true, $"{character.Name} removed from crew.") : (false, $"Failed to remove {character.Name} from crew.");
    }

    public async Task<(bool, string)> SetCharacterMaxLotToLend(SWGCharacter character, int lotToLend)
    {
        character.MaxLotsForCrew = lotToLend;
        DbContext.SWGCharacters.Update(character);
        return await DbContext.SaveChangesAsync() > 0 ? 
            (true, $"{character.Name} max lot to lend set to {lotToLend}.") : 
            (false, $"Failed to set {character.Name} max lot to lend.");
    }

    public Task<bool> CanRemoveCharacterFromCrew(SWGCharacter character)
    {
        return Task.FromResult(true);
    }
}