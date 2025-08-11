using System.IO.Abstractions;
using Cosmere.Tools.Data;
using Cosmere.Tools.Models;

namespace Cosmere.Tools.Generation.Generators;

public class FeruchemicalHediffsGenerator : BaseGenerator
{
    private readonly DataLoader _dataLoader;
    
    public FeruchemicalHediffsGenerator(GeneratorOptions options, IFileSystem fileSystem) : base(options, fileSystem)
    {
        _dataLoader = new DataLoader(fileSystem);
    }

    public override async Task GenerateAsync()
    {
        var metals = await _dataLoader.LoadAllAsync<MetalInfo>("Metals");
        var feruchemicalMetals = metals.Where(m => m.Feruchemy?.UserName != null).ToList();
        
        var templatesDir = FileSystem.Path.Combine(".scripts", "Generators", "FeruchemicalHediffs");
        var scadrialModDir = FileSystem.Path.Combine("CosmereScadrial");
        
        // Generate individual hediff definitions
        var defTemplate = CompileTemplate(templatesDir, "HediffDef.xml.template");
        
        foreach (var metal in feruchemicalMetals)
        {
            var outputDir = FileSystem.Path.Combine(scadrialModDir, "Defs", "Feruchemy", ToDefName(metal.Name));
            var content = defTemplate(new { metal });
            WriteGeneratedFile(outputDir, "Hediff.generated.xml", content);
        }

        // Generate HediffDefOf class
        var defOfTemplate = CompileTemplate(templatesDir, "HediffDefOf.cs.template");
        var defOfContent = defOfTemplate(new { metals = feruchemicalMetals });
        WriteGeneratedFile(FileSystem.Path.Combine(scadrialModDir, "CosmereScadrial"), "HediffDefOf.Feruchemy.generated.cs", defOfContent);
    }

    private static string ToDefName(string name)
    {
        return name.Replace(" ", string.Empty);
    }
}