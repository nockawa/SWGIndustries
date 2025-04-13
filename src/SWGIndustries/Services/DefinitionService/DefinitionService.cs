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
    private readonly Dictionary<string, BaseNode> _nodesByFullClassName = new();
    public BaseNode StructureRoot { get; }
    public ServerDefinition ServerDefinition { get; }

    public DefinitionService(IWebHostEnvironment env)
    {
        _env = env;

        // Uncomment to generate the XSD files
        /*
        {
            ExportXSD(typeof(BaseNode), "StructuresDefinition.xsd");
            ExportXSD(typeof(ServerDefinition), "ServerDefinition.xsd");
        }
        */
        
        var structureRoot = LoadXML<BaseNode>(Path.Combine(_env.WebRootPath, "Resources", "StructureReferential.xml"), null);
        PostImport(structureRoot);
        StructureRoot = structureRoot;
        
        ServerDefinition = LoadXML<ServerDefinition>(Path.Combine(_env.WebRootPath, "Resources", "SWGRestorationIII.xml"), "http://swgindustries.com/server");

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


    private static T LoadXML<T>(string fileName, string xmlNamespace)
    {
        var serializer = new XmlSerializer(typeof(T));
        using var fileStream = new FileStream(fileName, FileMode.Open);
        serializer.UnknownNode += (sender, e) => Console.WriteLine($"Unknown Node: {e.Name} - {e.Text}");
        serializer.UnknownAttribute += (sender, e) => Console.WriteLine($"Unknown Attribute: {e.Attr.Name}='{e.Attr.Value}'");
        serializer.UnknownElement += (sender, e) => Console.WriteLine($"Unknown Element: {e.Element.Name}");
        serializer.UnreferencedObject += (sender, e) => Console.WriteLine($"Unreferenced Object: {e.UnreferencedId}");
        var o = serializer.Deserialize(fileStream);
        return (T)o;
    }
    
    public BaseNode GetNodeByClass(string className)
    {
        if (_nodesByFullClassName.TryGetValue(className, out var node))
            return node;
        
        throw new KeyNotFoundException($"Class {className} not found in the structure referential.");
    }

    public T GetNodeByClass<T>(string className) where T : BaseNode => (T)GetNodeByClass(className);

    private void PostImport(BaseNode node)
    {
        var fc = node.FullClass;
        if (fc != null)
        {
            _nodesByFullClassName.Add(fc, node);
        }
        foreach (var child in node.Children)
        {
            child.Parent = node;
            PostImport(child);
        }
    }
}