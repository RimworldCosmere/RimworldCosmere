using System.IO.Abstractions;
using Cosmere.Tools.Data;
using Cosmere.Tools.Extensions;
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
        var allomancyMetals = metals.Where(m => !m.GodMetal && m.Allomancy != null).ToList();
        var atium = metals.FirstOrDefault(m => m.Name.Equals("Atium", StringComparison.OrdinalIgnoreCase));
        if (atium != null)
        {
            allomancyMetals.Add(atium);
        }
        
        var templatesDir = FileSystem.Path.Combine("Resources", "Templates", "GenesAndTraits");
        var scadrialXmlDir = FileSystem.Path.Combine("CosmereScadrial");
        var scadrialCsDir = FileSystem.Path.Combine("CosmereCore", "CosmereCore", "System", "Scadrial");
        var outputDir = FileSystem.Path.Combine(scadrialXmlDir, "Defs", "Allomancy");
        
        // Compile templates in parallel
        var geneTemplateTask = CompileTemplateAsync(templatesDir, "AllomancyGeneDef.xml.template");
        var traitTemplateTask = CompileTemplateAsync(templatesDir, "AllomancyTraitDef.xml.template");
        var defOfTemplateTask = CompileTemplateAsync(templatesDir, "DefOf.cs.template");
        
        await Task.WhenAll(geneTemplateTask, traitTemplateTask, defOfTemplateTask);
        
        var geneTemplate = geneTemplateTask.Result;
        var traitTemplate = traitTemplateTask.Result;
        var defOfTemplate = defOfTemplateTask.Result;
        
        // Generate metal files in parallel
        var fileWriteTasks = new List<Task>();
        int order = 2;
        
        foreach (var metal in allomancyMetals)
        {
            var defName = metal.DefName ?? metal.Name.ToDefName();
            var metalOutputDir = FileSystem.Path.Combine(outputDir, defName);
            
            var geneContent = geneTemplate(new { metal, defName, order = order++ });
            fileWriteTasks.Add(WriteGeneratedFileAsync(metalOutputDir, metal.Name.ToDefName() + "Gene.generated.xml", geneContent));
            
            var traitContent = traitTemplate(new { metal, defName, order = order++ });
            fileWriteTasks.Add(WriteGeneratedFileAsync(metalOutputDir, metal.Name.ToDefName() + "Trait.generated.xml", traitContent));
        }

        // Generate DefOf files
        var geneDefOfContent = defOfTemplate(new { type = "Misting", kind = "Gene", metals = allomancyMetals });
        fileWriteTasks.Add(WriteGeneratedFileAsync(scadrialCsDir, "GeneDefOf.Allomancy.generated.cs", geneDefOfContent));
        
        var traitDefOfContent = defOfTemplate(new { type = "Misting", kind = "Trait", metals = allomancyMetals });
        fileWriteTasks.Add(WriteGeneratedFileAsync(scadrialCsDir, "TraitDefOf.Allomancy.generated.cs", traitDefOfContent));
        
        // Wait for all file writes to complete
        await Task.WhenAll(fileWriteTasks);
    }

    private async Task GenerateFeruchemyAsync(List<MetalInfo> metals)
    {
        var feruchemyMetals = metals.Where(m => !m.GodMetal && m.Feruchemy != null).ToList();
        var atium = metals.FirstOrDefault(m => m.Name.Equals("Atium", StringComparison.OrdinalIgnoreCase));
        if (atium != null)
        {
            feruchemyMetals.Add(atium);
        }
        
        var templatesDir = FileSystem.Path.Combine("Resources", "Templates", "GenesAndTraits");
        var scadrialXmlDir = FileSystem.Path.Combine("CosmereScadrial");
        var scadrialCsDir = FileSystem.Path.Combine("CosmereCore", "CosmereCore", "System", "Scadrial");
        var outputDir = FileSystem.Path.Combine(scadrialXmlDir, "Defs", "Feruchemy");
        
        // Compile templates in parallel
        var geneTemplateTask = CompileTemplateAsync(templatesDir, "FeruchemyGeneDef.xml.template");
        var traitTemplateTask = CompileTemplateAsync(templatesDir, "FeruchemyTraitDef.xml.template");
        var defOfTemplateTask = CompileTemplateAsync(templatesDir, "DefOf.cs.template");
        
        await Task.WhenAll(geneTemplateTask, traitTemplateTask, defOfTemplateTask);
        
        var geneTemplate = geneTemplateTask.Result;
        var traitTemplate = traitTemplateTask.Result;
        var defOfTemplate = defOfTemplateTask.Result;
        
        // Generate metal files in parallel
        var fileWriteTasks = new List<Task>();
        int order = 2;
        
        foreach (var metal in feruchemyMetals)
        {
            var defName = metal.DefName ?? metal.Name.ToDefName();
            var metalOutputDir = FileSystem.Path.Combine(outputDir, defName);
            
            var geneContent = geneTemplate(new { metal, defName, order = order++ });
            fileWriteTasks.Add(WriteGeneratedFileAsync(metalOutputDir, metal.Name.ToDefName() + "Gene.generated.xml", geneContent));
            
            var traitContent = traitTemplate(new { metal, defName, order = order++ });
            fileWriteTasks.Add(WriteGeneratedFileAsync(metalOutputDir, metal.Name.ToDefName() + "Trait.generated.xml", traitContent));
        }

        // Generate DefOf files
        var geneDefOfContent = defOfTemplate(new { type = "Ferring", kind = "Gene", metals = feruchemyMetals });
        fileWriteTasks.Add(WriteGeneratedFileAsync(scadrialCsDir, "GeneDefOf.Feruchemy.generated.cs", geneDefOfContent));
        
        var traitDefOfContent = defOfTemplate(new { type = "Ferring", kind = "Trait", metals = feruchemyMetals });
        fileWriteTasks.Add(WriteGeneratedFileAsync(scadrialCsDir, "TraitDefOf.Feruchemy.generated.cs", traitDefOfContent));
        
        // Wait for all file writes to complete
        await Task.WhenAll(fileWriteTasks);
    }

    private async Task GenerateMistbornAsync(List<MetalInfo> metals)
    {
        var allomancyMetals = metals.Where(m => !m.GodMetal && m.Allomancy != null).ToList();
        var atium = metals.FirstOrDefault(m => m.Name.Equals("Atium", StringComparison.OrdinalIgnoreCase));
        if (atium != null)
        {
            allomancyMetals.Add(atium);
        }
        var abilities = allomancyMetals.SelectMany(m => m.Allomancy?.Abilities ?? new List<string>()).ToList();
        
        var templatesDir = FileSystem.Path.Combine("Resources", "Templates", "GenesAndTraits");
        var scadrialXmlDir = FileSystem.Path.Combine("CosmereScadrial");
        var scadrialCsDir = FileSystem.Path.Combine("CosmereCore", "CosmereCore", "System", "Scadrial");
        var outputDir = FileSystem.Path.Combine(scadrialXmlDir, "Defs", "Allomancy", "Mistborn");
        
        var template = await CompileTemplateAsync(templatesDir, "Mistborn.xml.template");
        var content = template(new { metals = allomancyMetals, abilities, rightClickAbilities = new List<string>() });
        await WriteGeneratedFileAsync(outputDir, "Trait.generated.xml", content);
    }

    private async Task GenerateFullFeruchemistAsync(List<MetalInfo> metals)
    {
        var feruchemyMetals = metals.Where(m => !m.GodMetal && m.Feruchemy != null).ToList();
        var atium = metals.FirstOrDefault(m => m.Name.Equals("Atium", StringComparison.OrdinalIgnoreCase));
        if (atium != null)
        {
            feruchemyMetals.Add(atium);
        }
        var abilities = feruchemyMetals.SelectMany(m => m.Feruchemy?.Abilities ?? new List<string>()).ToList();
        
        var templatesDir = FileSystem.Path.Combine("Resources", "Templates", "GenesAndTraits");
        var scadrialXmlDir = FileSystem.Path.Combine("CosmereScadrial");
        var scadrialCsDir = FileSystem.Path.Combine("CosmereCore", "CosmereCore", "System", "Scadrial");
        var outputDir = FileSystem.Path.Combine(scadrialXmlDir, "Defs", "Feruchemy", "FullFeruchemist");
        
        var template = await CompileTemplateAsync(templatesDir, "FullFeruchemist.xml.template");
        var content = template(new { metals = feruchemyMetals, abilities, rightClickAbilities = new List<string>() });
        await WriteGeneratedFileAsync(outputDir, "Trait.generated.xml", content);
    }

    private static bool IsDisabled(MetalInfo metal)
    {
        return metal.GetType().GetProperty("Disabled")?.GetValue(metal) as bool? ?? false;
    }

}