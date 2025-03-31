using JetBrains.Annotations;
using SWGIndustries.Data;

namespace SWGIndustries.Services;

public class DataScopeService
{
    private readonly DataAccessService _dataAccessService;

    public DataScopeService(DataAccessService dataAccessService)
    {
        _dataAccessService = dataAccessService;
    }    
    
    public DataScope GetCrewScope()
    {
        var crew = _dataAccessService.GetAppAccount().Crew;
        return crew == null ? null : DataScope.FromCrew(crew);
    }
    
    public DataScope GetAppAccountScope()
    {
        var appAccount = _dataAccessService.GetAppAccount();
        return appAccount == null ? null : DataScope.FromAppAccount(appAccount);
    }
    
    public DataScope GetGameAccountScope(GameAccountEntity account)
    {
        account ??= _dataAccessService.GetAppAccount().GameAccounts.FirstOrDefault();
        return account == null ? null : DataScope.FromGameAccount(account);
    }
}

[PublicAPI]
public class DataScope
{
    public static DataScope FromCrew(CrewEntity crew) => new(crew);
    public static DataScope FromAppAccount(AppAccountEntity appAccount) => new(appAccount);
    public static DataScope FromGameAccount(GameAccountEntity account) => new(account);
    
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
    public AppAccountEntity AppAccount { get; }
    public GameAccountEntity GameAccount { get; }

    private DataScope(GameAccountEntity account)
    {
        GameAccount = account ?? throw new ArgumentNullException(nameof(account));
    }

    private DataScope(CrewEntity crew)
    {
        Crew = crew ?? throw new ArgumentNullException(nameof(crew));
    }

    private DataScope(AppAccountEntity appAccount)
    {
        AppAccount = appAccount ?? throw new ArgumentNullException(nameof(appAccount));
    }
}

