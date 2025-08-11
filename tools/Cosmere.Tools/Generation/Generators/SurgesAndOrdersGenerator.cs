using System.IO.Abstractions;
using Cosmere.Tools.Data;
using Cosmere.Tools.Models;

namespace Cosmere.Tools.Generation.Generators;

public class SurgesAndOrdersGenerator : BaseGenerator
{
    private readonly DataLoader _dataLoader;
    
    public SurgesAndOrdersGenerator(GeneratorOptions options, IFileSystem fileSystem) : base(options, fileSystem)
    {
        _dataLoader = new DataLoader(fileSystem);
    }

    public override async Task GenerateAsync()
    {
        var surges = await _dataLoader.LoadAllAsync<SurgeInfo>("Surges");
        var orders = await _dataLoader.LoadAllAsync<RadiantOrderInfo>("RadiantOrders");
        
        var templatesDir = FileSystem.Path.Combine(".scripts", "Generators", "SurgesAndOrders");
        var rosharModDir = FileSystem.Path.Combine("CosmereRoshar");
        
        // Generate Surge definitions
        var surgeDefTemplate = CompileTemplate(templatesDir, "SurgeDef.xml.template");
        var surgeDefOutputDir = FileSystem.Path.Combine(rosharModDir, "Defs", "Surges");
        
        foreach (var surge in surges)
        {
            var content = surgeDefTemplate(new { surge });
            WriteGeneratedFile(surgeDefOutputDir, $"{ToDefName(surge.Name)}.generated.xml", content);
        }

        // Generate SurgeDefOf
        var surgeDefOfTemplate = CompileTemplate(templatesDir, "SurgeDefOf.cs.template");
        var surgeDefOfContent = surgeDefOfTemplate(new { surges });
        WriteGeneratedFile(FileSystem.Path.Combine(rosharModDir, "CosmereRoshar"), "SurgeDefOf.generated.cs", surgeDefOfContent);

        // Generate Radiant Order definitions
        var radiantOrderDefTemplate = CompileTemplate(templatesDir, "RadiantOrderDef.xml.template");
        var radiantOrderDefOutputDir = FileSystem.Path.Combine(rosharModDir, "Defs", "RadiantOrders");
        
        foreach (var order in orders)
        {
            var content = radiantOrderDefTemplate(new { order });
            WriteGeneratedFile(radiantOrderDefOutputDir, $"{ToDefName(order.Name)}.generated.xml", content);
        }

        // Generate RadiantOrderDefOf
        var radiantOrderDefOfTemplate = CompileTemplate(templatesDir, "RadiantOrderDefOf.cs.template");
        var radiantOrderDefOfContent = radiantOrderDefOfTemplate(new { orders });
        WriteGeneratedFile(FileSystem.Path.Combine(rosharModDir, "CosmereRoshar"), "RadiantOrderDefOf.generated.cs", radiantOrderDefOfContent);

        // Generate Gene definitions for Radiant Orders
        var geneDefTemplate = CompileTemplate(templatesDir, "GeneDef.xml.template");
        var geneDefOutputDir = FileSystem.Path.Combine(rosharModDir, "Defs", "Genes");
        
        foreach (var order in orders)
        {
            var content = geneDefTemplate(new { order });
            WriteGeneratedFile(geneDefOutputDir, $"{ToDefName(order.Name)}.generated.xml", content);
        }

        // Generate DefOf classes for genes and traits
        var geneDefOfTemplate = CompileTemplate(templatesDir, "GeneDefOf.cs.template");
        var geneDefOfContent = geneDefOfTemplate(new { orders });
        WriteGeneratedFile(FileSystem.Path.Combine(rosharModDir, "CosmereRoshar"), "GeneDefOf.RadiantOrders.generated.cs", geneDefOfContent);

        var traitDefOfTemplate = CompileTemplate(templatesDir, "TraitDefOf.cs.template");
        var traitDefOfContent = traitDefOfTemplate(new { orders });
        WriteGeneratedFile(FileSystem.Path.Combine(rosharModDir, "CosmereRoshar"), "TraitDefOf.RadiantOrders.generated.cs", traitDefOfContent);
    }

    private static string ToDefName(string name)
    {
        return name.Replace(" ", string.Empty);
    }
}