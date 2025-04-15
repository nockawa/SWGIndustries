using System.Diagnostics;
using System.Xml.Serialization;

namespace SWGIndustries.Services;

public class GameServersManager : IDisposable
{
    private readonly ILogger<GameServersManager> _logger;
    private readonly IWebHostEnvironment _env;
    private Dictionary<string, GameServerDefinition> _servers;
    private string ServerDefinitionsDirectory => Path.Combine(_env.WebRootPath, "Resources/ServerDefinitions");

    private readonly Lock _locker = new();
    private readonly FileSystemWatcher _watcher;

    public const string DefaultServerName = "SWG Restoration III";
    
    public GameServersManager(ILogger<GameServersManager> logger, IWebHostEnvironment env)
    {
        _logger = logger;
        _env = env;
        
        BuildServerList();
        
        _watcher = new FileSystemWatcher(ServerDefinitionsDirectory)
        {
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
            Filter = "*.xml"
        };
        _watcher.Changed += OnChanged;
        _watcher.Created += OnChanged;
        _watcher.Deleted += OnChanged;
        _watcher.Renamed += OnChanged;
        _watcher.EnableRaisingEvents = true;
    }

    public void Dispose()
    {
        _watcher?.Dispose();
    }
    
    public IReadOnlyDictionary<string, GameServerDefinition> Servers => GetServers();

    private Dictionary<string, GameServerDefinition> GetServers()
    {
        // ReSharper disable once InconsistentlySynchronizedField
        var servers = _servers;
        if (servers == null)
        {
            lock (_locker)
            {
                if (_servers == null)
                {
                    BuildServerList();
                }
                servers = _servers;
                Debug.Assert(servers != null);
            }
        }
        return servers;
    }

    public GameServerDefinition GetServer(string serverName)
    {
        if (string.IsNullOrWhiteSpace(serverName))
        {
            return null;
        }
        var servers = GetServers();
        servers.TryGetValue(serverName, out var server);
        return server;
    }
    
    public IReadOnlyCollection<GameServerDefinition> GetServerList()
    {
        return GetServers().Values;
    }
    
    private void OnChanged(object sender, FileSystemEventArgs e)
    {
        _servers = null;
    }

    private void BuildServerList()
    {
        var serverList = new Dictionary<string, GameServerDefinition>();
        var files = Directory.GetFiles(ServerDefinitionsDirectory, "*.xml");
        foreach (var file in files)
        {
            try
            {
                var serverDefinition = XMLHelper.LoadXML<GameServerDefinition>(file);
                serverDefinition.FileName = Path.GetFileName(file);
                serverList.Add(serverDefinition.Name, serverDefinition);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error loading file {FileName}", file);
            }
        }
        _servers = serverList;
    }
}
