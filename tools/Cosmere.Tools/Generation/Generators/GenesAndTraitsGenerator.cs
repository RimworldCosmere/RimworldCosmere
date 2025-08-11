using System.IO.Abstractions;
using Cosmere.Tools.Data;
using Cosmere.Tools.Models;

namespace Cosmere.Tools.Generation.Generators;

public class GenesAndTraitsGenerator : BaseGenerator
{
    private readonly DataLoader _dataLoader;
    
    public GenesAndTraitsGenerator(GeneratorOptions options, IFileSystem fileSystem) : base(options, fileSystem)
    {
        _dataLoader = new DataLoader(fileSystem);
    }

    public override async Task GenerateAsync()
    {
        var metals = await _dataLoader.LoadAllAsync<MetalInfo>("Metals");
        var enabledMetals = metals.Where(m => !IsDisabled(m)).ToList();
        
        await GenerateAllomancyAsync(enabledMetals);
        await GenerateFeruchemyAsync(enabledMetals);
        await GenerateMistbornAsync(enabledMetals);
        await GenerateFullFeruchemistAsync(enabledMetals);
    }

    private async Task GenerateAllomancyAsync(List<MetalInfo> metals)
    {
        var allomancyMetals = metals.Where(m => !m.GodMetal && m.Allomancy != null).Concat(new[] { metals.First(m => m.Name == "Atium") }).ToList();
        
        var templatesDir = FileSystem.Path.Combine(".scripts", "Generators", "GenesAndTraits");
        var scadrialModDir = FileSystem.Path.Combine("CosmereScadrial");
        var outputDir = FileSystem.Path.Combine(scadrialModDir, "Defs", "Allomancy");
        
        var geneTemplate = CompileTemplate(templatesDir, "AllomancyGeneDef.xml.template");
        var traitTemplate = CompileTemplate(templatesDir, "AllomancyTraitDef.xml.template");
        var defOfTemplate = CompileTemplate(templatesDir, "DefOf.cs.template");
        
        int order = 2;
        foreach (var metal in allomancyMetals)
        {
            var defName = metal.DefName ?? ToDefName(metal.Name);
            var metalOutputDir = FileSystem.Path.Combine(outputDir, defName);
            
            var geneContent = geneTemplate(new { metal, defName, order = order++ });
            WriteGeneratedFile(metalOutputDir, "Gene.generated.xml", geneContent);
            
            var traitContent = traitTemplate(new { metal, defName, order = order++ });
            WriteGeneratedFile(metalOutputDir, "Trait.generated.xml", traitContent);
        }

        // Generate DefOf files
        var geneDefOfContent = defOfTemplate(new { type = "Misting", kind = "Gene", metals = allomancyMetals });
        WriteGeneratedFile(FileSystem.Path.Combine(scadrialModDir, "CosmereScadrial"), "GeneDefOf.Allomancy.generated.cs", geneDefOfContent);
        
        var traitDefOfContent = defOfTemplate(new { type = "Misting", kind = "Trait", metals = allomancyMetals });
        WriteGeneratedFile(FileSystem.Path.Combine(scadrialModDir, "CosmereScadrial"), "TraitDefOf.Allomancy.generated.cs", traitDefOfContent);
    }

    private async Task GenerateFeruchemyAsync(List<MetalInfo> metals)
    {
        var feruchemyMetals = metals.Where(m => !m.GodMetal && m.Feruchemy != null).Concat(new[] { metals.First(m => m.Name == "Atium") }).ToList();
        
        var templatesDir = FileSystem.Path.Combine(".scripts", "Generators", "GenesAndTraits");
        var scadrialModDir = FileSystem.Path.Combine("CosmereScadrial");
        var outputDir = FileSystem.Path.Combine(scadrialModDir, "Defs", "Feruchemy");
        
        var geneTemplate = CompileTemplate(templatesDir, "FeruchemyGeneDef.xml.template");
        var traitTemplate = CompileTemplate(templatesDir, "FeruchemyTraitDef.xml.template");
        var defOfTemplate = CompileTemplate(templatesDir, "DefOf.cs.template");
        
        int order = 2;
        foreach (var metal in feruchemyMetals)
        {
            var defName = metal.DefName ?? ToDefName(metal.Name);
            var metalOutputDir = FileSystem.Path.Combine(outputDir, defName);
            
            var geneContent = geneTemplate(new { metal, defName, order = order++ });
            WriteGeneratedFile(metalOutputDir, "Gene.generated.xml", geneContent);
            
            var traitContent = traitTemplate(new { metal, defName, order = order++ });
            WriteGeneratedFile(metalOutputDir, "Trait.generated.xml", traitContent);
        }

        // Generate DefOf files
        var geneDefOfContent = defOfTemplate(new { type = "Ferring", kind = "Gene", metals = feruchemyMetals });
        WriteGeneratedFile(FileSystem.Path.Combine(scadrialModDir, "CosmereScadrial"), "GeneDefOf.Feruchemy.generated.cs", geneDefOfContent);
        
        var traitDefOfContent = defOfTemplate(new { type = "Ferring", kind = "Trait", metals = feruchemyMetals });
        WriteGeneratedFile(FileSystem.Path.Combine(scadrialModDir, "CosmereScadrial"), "TraitDefOf.Feruchemy.generated.cs", traitDefOfContent);
    }

    private async Task GenerateMistbornAsync(List<MetalInfo> metals)
    {
        var allomancyMetals = metals.Where(m => !m.GodMetal && m.Allomancy != null).Concat(new[] { metals.First(m => m.Name == "Atium") }).ToList();
        var abilities = allomancyMetals.SelectMany(m => m.Allomancy?.Abilities ?? new List<string>()).ToList();
        
        var templatesDir = FileSystem.Path.Combine(".scripts", "Generators", "GenesAndTraits");
        var scadrialModDir = FileSystem.Path.Combine("CosmereScadrial");
        var outputDir = FileSystem.Path.Combine(scadrialModDir, "Defs", "Allomancy", "Mistborn");
        
        var template = CompileTemplate(templatesDir, "Mistborn.xml.template");
        var content = template(new { metals = allomancyMetals, abilities, rightClickAbilities = new List<string>() });
        WriteGeneratedFile(outputDir, "Trait.generated.xml", content);
    }

    private async Task GenerateFullFeruchemistAsync(List<MetalInfo> metals)
    {
        var feruchemyMetals = metals.Where(m => !m.GodMetal && m.Feruchemy != null).Concat(new[] { metals.First(m => m.Name == "Atium") }).ToList();
        var abilities = feruchemyMetals.SelectMany(m => m.Feruchemy?.Abilities ?? new List<string>()).ToList();
        
        var templatesDir = FileSystem.Path.Combine(".scripts", "Generators", "GenesAndTraits");
        var scadrialModDir = FileSystem.Path.Combine("CosmereScadrial");
        var outputDir = FileSystem.Path.Combine(scadrialModDir, "Defs", "Feruchemy", "FullFeruchemist");
        
        var template = CompileTemplate(templatesDir, "FullFeruchemist.xml.template");
        var content = template(new { metals = feruchemyMetals, abilities, rightClickAbilities = new List<string>() });
        WriteGeneratedFile(outputDir, "Trait.generated.xml", content);
    }

    private static bool IsDisabled(MetalInfo metal)
    {
        return metal.GetType().GetProperty("Disabled")?.GetValue(metal) as bool? ?? false;
    }

    private static string ToDefName(string name)
    {
        return name.Replace(" ", string.Empty);
    }
}