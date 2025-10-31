using System.IO.Abstractions;
using Cosmere.Tools.Data;
using Cosmere.Tools.Extensions;
using Cosmere.Tools.Models;
using HandlebarsDotNet;

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
        var mineableMetals = enabledMetals.Where(m => m.Mining != null).ToList();
        
        var templatesDir = FileSystem.Path.Combine("Resources", "Templates", "Resources", "metals");
        var resourcesModDir = FileSystem.Path.Combine("CosmereCore");
        
        // Compile all templates in parallel
        var templateTasks = new List<Task<HandlebarsTemplate<object, object>>>
        {
            CompileTemplateAsync(templatesDir, "MetalDef.xml.template"),
            CompileTemplateAsync(templatesDir, "MetalDefOf.cs.template"),
            CompileTemplateAsync(templatesDir, "ItemMetalDef.xml.template"),
            CompileTemplateAsync(templatesDir, "ThingDefOf.Metal.Item.cs.template")
        };
        
        if (mineableMetals.Any())
        {
            templateTasks.Add(CompileTemplateAsync(templatesDir, "MineableMetalDef.xml.template"));
            templateTasks.Add(CompileTemplateAsync(templatesDir, "ThingDefOf.Metal.Mineable.cs.template"));
        }
        
        await Task.WhenAll(templateTasks);
        
        var metalDefTemplate = templateTasks[0].Result;
        var metalDefOfTemplate = templateTasks[1].Result;
        var itemTemplate = templateTasks[2].Result;
        var thingDefOfItemTemplate = templateTasks[3].Result;
        
        HandlebarsTemplate<object, object>? mineableTemplate = null;
        HandlebarsTemplate<object, object>? thingDefOfMineableTemplate = null;
        if (mineableMetals.Any())
        {
            mineableTemplate = templateTasks[4].Result;
            thingDefOfMineableTemplate = templateTasks[5].Result;
        }
        
        // Generate all files in parallel
        var fileWriteTasks = new List<Task>();
        
        // Generate MetalDefs
        var metalDefOutputDir = FileSystem.Path.Combine(resourcesModDir, "Defs", "Metal");
        foreach (var metal in enabledMetals)
        {
            var content = metalDefTemplate(new { metal });
            fileWriteTasks.Add(WriteGeneratedFileAsync(metalDefOutputDir, $"{metal.Name.ToDefName()}Metal.generated.xml", content));
        }

        // Generate MetalDefOf
        var metalDefOfContent = metalDefOfTemplate(new { metals = enabledMetals });
        fileWriteTasks.Add(WriteGeneratedFileAsync(FileSystem.Path.Combine(resourcesModDir, "CosmereCore", "Core"), "MetalDefOf.generated.cs", metalDefOfContent));

        // Generate Mineable metals
        if (mineableMetals.Any() && mineableTemplate != null && thingDefOfMineableTemplate != null)
        {
            var mineableOutputDir = FileSystem.Path.Combine(resourcesModDir, "Defs", "Thing", "Metal", "Mineable");
            foreach (var metal in mineableMetals)
            {
                var content = mineableTemplate(new { metal });
                fileWriteTasks.Add(WriteGeneratedFileAsync(mineableOutputDir, $"{metal.Name.ToDefName()}Mineable.generated.xml", content));
            }

            var mineableDefOfContent = thingDefOfMineableTemplate(new { metals = mineableMetals });
            fileWriteTasks.Add(WriteGeneratedFileAsync(FileSystem.Path.Combine(resourcesModDir, "CosmereCore", "Core"), "ThingDefOf.Metal.Mineable.generated.cs", mineableDefOfContent));
        }

        // Generate Item metals
        var itemOutputDir = FileSystem.Path.Combine(resourcesModDir, "Defs", "Thing", "Metal", "Item");
        foreach (var metal in enabledMetals)
        {
            var content = itemTemplate(new { metal });
            fileWriteTasks.Add(WriteGeneratedFileAsync(itemOutputDir, $"{metal.Name.ToDefName()}Item.generated.xml", content));
        }

        var itemDefOfContent = thingDefOfItemTemplate(new { metals = enabledMetals });
        fileWriteTasks.Add(WriteGeneratedFileAsync(FileSystem.Path.Combine(resourcesModDir, "CosmereCore", "Core"), "ThingDefOf.Metal.Items.generated.cs", itemDefOfContent));
        
        // Wait for all file writes to complete
        await Task.WhenAll(fileWriteTasks);
    }

    private async Task GenerateGemsAsync()
    {
        var gems = await _dataLoader.LoadAllAsync<GemInfo>("Gems");
        var enabledGems = gems.Where(g => !IsDisabled(g)).ToList();
        var mineableGems = enabledGems.Where(g => g.Mining != null).ToList();
        
        var templatesDir = FileSystem.Path.Combine("Resources", "Templates", "Resources", "gems");
        var resourcesModDir = FileSystem.Path.Combine("CosmereCore");
        
        // Compile all templates in parallel
        var templateTasks = new List<Task<HandlebarsTemplate<object, object>>>
        {
            CompileTemplateAsync(templatesDir, "GemDef.xml.template"),
            CompileTemplateAsync(templatesDir, "GemDefOf.cs.template"),
            CompileTemplateAsync(templatesDir, "ItemGemDef.xml.template"),
            CompileTemplateAsync(templatesDir, "ThingDefOf.Gems.Item.cs.template")
        };
        
        if (mineableGems.Any())
        {
            templateTasks.Add(CompileTemplateAsync(templatesDir, "MineableGemDef.xml.template"));
            templateTasks.Add(CompileTemplateAsync(templatesDir, "ThingDefOf.Gems.Mineable.cs.template"));
        }
        
        await Task.WhenAll(templateTasks);
        
        var gemDefTemplate = templateTasks[0].Result;
        var gemDefOfTemplate = templateTasks[1].Result;
        var itemTemplate = templateTasks[2].Result;
        var thingDefOfItemTemplate = templateTasks[3].Result;
        
        HandlebarsTemplate<object, object>? mineableTemplate = null;
        HandlebarsTemplate<object, object>? thingDefOfMineableTemplate = null;
        if (mineableGems.Any())
        {
            mineableTemplate = templateTasks[4].Result;
            thingDefOfMineableTemplate = templateTasks[5].Result;
        }
        
        // Generate all files in parallel
        var fileWriteTasks = new List<Task>();
        
        // Generate GemDefs
        var gemDefOutputDir = FileSystem.Path.Combine(resourcesModDir, "Defs", "Gem");
        foreach (var gem in enabledGems)
        {
            var content = gemDefTemplate(new { gem });
            fileWriteTasks.Add(WriteGeneratedFileAsync(gemDefOutputDir, $"{gem.Name.ToDefName()}Gem.generated.xml", content));
        }

        // Generate GemDefOf
        var gemDefOfContent = gemDefOfTemplate(new { gems = enabledGems });
        fileWriteTasks.Add(WriteGeneratedFileAsync(FileSystem.Path.Combine(resourcesModDir, "CosmereCore", "Core"), "GemDefOf.generated.cs", gemDefOfContent));

        // Generate Mineable gems
        if (mineableGems.Any() && mineableTemplate != null && thingDefOfMineableTemplate != null)
        {
            var mineableOutputDir = FileSystem.Path.Combine(resourcesModDir, "Defs", "Thing", "Gem", "Mineable");
            foreach (var gem in mineableGems)
            {
                var content = mineableTemplate(new { gem });
                fileWriteTasks.Add(WriteGeneratedFileAsync(mineableOutputDir, $"{gem.Name.ToDefName()}Mineable.generated.xml", content));
            }

            var mineableDefOfContent = thingDefOfMineableTemplate(new { gems = mineableGems });
            fileWriteTasks.Add(WriteGeneratedFileAsync(FileSystem.Path.Combine(resourcesModDir, "CosmereCore", "Core"), "ThingDefOf.Gems.Mineable.generated.cs", mineableDefOfContent));
        }

        // Generate Item gems
        var itemOutputDir = FileSystem.Path.Combine(resourcesModDir, "Defs", "Thing", "Gem", "Item");
        foreach (var gem in enabledGems)
        {
            var content = itemTemplate(new { gem });
            fileWriteTasks.Add(WriteGeneratedFileAsync(itemOutputDir, $"{gem.Name.ToDefName()}Item.generated.xml", content));
        }

        var itemDefOfContent = thingDefOfItemTemplate(new { gems = enabledGems });
        fileWriteTasks.Add(WriteGeneratedFileAsync(FileSystem.Path.Combine(resourcesModDir, "CosmereCore", "Core"), "ThingDefOf.Gems.Item.generated.cs", itemDefOfContent));
        
        // Wait for all file writes to complete
        await Task.WhenAll(fileWriteTasks);
    }

    private static bool IsDisabled(dynamic item)
    {
        return item.GetType().GetProperty("Disabled")?.GetValue(item) as bool? ?? false;
    }

}