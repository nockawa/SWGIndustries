using System.Diagnostics;
using System.IO.Compression;
using System.Xml;
using System.Xml.Serialization;

namespace SWGAideResourceImport;

[Flags]
public enum Planet
{
    Undefined   = 0,
    Corellia    = 0x0001,
    Dantooine   = 0x0002,
    Dathomir    = 0x0004,
    Endor       = 0x0008,
    Kashyyyk    = 0x0010,
    Lok         = 0x0020,
    Mustafar    = 0x0040,
    Naboo       = 0x0080,
    Rori        = 0x0100,
    Talus       = 0x0200,
    Tatooine    = 0x0400,
    Yavin4      = 0x0800
}

[XmlRoot(ElementName = "resource_data", Namespace = "")]
public class XmlResourceData
{
    [XmlAttribute("server_id")]
    public string ServerId { get; set; }

    [XmlAttribute("server_name")]
    public string ServerName { get; set; }

    [XmlAttribute("generated")]
    public string GeneratedDateTime { get; set; }

    [XmlArray("resources")]
    [XmlArrayItem("resource")]
    public List<XmlResource> Resources { get; set; }
}

[XmlRoot(ElementName = "resource", Namespace = "")]
public class XmlResource
{
    [XmlAttribute("swgaide_id")]
    public string SwgAideId { get; set; }

    [XmlElement("name")]
    public string Name { get; set; }

    [XmlElement("type")]
    public string Type { get; set; }

    [XmlElement("swgaide_type_id")]
    public string SwgAideTypeId { get; set; }

    [XmlElement("stats")]
    public XmlStats Stats { get; set; }

    [XmlArray("planets")]
    [XmlArrayItem("planet")]
    public List<XmlPlanet> Planets { get; set; }

    [XmlElement("waypoints")]
    public string Waypoints { get; set; }

    [XmlElement("available_timestamp")]
    public long AvailableTimestamp { get; set; }

    [XmlElement("available_by")]
    public string AvailableBy { get; set; }
}

public class XmlStats
{
    [XmlElement("cr")]
    public ushort CR { get; set; }

    [XmlElement("cd")]
    public ushort CD { get; set; }

    [XmlElement("dr")]
    public ushort DR { get; set; }

    [XmlElement("er")]
    public ushort ER { get; set; }

    [XmlElement("fl")]
    public ushort FL { get; set; }

    [XmlElement("hr")]
    public ushort HR { get; set; }

    [XmlElement("ma")]
    public ushort MA { get; set; }

    [XmlElement("oq")]
    public ushort OQ { get; set; }

    [XmlElement("pe")]
    public ushort PE { get; set; }

    [XmlElement("sr")]
    public ushort SR { get; set; }

    [XmlElement("ut")]
    public ushort UT { get; set; }
}

[DebuggerDisplay("{Name} ({Index})")]
[XmlRoot(ElementName = "ResourceCategory")]
public class XmlResourceCategory
{
    [XmlAttribute("Name")]
    public string Name { get; set; }

    [XmlAttribute("Index")]
    public ushort Index { get; set; }

    [XmlElement("ResourceCategory")]
    public List<XmlResourceCategory> SubCategories { get; set; } = [];
}

[DebuggerDisplay("{Name} ({Index}) Total Resources: {Resources.Count}")]
public class ResourceCategory
{
    public ResourceCategory(ResourceCategory parent, XmlResourceCategory xmlCategory)
    {
        Parent = parent;
        Name = xmlCategory.Name;
        Index = xmlCategory.Index;
        CategoryIndices = new ushort[8];
        
        if (parent != null)
        {
            parent.SubCategories.Add(this);

            var i = 0;
            while (parent.CategoryIndices[i] != 0)
            {
                CategoryIndices[i] = parent.CategoryIndices[i];
                i++;
            }
            CategoryIndices[i] = Index;
        }
        else
        {
            CategoryIndices[0] = Index;
        }
    }

    public ushort[] CategoryIndices { get; }

    public ResourceCategory Parent { get; }
    public string Name { get; set; }
    public ushort Index { get; set; }
    public List<ResourceCategory> SubCategories { get; } = new();
    public List<Resource> Resources { get; } = new();

