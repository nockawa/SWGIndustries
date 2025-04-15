namespace SWGAideResourceImport;

class Program
{
    static void Main(string[] args)
    {
        var resourceImport = new ResourceImport();
        resourceImport.Update().GetAwaiter().GetResult();
        
        Console.WriteLine("Done");        
    }
}