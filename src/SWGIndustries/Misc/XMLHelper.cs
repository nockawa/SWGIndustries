using System.Xml.Serialization;

namespace SWGIndustries;

public static class XMLHelper
{
    public static T LoadXML<T>(string fileName)
    {
        var serializer = new XmlSerializer(typeof(T));
        using var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        return (T)serializer.Deserialize(fileStream);
    }

    
}