using JetBrains.Annotations;
using SWGIndustries.Data;

namespace SWGIndustries.Services;

public class DataScopeService
{
    private readonly AdminService _adminService;

    public DataScopeService(AdminService adminService)
    {
        _adminService = adminService;
    }    
    
    public DataScope GetCrewScope()
    {
        var appAccount = _adminService.GetAppAccount();
        var crew = appAccount.Crew;
        return crew == null ? null : DataScope.FromCrew(crew, crew.CrewLeader == appAccount);
    }
    
    public DataScope GetAppAccountScope()
    {
        var appAccount = _adminService.GetAppAccount();
        return appAccount == null ? null : DataScope.FromAppAccount(appAccount);
    }
    
    public DataScope GetGameAccountScope(GameAccountEntity account)
    {
        account ??= _adminService.GetAppAccount().GameAccounts.FirstOrDefault();
        return account == null ? null : DataScope.FromGameAccount(account);
    }
}

[PublicAPI]
public class DataScope
{
    internal static DataScope FromCrew(CrewEntity crew, bool isCrewLeader) => new(crew, isCrewLeader);
    internal static DataScope FromAppAccount(AppAccountEntity appAccount) => new(appAccount);
    internal static DataScope FromGameAccount(GameAccountEntity account) => new(account);
    
    public override string ToString()
    {
        if (IsCrew)
        {
            return Crew.Name;
        }

        if (IsAppAccount)
        {
            return AppAccount.Name;
        }

        return GameAccount.Name;
    }

    public bool IsCrew => Crew != null;
    public bool IsAppAccount => AppAccount != null;
    public bool IsGameAccount => GameAccount != null;

    public CrewEntity Crew { get; }
    public bool IsLeader { get; set; }
    public AppAccountEntity AppAccount { get; }
    public GameAccountEntity GameAccount { get; }

    private DataScope(GameAccountEntity account)
    {
        GameAccount = account ?? throw new ArgumentNullException(nameof(account));
    }

    private DataScope(CrewEntity crew, bool isCrewLeader)
    {
        IsLeader = isCrewLeader;
        Crew = crew ?? throw new ArgumentNullException(nameof(crew));
    }

    private DataScope(AppAccountEntity appAccount)
    {
        AppAccount = appAccount ?? throw new ArgumentNullException(nameof(appAccount));
    }
}

