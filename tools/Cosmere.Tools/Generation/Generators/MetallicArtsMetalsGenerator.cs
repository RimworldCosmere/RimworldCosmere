using System.IO.Abstractions;
using Cosmere.Tools.Data;
using Cosmere.Tools.Extensions;
using Cosmere.Tools.Models;

namespace Cosmere.Tools.Generation.Generators;

public class MetallicArtsMetalsGenerator : BaseGenerator
{
    private readonly DataLoader _dataLoader;
    
    public MetallicArtsMetalsGenerator(GeneratorOptions options, IFileSystem fileSystem) : base(options, fileSystem)
    {
        _dataLoader = new DataLoader(fileSystem);
    }

    public override async Task GenerateAsync()
    {
        var metals = await _dataLoader.LoadAllAsync<MetalInfo>("Metals");
        var metallicArtsMetals = metals.Where(m => m.Allomancy != null || m.Feruchemy != null).ToList();
        
        var templatesDir = FileSystem.Path.Combine("Resources", "Templates", "MetallicArtsMetals");
        var scadrialModDir = FileSystem.Path.Combine("CosmereScadrial");
        
        // Compile all templates in parallel
        var metalTemplateTask = CompileTemplateAsync(templatesDir, "MetallicArtsMetalDef.xml.template");
        var patchTemplateTask = CompileTemplateAsync(templatesDir, "PatchScadrialMetalDef.xml.template");
        var metalDefOfTemplateTask = CompileTemplateAsync(templatesDir, "MetallicArtsMetalDefOf.cs.template");
        var recordsTemplateTask = CompileTemplateAsync(templatesDir, "Records.xml.template");
        var recordDefOfTemplateTask = CompileTemplateAsync(templatesDir, "RecordDefOf.cs.template");
        
        await Task.WhenAll(metalTemplateTask, patchTemplateTask, metalDefOfTemplateTask, recordsTemplateTask, recordDefOfTemplateTask);
        
        var metalTemplate = metalTemplateTask.Result;
        var patchTemplate = patchTemplateTask.Result;
        var metalDefOfTemplate = metalDefOfTemplateTask.Result;
        var recordsTemplate = recordsTemplateTask.Result;
        var recordDefOfTemplate = recordDefOfTemplateTask.Result;
        
        // Generate all files in parallel
        var fileWriteTasks = new List<Task>();
        
        // Generate metallic arts metal definitions
        var metalOutputDir = FileSystem.Path.Combine(scadrialModDir, "Defs", "Things", "Metals");
        foreach (var metal in metallicArtsMetals)
        {
            var content = metalTemplate(new { metal });
            fileWriteTasks.Add(WriteGeneratedFileAsync(metalOutputDir, $"{metal.Name.ToDefName()}MetallicArtsMetal.generated.xml", content));
        }

        // Generate patches
        var patchOutputDir = FileSystem.Path.Combine(scadrialModDir, "Patches", "Metals");
        foreach (var metal in metallicArtsMetals)
        {
            var content = patchTemplate(new { metal });
            fileWriteTasks.Add(WriteGeneratedFileAsync(patchOutputDir, $"{metal.Name.ToDefName()}Patch.generated.xml", content));
        }

        // Generate DefOf classes
        var metalDefOfContent = metalDefOfTemplate(new { metals = metallicArtsMetals });
        fileWriteTasks.Add(WriteGeneratedFileAsync(FileSystem.Path.Combine(scadrialModDir, "CosmereScadrial"), "MetallicArtsMetalDefOf.generated.cs", metalDefOfContent));

        // Generate Records
        var recordsContent = recordsTemplate(new { metals = metallicArtsMetals });
        fileWriteTasks.Add(WriteGeneratedFileAsync(FileSystem.Path.Combine(scadrialModDir, "Defs"), "Records.generated.xml", recordsContent));

        var recordDefOfContent = recordDefOfTemplate(new { metals = metallicArtsMetals });
        fileWriteTasks.Add(WriteGeneratedFileAsync(FileSystem.Path.Combine(scadrialModDir, "CosmereScadrial"), "RecordDefOf.generated.cs", recordDefOfContent));
        
        // Wait for all file writes to complete
        await Task.WhenAll(fileWriteTasks);
    }

}