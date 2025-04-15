using System.Collections.Concurrent;
using System.IO.Compression;
using System.Xml;
using System.Xml.Serialization;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using SWGIndustries.Data;

namespace SWGIndustries.Services;

[PublicAPI]
public class ResourceManagerService
{
    private readonly GameServersManager _gameServersManager;
    private readonly ILogger<ResourceManagerService> _logger;

    private readonly ConcurrentDictionary<int, GameServerResources> _servers;
    public class GameServerResources
    {
        private readonly GameServerDefinition _definition;
        private readonly int _gameServerId;
        private readonly IWebHostEnvironment _env;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<GameServerResources> _logger;
        private readonly SemaphoreSlim _locker = new(1, 1);

        private readonly Dictionary<string, ResourceCategory> _resourceCategoryByName;
        private readonly Dictionary<ushort, ResourceCategory> _resourceCategoryById;
        private readonly Dictionary<string, Resource> _resourceByName;
        public ResourceCategory RootCategory { get; private set; }

        public Resource GetResourceByName(string name)
        {
            _resourceByName.TryGetValue(name, out Resource resource);
            return resource;
        }

        public GameServerResources(GameServerDefinition definition, IWebHostEnvironment env, IServiceProvider serviceProvider, 
            ILogger<GameServerResources> logger)
        {
            _definition = definition;
            _gameServerId = definition.Id;
            _env = env;
            _serviceProvider = serviceProvider;
            _logger = logger;
            _resourceCategoryByName = new Dictionary<string, ResourceCategory>();
            _resourceCategoryById = new Dictionary<ushort, ResourceCategory>();
            _resourceByName = new Dictionary<string, Resource>();

            try
            {
                Initialize().GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occured while initializing the resource category.");
                throw;
            }
        }
        
        private async Task Initialize()
        {
            await _locker.WaitAsync();
            try
            {
                // Load the resource category tree from XML
                try
                {
                    var resourceTreeFilePathName = Path.Combine(_env.WebRootPath, "Resources", "resource_tree.xml");
                    var serializer = new XmlSerializer(typeof(XmlResourceCategory));
                    await using var fileStream = new FileStream(resourceTreeFilePathName, FileMode.Open);
                    var rootCategory = (XmlResourceCategory)serializer.Deserialize(fileStream);

                    RootCategory = BuildResourceCategories(null, rootCategory);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Failed to load categories from 'resource_tree.xml'");
                    throw;
                }

                // Load actives resources from database, reference them into their respective category
                {
                    using var scope = _serviceProvider.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    var activeResources = await context.Resources.Where(r => r.DepletedSince == null).ToListAsync();

                    foreach (var resourceModel in activeResources)
                    {
                        var resource = new Resource(resourceModel);

                        var category = _resourceCategoryById[resourceModel.CategoryIndex];
                        category.AddResource(resource);

                        _resourceByName.Add(resource.Name, resource);
                    }
                }
            }
            finally
            {
                _locker.Release();
            }
        }

        // Dictionary to map the resource category ID to their matching harvesting resource type
        private readonly Dictionary<int, HarvestingResourceType> _harvestingTypeFromCategoryId = new()
        {
            { 1, HarvestingResourceType.Unknown       },
            { 3, HarvestingResourceType.Creature      },
            { 228, HarvestingResourceType.Flora       },
            { 439, HarvestingResourceType.Chemical    },
            { 453, HarvestingResourceType.Water       },
            { 464, HarvestingResourceType.Mineral     },
            { 475, HarvestingResourceType.Radioactive },
            { 591, HarvestingResourceType.Gas         },
            { 1104, HarvestingResourceType.Geothermal },
            { 903, HarvestingResourceType.Wind        },
            { 904, HarvestingResourceType.Solar       }
        };