    public void AddResource(Resource resource)
    {
        var cat = this;
        while (cat != null)
        {
            cat.Resources.Add(resource);
            cat = cat.Parent;
        }
    }
}

public class Resource
{
    private int _swgAideId;
    public string Name { get; set; }

    public ref int SwgAideId => ref _swgAideId;

    public Planet Planets { get; set; }
    public ushort CR { get; set; }
    public ushort CD { get; set; }
    public ushort DR { get; set; }
    public ushort ER { get; set; }
    public ushort FL { get; set; }
    public ushort HR { get; set; }
    public ushort MA { get; set; }
    public ushort OQ { get; set; }
    public ushort PE { get; set; }
    public ushort SR { get; set; }
    public ushort UT { get; set; }

    public DateTime AvailableSince { get; set; }
    public string ReportedBy { get; set; }
}

public class XmlPlanet
{
    [XmlAttribute("swgaide_id")]
    public string SwgAideId { get; set; }

    [XmlElement("name")]
    public string Name { get; set; }
}
public class ResourceImport
{
    private Dictionary<string, ResourceCategory> _resourceCategories = new();
    private ResourceCategory _rootCategory;

    public async Task Update()
    {
        // Load the resource tree from XML
        try
        {
            var serializer = new XmlSerializer(typeof(XmlResourceCategory));
            await using FileStream fileStream = new FileStream("resource_tree.xml", FileMode.Open);
            var rootCategory = (XmlResourceCategory)serializer.Deserialize(fileStream);
    
            _rootCategory = BuildResourceCategories(null, rootCategory);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        // Load current resources from SWGAide for the Restoration 3 server
        var url = "https://swgaide.com/pub/exports/currentresources_162.xml.gz";
        try
        {
            using HttpClient client = new HttpClient();
            await using var fileStream = await client.GetStreamAsync(url);
            await using var gzipStream = new GZipStream(fileStream, CompressionMode.Decompress);
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(gzipStream);
            var serializer = new XmlSerializer(typeof(XmlResourceData));
            using var reader = new XmlNodeReader(xmlDoc);
            var resources = (XmlResourceData)serializer.Deserialize(reader)!;

            resources.Resources.ForEach(LoadResource);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    private void LoadResource(XmlResource xmlResource)
    {
        var r = new Resource();
        r.Name = xmlResource.Name;
        if (int.TryParse(xmlResource.SwgAideId, out r.SwgAideId) == false)
        {
            // TODO log error
            return;
        }
        
        if (_resourceCategories.TryGetValue(xmlResource.Type, out var cat) == false)
        {
            // TODO log error
            return;
        }
        cat.AddResource(r);
        
        r.AvailableSince = DateTimeOffset.FromUnixTimeSeconds(xmlResource.AvailableTimestamp).DateTime;
        r.ReportedBy = xmlResource.AvailableBy;

        r.CR = xmlResource.Stats.CR;
        r.CD = xmlResource.Stats.CD;
        r.DR = xmlResource.Stats.DR;
        r.ER = xmlResource.Stats.ER;
        r.FL = xmlResource.Stats.FL;
        r.HR = xmlResource.Stats.HR;
        r.MA = xmlResource.Stats.MA;
        r.OQ = xmlResource.Stats.OQ;
        r.PE = xmlResource.Stats.PE;
        r.SR = xmlResource.Stats.SR;
        r.UT = xmlResource.Stats.UT;

        foreach (var xmlPlanet in xmlResource.Planets)
        {
            var planetName = xmlPlanet.Name;
            if (planetName == "Yavin 4")
            {
                planetName = "Yavin4";
            }
            
            if (Enum.TryParse<Planet>(planetName, true, out var p) == false)
            {
                // TODO log error
                Console.WriteLine($"Couldn't find planet {planetName} in enum Planet");
                continue;
            }
            r.Planets |= p;
        }
    }

    private ResourceCategory BuildResourceCategories(ResourceCategory parent, XmlResourceCategory xmlCategory)
    {
        var cat = new ResourceCategory(parent, xmlCategory);
        foreach (var subCategory in xmlCategory.SubCategories)
        {
            var child = BuildResourceCategories(cat, subCategory);
        }
        _resourceCategories.Add(cat.Name, cat);
        return cat;
    }    
}