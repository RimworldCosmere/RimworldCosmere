using System.IO.Abstractions;
using Cosmere.Tools.Data;
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
        
        var templatesDir = FileSystem.Path.Combine(".scripts", "Generators", "MetallicArtsMetals");
        var scadrialModDir = FileSystem.Path.Combine("CosmereScadrial");
        
        // Generate metallic arts metal definitions
        var metalTemplate = CompileTemplate(templatesDir, "MetallicArtsMetalDef.xml.template");
        var metalOutputDir = FileSystem.Path.Combine(scadrialModDir, "Defs", "Things", "Metals");
        
        foreach (var metal in metallicArtsMetals)
        {
            var content = metalTemplate(new { metal });
            WriteGeneratedFile(metalOutputDir, $"{ToDefName(metal.Name)}.generated.xml", content);
        }

        // Generate patches
        var patchTemplate = CompileTemplate(templatesDir, "PatchScadrialMetalDef.xml.template");
        var patchOutputDir = FileSystem.Path.Combine(scadrialModDir, "Patches", "Metals");
        
        foreach (var metal in metallicArtsMetals)
        {
            var content = patchTemplate(new { metal });
            WriteGeneratedFile(patchOutputDir, $"{ToDefName(metal.Name)}.generated.xml", content);
        }

        // Generate DefOf classes
        var metalDefOfTemplate = CompileTemplate(templatesDir, "MetallicArtsMetalDefOf.cs.template");
        var metalDefOfContent = metalDefOfTemplate(new { metals = metallicArtsMetals });
        WriteGeneratedFile(FileSystem.Path.Combine(scadrialModDir, "CosmereScadrial"), "MetallicArtsMetalDefOf.generated.cs", metalDefOfContent);

        // Generate Records
        var recordsTemplate = CompileTemplate(templatesDir, "Records.xml.template");
        var recordsContent = recordsTemplate(new { metals = metallicArtsMetals });
        WriteGeneratedFile(FileSystem.Path.Combine(scadrialModDir, "Defs"), "Records.generated.xml", recordsContent);

        var recordDefOfTemplate = CompileTemplate(templatesDir, "RecordDefOf.cs.template");
        var recordDefOfContent = recordDefOfTemplate(new { metals = metallicArtsMetals });
        WriteGeneratedFile(FileSystem.Path.Combine(scadrialModDir, "CosmereScadrial"), "RecordDefOf.generated.cs", recordDefOfContent);
    }

    private static string ToDefName(string name)
    {
        return name.Replace(" ", string.Empty);
    }
}