        private ResourceCategory BuildResourceCategories(ResourceCategory parent, XmlResourceCategory xmlCategory)
        {
            HarvestingResourceType? hrt = null;
            if (_harvestingTypeFromCategoryId.TryGetValue(xmlCategory.Index, out var tps))
            {
                hrt = tps;
            }
            var cat = new ResourceCategory(parent, xmlCategory, hrt);
            _resourceCategoryByName.Add(cat.Name, cat);
            _resourceCategoryById.Add(cat.Index, cat);

            foreach (var subCategory in xmlCategory.SubCategories)
            {
                BuildResourceCategories(cat, subCategory);
            }
            return cat;
        }    
        internal async Task RefreshResources()
        {
            _logger.LogInformation("Updating resources from SWGAide");
            
            try
            {
                // Load current resources from SWGAide for the Restoration 3 server, it's a zipped XML file
                using var client = new HttpClient();
                await using var fileStream = await client.GetStreamAsync(_definition.SWGAideResourceFileURL);
                await using var gzipStream = new GZipStream(fileStream, CompressionMode.Decompress);
                var xmlDoc = new XmlDocument();
                xmlDoc.Load(gzipStream);
                var serializer = new XmlSerializer(typeof(XmlResourceData));
                using var reader = new XmlNodeReader(xmlDoc);
                XmlResourceData resources;
                try
                {
                    resources = (XmlResourceData)serializer.Deserialize(reader)!;
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Couldn't deserialize SWGAide resources zip file");
                    throw;
                }

                // Load the resources into temp dictionary
                var dic = new Dictionary<string, Resource>(resources.Resources.Count);
                foreach (var xmlResource in resources.Resources)
                {
                    if (_resourceCategoryByName.TryGetValue(xmlResource.Type, out var category) == false)
                    {
                        _logger.LogError("Couldn't find category {XmlResourceType} while loading resource {XmlResourceName} from SWGAide.", xmlResource.Type, xmlResource.Name);
                        continue;
                    }
                    var resource = new Resource(xmlResource, category.Index);
                    if (resource.IsNotQualified)
                    {
                        continue;
                    }
                    dic.Add(resource.Name, resource);
                }

                // From this point we enter a blocking section
                await _locker.WaitAsync();
                try
                {
                    // Look for depleted resources, they are present in _resourceByName but not our temporary dictionary
                    var depletedResources = new List<Resource>();
                    foreach (var kvp in _resourceByName)
                    {
                        // Present in both dictionaries, the resource is still active, skip to the next one
                        if (dic.ContainsKey(kvp.Key))
                        {
                            continue;
                        }

                        // Resource is depleted, set the depleted date/time, add to our list
                        var resource = kvp.Value;
                        resource.DepletedSince = DateTime.UtcNow;
                        depletedResources.Add(resource);
                    }

                    // Look for new resources or updated, they are present in our temporary dictionary but not _resourceByName
                    var newResources = new List<Resource>();
                    var updatedResources = new List<(Resource, Resource)>();
                    foreach (var kvp in dic)
                    {
                        if (_resourceByName.TryGetValue(kvp.Key, out var existingResource))
                        {
                            // Resource is present in both dictionaries, check if it has been updated
                            var newResource = kvp.Value;
                            if (existingResource.IsOutDated(newResource))
                            {
                                updatedResources.Add((newResource, existingResource));
                                continue;
                            }
                            else
                            {
                                continue;
                            }
                        }

                        newResources.Add(kvp.Value);
                    }

                    // Time to conciliate with database

                    // First we need to remove the depleted resources
                    using var scope = _serviceProvider.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    foreach (var resource in depletedResources)
                    {
                        var resourceModel = context.Resources.First(r => r.GameServerId==_gameServerId && r.Name == resource.Name);
                        resourceModel.DepletedSince = resource.DepletedSince;
                        context.Resources.Update(resourceModel);
                        _logger.LogDebug("Resource {ResourceModel} is being depleted.", resourceModel);

                        // Remove the resource from the service (as our service only tracks active resources)
                        _resourceByName.Remove(resource.Name);
                        resource.Category.Remove(resource);
                    }

                    await context.SaveChangesAsync();
                    _logger.LogInformation("Removed {DepletedResourcesCount} depleted resources.", depletedResources.Count);

                    // Now we can add the new resources
                    foreach (var resource in newResources)
                    {
                        // Add the resource to the service
                        _resourceByName.Add(resource.Name, resource);
                        _logger.LogDebug("New resource {ResourceName} is being added.", resource.Name);

                        // Add the resource to its category
                        var category = _resourceCategoryById[resource.CategoryIndex];
                        category.AddResource(resource);

                        // Add to the database
                        var model = resource.ToModel(_gameServerId);
                        context.Resources.Add(model);
                    }

                    await context.SaveChangesAsync();
                    _logger.LogInformation("Added {NewResourcesCount} new resources.", newResources.Count);

                    // Now we can update the outdated resources
                    foreach (var (newResource, curResource) in updatedResources)
                    {
                        curResource.UpdateFrom(newResource);
                        _logger.LogDebug("Resource {ResourceName} is being updated with new stats/planets.", curResource.Name);

                        context.Resources.Update(curResource.ToModel(_gameServerId));
                    }

                    await context.SaveChangesAsync();
                    _logger.LogInformation("Updated {UpdatedResourcesCount} existing resources.", updatedResources.Count);
                }
                finally
                {
                    _locker.Release();
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to load resources from SWGAide");
                return;
            }    
            
            _logger.LogInformation("Resources updated from SWGAide");
        }
    
    }

    public ResourceManagerService(GameServersManager gameServersManager, IWebHostEnvironment env, IServiceProvider serviceProvider, ILogger<ResourceManagerService> logger)
    {
        _gameServersManager = gameServersManager;
        _logger = logger;
        
        _servers = new ConcurrentDictionary<int, GameServerResources>();
        
        _logger.LogInformation("Timed Hosted Service running.");

        foreach (var def in _gameServersManager.GetServerList())
        {
            var gsr = new GameServerResources(def, env, serviceProvider, serviceProvider.GetRequiredService<ILogger<GameServerResources>>());
            _servers.TryAdd(def.Id, gsr);
        }
    }

    public Resource GetResourceByName(int gameServerId, string name)
    {
        if (_servers.TryGetValue(gameServerId, out var gsr) == false)
        {
            return null;
        }
        return gsr.GetResourceByName(name);
    }
    
    public async Task RefreshResources()
    {
        await Task.WhenAll(_servers.Values.Select(server => server.RefreshResources()));
    }
    
    public ResourceCategory GetRootCategory(int gameServerId)
    {
        if (_servers.TryGetValue(gameServerId, out var gsr) == false)
        {
            return null;
        }
        return gsr.RootCategory;
    }
}