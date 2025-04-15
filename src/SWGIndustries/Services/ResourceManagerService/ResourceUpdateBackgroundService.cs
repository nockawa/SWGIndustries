namespace SWGIndustries.Services;

public class ResourceUpdateBackgroundService : BackgroundService
{
    private readonly ResourceManagerService _resourceManagerService;
    private readonly ILogger<ResourceUpdateBackgroundService> _logger;

    public ResourceUpdateBackgroundService(ResourceManagerService resourceManagerService, ILogger<ResourceUpdateBackgroundService> logger)
    {
        _resourceManagerService = resourceManagerService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await _resourceManagerService.RefreshResources();
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}