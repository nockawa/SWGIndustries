using System.Diagnostics;
using JetBrains.Annotations;

namespace SWGIndustries.Services;

[PublicAPI]
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
            NestedLevel = parent.NestedLevel + 1;
        }
        else
        {
            CategoryIndices[0] = Index;
            NestedLevel = 0;
        }
    }

    public ushort[] CategoryIndices { get; }
    
    /// <summary>
    /// 0 based nested-level of this category.
    /// </summary>
    public int NestedLevel { get; }

    public ResourceCategory Parent { get; }
    public string Name { get; set; }
    public ushort Index { get; set; }
    public List<ResourceCategory> SubCategories { get; } = new();
    public List<Resource> Resources { get; } = new();

    public void AddResource(Resource resource)
    {
        resource.Category = this;
        var cat = this;
        while (cat != null)
        {
            cat.Resources.Add(resource);
            cat = cat.Parent;
        }
    }

    public void Remove(Resource resource)
    {
        resource.Category = null;
        var cat = this;
        while (cat != null)
        {
            cat.Resources.Remove(resource);
            cat = cat.Parent;
        }
    }
}