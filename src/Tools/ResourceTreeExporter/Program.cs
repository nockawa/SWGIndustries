using System.Dynamic;
using System.Xml.Linq;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;

namespace ResourceTreeExporter;

// ReSharper disable once ClassNeverInstantiated.Global
public class ResourceCategory
{
    [Name("INDEX")]             public int Index { get; set; }
    [Name("ENUM")]              public string Enum { get; set; }
    [Name("CLASS 1")]           public string Class1 { get; set; }
    [Name("CLASS 2")]           public string Class2 { get; set; }
    [Name("CLASS 3")]           public string Class3 { get; set; }
    [Name("CLASS 4")]           public string Class4 { get; set; }
    [Name("CLASS 5")]           public string Class5 { get; set; }
    [Name("CLASS 6")]           public string Class6 { get; set; }
    [Name("CLASS 7")]           public string Class7 { get; set; }
    [Name("CLASS 8")]           public string Class8 { get; set; }

    public int ClassLevel
    {
        get
        {
            if (!string.IsNullOrEmpty(Class1)) return 1;
            if (!string.IsNullOrEmpty(Class2)) return 2;
            if (!string.IsNullOrEmpty(Class3)) return 3;
            if (!string.IsNullOrEmpty(Class4)) return 4;
            if (!string.IsNullOrEmpty(Class5)) return 5;
            if (!string.IsNullOrEmpty(Class6)) return 6;
            if (!string.IsNullOrEmpty(Class7)) return 7;
            if (!string.IsNullOrEmpty(Class8)) return 8;

            return -1;
        }
    }

    public string ClassName
    {
        get
        {
            return ClassLevel switch
            {
                1 => Class1,
                2 => Class2,
                3 => Class3,
                4 => Class4,
                5 => Class5,
                6 => Class6,
                7 => Class7,
                8 => Class8,
                _ => throw new Exception()
            };
        }
    }
}

static class Program
{
    static void Main(string[] args)
    {
        var csvFilePath = "resource_tree.txt";
        
        using (var reader = new StreamReader(csvFilePath))
        {
            var readerConfiguration = new CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture)
            {
                Delimiter = "\t"
            };
            
            using var csv = new CsvReader(reader, readerConfiguration);

            // Read header
            csv.Read();
            csv.ReadHeader();
            
            // Skip the first row, because it is used to specify the columns type
            csv.Read();

            // XmlDocument
            var xmlDoc = new XDocument();

            XElement previousNode = null;
            var depthStack = new Stack<XElement>();
            
            while (csv.Read())
            {
                var record = csv.GetRecord<ResourceCategory>();

                var node = new XElement("ResourceCategory");
                node.Add(new XAttribute("Name", record.ClassName));
                
                var nodeLevel = record.ClassLevel;
                
                node.Add(new XAttribute("Index", record.Index));
                
                if (depthStack.Count == 0)
                {
                    depthStack.Push(node);
                    xmlDoc.Add(node);
                }
                else
                {
                    // Same level as previous node
                    if (depthStack.Count+1 == nodeLevel)
                    {
                        depthStack.Peek().Add(node);
                    }
                    
                    // One level deeper
                    else if (depthStack.Count+2 == nodeLevel)
                    {
                        depthStack.Push(previousNode);
                        previousNode.Add(node);
                    }
                    
                    // One/many level up
                    else
                    {
                        var diff = depthStack.Count + 1 - nodeLevel;
                        while (diff-- > 0)
                        {
                            depthStack.Pop();
                        }
                        depthStack.Peek().Add(node);
                    }
                }
                
                previousNode = node;
            }

            xmlDoc.Save("resource_tree.xml");
        }

        Console.ReadKey();
    }
}