using System.Diagnostics;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using JetBrains.Annotations;

namespace SWGIndustries.Services;

/// <summary>
/// Service exposing a tree oriented structure of the definition types in the game we support.
/// </summary>
/// This service is a singleton, the data is readonly and immutable.
/// 
/// <see cref="StructureRoot"/> is the entry point to structure based definitions:
///  - Houses: different types of houses, their specifications
///  - Factories: different types of factories, their specifications
///  - Harvesters: different types of harvesters, their specifications
/// This referential is built from the XML file located in the Resources folder.
[PublicAPI]
public class DefinitionService
{
    private readonly IWebHostEnvironment _env;
    private readonly GameServersManager _gameServersManager;
    private readonly UserService _userService;
    private readonly ILogger<DefinitionService> _logger;
    private readonly Dictionary<string, BaseNode> _nodesByFullClassName = new();
    private GameServerDefinition _gameServerDefinition;
    private int? _gameServerId;
    public IBaseNode StructureRoot { get; }

    public int GameServerId
    {
        get
        {
            if (_gameServerId.HasValue == false)
            {
                _ = GameServerDefinition;
                Debug.Assert(_gameServerId != null, nameof(_gameServerId) + " != null");
            }

            return _gameServerId.Value;
        }
    }

    public GameServerDefinition GameServerDefinition
    {
        get
        {
            if (_gameServerDefinition == null)
            {
                var user = _userService.GetUserInfo().Result;
                var serverDefinition = _gameServersManager.GetServer(user.SWGServerName);
                if (serverDefinition == null)
                {
                    _logger.LogWarning($"No SWG Server configured for user {user.Name}, taking SWG Restoration as default.");
                    serverDefinition = _gameServersManager.GetServer(GameServersManager.DefaultServerName);
                }

                _gameServerId = serverDefinition.Id;
                _gameServerDefinition = serverDefinition;
            }

            return _gameServerDefinition;
        }
    }

    public DefinitionService(IWebHostEnvironment env, GameServersManager gameServersManager, UserService userService, ILogger<DefinitionService> logger)
    {
        _env = env;
        _gameServersManager = gameServersManager;
        _userService = userService;
        _logger = logger;

        // Uncomment to generate the XSD files
        {
            // ExportXSD(typeof(BaseNode), "StructuresDefinition.xsd");
            // ExportXSD(typeof(GameServerDefinition), "ServerDefinitions/ServerDefinition.xsd");
        }
        
        var structureRoot = XMLHelper.LoadXML<BaseNode>(Path.Combine(_env.WebRootPath, "Resources", "StructureReferential.xml"));
        PostImport(structureRoot);
        StructureRoot = structureRoot;
    }

    internal void ResetServerDefinition()
    {
        _gameServerDefinition = null;
        _gameServerId = null;
    }

    private void ExportXSD(Type type, string xsdName)
    {
        var importer = new XmlReflectionImporter();
        var schemaSet = new XmlSchemas();
        var exporter = new XmlSchemaExporter(schemaSet);
        var mapping = importer.ImportTypeMapping(type);

        // Generate the schema
        exporter.ExportTypeMapping(mapping);

        // Write the schema to a file
        foreach (XmlSchema schema in schemaSet)
        {
            using var writer = XmlWriter.Create(Path.Combine(_env.WebRootPath, "Resources", xsdName), new XmlWriterSettings { Indent = true });
            schema.Write(writer);
        }
    }

    public IBaseNode GetNodeByClass(string className)
    {
        if (_nodesByFullClassName.TryGetValue(className, out var node))
            return node;
        
        throw new KeyNotFoundException($"Class {className} not found in the structure referential.");
    }

    public T GetNodeByClass<T>(string className) where T : IBaseNode => (T)GetNodeByClass(className);

    private void PostImport(BaseNode node)
    {
        var fc = node.FullClass;
        if (fc != null)
        {
            _nodesByFullClassName.Add(fc, node);
        }
        foreach (var child in node.Children)
        {
            ((BaseNode)child).Parent = node;
            PostImport((BaseNode)child);
        }
    }
}