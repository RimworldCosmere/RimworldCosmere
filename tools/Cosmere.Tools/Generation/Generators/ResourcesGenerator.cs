using System.IO.Abstractions;
using Cosmere.Tools.Data;
using Cosmere.Tools.Models;

namespace Cosmere.Tools.Generation.Generators;

public class ResourcesGenerator : BaseGenerator
{
    private readonly DataLoader _dataLoader;
    
    public ResourcesGenerator(GeneratorOptions options, IFileSystem fileSystem) : base(options, fileSystem)
    {
        _dataLoader = new DataLoader(fileSystem);
    }

    public override async Task GenerateAsync()
    {
        await GenerateMetalsAsync();
        await GenerateGemsAsync();
    }

    private async Task GenerateMetalsAsync()
    {
        var metals = await _dataLoader.LoadAllAsync<MetalInfo>("Metals");
        var enabledMetals = metals.Where(m => !IsDisabled(m)).ToList();
        
        var templatesDir = FileSystem.Path.Combine(".scripts", "Generators", "Resources", "metals");
        var resourcesModDir = FileSystem.Path.Combine("CosmereResources");
        
        // Generate MetalDefs
        var metalDefTemplate = CompileTemplate(templatesDir, "MetalDef.xml.template");
        var metalDefOutputDir = FileSystem.Path.Combine(resourcesModDir, "Defs", "Metal");
        
        foreach (var metal in enabledMetals)
        {
            var content = metalDefTemplate(new { metal });
            WriteGeneratedFile(metalDefOutputDir, $"{ToDefName(metal.Name)}.generated.xml", content);
        }

        // Generate MetalDefOf
        var metalDefOfTemplate = CompileTemplate(templatesDir, "MetalDefOf.cs.template");
        var metalDefOfContent = metalDefOfTemplate(new { metals = enabledMetals });
        WriteGeneratedFile(FileSystem.Path.Combine(resourcesModDir, "CosmereResources"), "MetalDefOf.generated.cs", metalDefOfContent);

        // Generate Mineable metals
        var mineableMetals = enabledMetals.Where(m => m.Mining != null).ToList();
        if (mineableMetals.Any())
        {
            var mineableTemplate = CompileTemplate(templatesDir, "MineableMetalDef.xml.template");
            var mineableOutputDir = FileSystem.Path.Combine(resourcesModDir, "Defs", "Thing", "Metal", "Mineable");
            
            foreach (var metal in mineableMetals)
            {
                var content = mineableTemplate(new { metal });
                WriteGeneratedFile(mineableOutputDir, $"{ToDefName(metal.Name)}.generated.xml", content);
            }

            var thingDefOfMineableTemplate = CompileTemplate(templatesDir, "ThingDefOf.Metal.Mineable.cs.template");
            var mineableDefOfContent = thingDefOfMineableTemplate(new { metals = mineableMetals });
            WriteGeneratedFile(FileSystem.Path.Combine(resourcesModDir, "CosmereResources"), "ThingDefOf.Metal.Mineable.generated.cs", mineableDefOfContent);
        }

        // Generate Item metals
        var itemTemplate = CompileTemplate(templatesDir, "ItemMetalDef.xml.template");
        var itemOutputDir = FileSystem.Path.Combine(resourcesModDir, "Defs", "Thing", "Metal", "Item");
        
        foreach (var metal in enabledMetals)
        {
            var content = itemTemplate(new { metal });
            WriteGeneratedFile(itemOutputDir, $"{ToDefName(metal.Name)}.generated.xml", content);
        }

        var thingDefOfItemTemplate = CompileTemplate(templatesDir, "ThingDefOf.Metal.Item.cs.template");
        var itemDefOfContent = thingDefOfItemTemplate(new { metals = enabledMetals });
        WriteGeneratedFile(FileSystem.Path.Combine(resourcesModDir, "CosmereResources"), "ThingDefOf.Metal.Items.generated.cs", itemDefOfContent);
    }

    private async Task GenerateGemsAsync()
    {
        var gems = await _dataLoader.LoadAllAsync<GemInfo>("Gems");
        var enabledGems = gems.Where(g => !IsDisabled(g)).ToList();
        
        var templatesDir = FileSystem.Path.Combine(".scripts", "Generators", "Resources", "gems");
        var resourcesModDir = FileSystem.Path.Combine("CosmereResources");
        
        // Generate GemDefs
        var gemDefTemplate = CompileTemplate(templatesDir, "GemDef.xml.template");
        var gemDefOutputDir = FileSystem.Path.Combine(resourcesModDir, "Defs", "Gem");
        
        foreach (var gem in enabledGems)
        {
            var content = gemDefTemplate(new { gem });
            WriteGeneratedFile(gemDefOutputDir, $"{ToDefName(gem.Name)}.generated.xml", content);
        }

        // Generate GemDefOf
        var gemDefOfTemplate = CompileTemplate(templatesDir, "GemDefOf.cs.template");
        var gemDefOfContent = gemDefOfTemplate(new { gems = enabledGems });
        WriteGeneratedFile(FileSystem.Path.Combine(resourcesModDir, "CosmereResources"), "GemDefOf.generated.cs", gemDefOfContent);

        // Generate Mineable gems
        var mineableGems = enabledGems.Where(g => g.Mining != null).ToList();
        if (mineableGems.Any())
        {
            var mineableTemplate = CompileTemplate(templatesDir, "MineableGemDef.xml.template");
            var mineableOutputDir = FileSystem.Path.Combine(resourcesModDir, "Defs", "Thing", "Gem", "Mineable");
            
            foreach (var gem in mineableGems)
            {
                var content = mineableTemplate(new { gem });
                WriteGeneratedFile(mineableOutputDir, $"{ToDefName(gem.Name)}.generated.xml", content);
            }

            var thingDefOfMineableTemplate = CompileTemplate(templatesDir, "ThingDefOf.Gems.Mineable.cs.template");
            var mineableDefOfContent = thingDefOfMineableTemplate(new { gems = mineableGems });
            WriteGeneratedFile(FileSystem.Path.Combine(resourcesModDir, "CosmereResources"), "ThingDefOf.Gems.Mineable.generated.cs", mineableDefOfContent);
        }

        // Generate Item gems
        var itemTemplate = CompileTemplate(templatesDir, "ItemGemDef.xml.template");
        var itemOutputDir = FileSystem.Path.Combine(resourcesModDir, "Defs", "Thing", "Gem", "Item");
        
        foreach (var gem in enabledGems)
        {
            var content = itemTemplate(new { gem });
            WriteGeneratedFile(itemOutputDir, $"{ToDefName(gem.Name)}.generated.xml", content);
        }

        var thingDefOfItemTemplate = CompileTemplate(templatesDir, "ThingDefOf.Gems.Item.cs.template");
        var itemDefOfContent = thingDefOfItemTemplate(new { gems = enabledGems });
        WriteGeneratedFile(FileSystem.Path.Combine(resourcesModDir, "CosmereResources"), "ThingDefOf.Gems.Item.generated.cs", itemDefOfContent);
    }

    private static bool IsDisabled(dynamic item)
    {
        return item.GetType().GetProperty("Disabled")?.GetValue(item) as bool? ?? false;
    }

    private static string ToDefName(string name)
    {
        return name.Replace(" ", string.Empty);
    }
}