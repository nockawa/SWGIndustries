using System.Diagnostics;
using JetBrains.Annotations;
using SWGIndustries.Data;

namespace SWGIndustries.Services;

[PublicAPI]
[DebuggerDisplay("Name: {Name}, since {AvailableSince}")]
public class Resource
{
    public Resource(Data.ResourceEntity model)
    {
        SWGAideId = model.SWGAideId;
        Name = model.Name;
        Planets = model.Planets;
        AvailableSince = model.AvailableSince;
        DepletedSince = model.DepletedSince;
        ReportedBy = model.ReportedBy;
        CategoryIndex = model.CategoryIndex;
        
        CR = model.CR;
        CD = model.CD;
        DR = model.DR;
        ER = model.ER;
        FL = model.FL;
        HR = model.HR;
        MA = model.MA;
        OQ = model.OQ;
        PE = model.PE;
        SR = model.SR;
        UT = model.UT;
    }

    public Resource(XmlResource xmlResource, ushort categoryIndex)
    {
        CategoryIndex = categoryIndex;
        Name = xmlResource.Name;
        if (int.TryParse(xmlResource.SwgAideId, out var swgAideId) == false)
        {
            // TODO log error
            return;
        }
        SWGAideId = swgAideId;
        
        AvailableSince = DateTimeOffset.FromUnixTimeSeconds(xmlResource.AvailableTimestamp).DateTime;
        ReportedBy = xmlResource.AvailableBy;

        CR = xmlResource.Stats.CR;
        CD = xmlResource.Stats.CD;
        DR = xmlResource.Stats.DR;
        ER = xmlResource.Stats.ER;
        FL = xmlResource.Stats.FL;
        HR = xmlResource.Stats.HR;
        MA = xmlResource.Stats.MA;
        OQ = xmlResource.Stats.OQ;
        PE = xmlResource.Stats.PE;
        SR = xmlResource.Stats.SR;
        UT = xmlResource.Stats.UT;

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
            Planets |= p;
        }
    }

    public string Name { get; set; }
    public ResourceCategory Category { get; internal set; }
    internal ushort CategoryIndex { get; private set; }         // Always set, even if the resource is not (yet) in a category

    public int SWGAideId { get; private set; }

    public Planet Planets { get; private set; }
    public ushort CR { get; private set; }
    public ushort CD { get; private set; }
    public ushort DR { get; private set; }
    public ushort ER { get; private set; }
    public ushort FL { get; private set; }
    public ushort HR { get; private set; }
    public ushort MA { get; private set; }
    public ushort OQ { get; private set; }
    public ushort PE { get; private set; }
    public ushort SR { get; private set; }
    public ushort UT { get; private set; }

    public DateTime AvailableSince { get; private set; }
    public DateTime? DepletedSince { get; internal set; }
    public string ReportedBy { get; private set; }
    public bool IsNotQualified => (CR + CD + DR + ER + FL + HR + MA + OQ + PE + SR + UT) == 0;

    public Data.ResourceEntity ToModel()
    {
        var model = new Data.ResourceEntity
        {
            Name = Name,
            CategoryIndex = Category.Index,
            SWGAideId = SWGAideId,
            AvailableSince = AvailableSince,
            DepletedSince = DepletedSince,
            ReportedBy = ReportedBy,
            Planets = Planets,
            CR = CR,
            CD = CD,
            DR = DR,
            ER = ER,
            FL = FL,
            HR = HR,
            MA = MA,
            OQ = OQ,
            PE = PE,
            SR = SR,
            UT = UT
        };

        var cat = Category;
        for (var i = Category.NestedLevel; i >= 0; i--)
        {
            if (cat == null)
                break;

            switch (i)
            {
                case 0:
                    model.CI0 = cat.Index;
                    break;
                case 1:
                    model.CI1 = cat.Index;
                    break;
                case 2:
                    model.CI2 = cat.Index;
                    break;
                case 3:
                    model.CI3 = cat.Index;
                    break;
                case 4:
                    model.CI4 = cat.Index;
                    break;
                case 5:
                    model.CI5 = cat.Index;
                    break;
                case 6:
                    model.CI6 = cat.Index;
                    break;
                case 7:
                    model.CI7 = cat.Index;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            cat = cat.Parent;
        }

        return model;
    }

    public bool IsOutDated(Resource newResource) =>
        Planets != newResource.Planets ||
        CR != newResource.CR ||
        CD != newResource.CD ||
        DR != newResource.DR ||
        ER != newResource.ER ||
        FL != newResource.FL ||
        HR != newResource.HR ||
        MA != newResource.MA ||
        OQ != newResource.OQ ||
        PE != newResource.PE ||
        SR != newResource.SR ||
        UT != newResource.UT;

    public void UpdateFrom(Resource newResource)
    {
        Planets = newResource.Planets;
        CR = newResource.CR;
        CD = newResource.CD;
        DR = newResource.DR;
        ER = newResource.ER;
        FL = newResource.FL;
        HR = newResource.HR;
        MA = newResource.MA;
        OQ = newResource.OQ;
        PE = newResource.PE;
        SR = newResource.SR;
        UT = newResource.UT;
    }
}