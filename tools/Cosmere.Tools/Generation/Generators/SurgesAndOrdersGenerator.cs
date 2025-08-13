using System.IO.Abstractions;
using Cosmere.Tools.Data;
using Cosmere.Tools.Extensions;
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
        
        var templatesDir = FileSystem.Path.Combine("Resources", "Templates", "SurgesAndOrders");
        var rosharModDir = FileSystem.Path.Combine("CosmereRoshar");
        
        // Compile all templates in parallel
        var surgeDefTemplateTask = CompileTemplateAsync(templatesDir, "SurgeDef.xml.template");
        var surgeDefOfTemplateTask = CompileTemplateAsync(templatesDir, "SurgeDefOf.cs.template");
        var radiantOrderDefTemplateTask = CompileTemplateAsync(templatesDir, "RadiantOrderDef.xml.template");
        var radiantOrderDefOfTemplateTask = CompileTemplateAsync(templatesDir, "RadiantOrderDefOf.cs.template");
        var geneDefTemplateTask = CompileTemplateAsync(templatesDir, "GeneDef.xml.template");
        var geneDefOfTemplateTask = CompileTemplateAsync(templatesDir, "GeneDefOf.cs.template");
        var traitDefOfTemplateTask = CompileTemplateAsync(templatesDir, "TraitDefOf.cs.template");
        
        await Task.WhenAll(
            surgeDefTemplateTask, surgeDefOfTemplateTask, radiantOrderDefTemplateTask,
            radiantOrderDefOfTemplateTask, geneDefTemplateTask, geneDefOfTemplateTask, traitDefOfTemplateTask
        );
        
        var surgeDefTemplate = surgeDefTemplateTask.Result;
        var surgeDefOfTemplate = surgeDefOfTemplateTask.Result;
        var radiantOrderDefTemplate = radiantOrderDefTemplateTask.Result;
        var radiantOrderDefOfTemplate = radiantOrderDefOfTemplateTask.Result;
        var geneDefTemplate = geneDefTemplateTask.Result;
        var geneDefOfTemplate = geneDefOfTemplateTask.Result;
        var traitDefOfTemplate = traitDefOfTemplateTask.Result;
        
        // Generate all files in parallel
        var fileWriteTasks = new List<Task>();
        
        // Generate Surge definitions
        var surgeDefOutputDir = FileSystem.Path.Combine(rosharModDir, "Defs", "Surges");
        foreach (var surge in surges)
        {
            var content = surgeDefTemplate(new { surge });
            fileWriteTasks.Add(WriteGeneratedFileAsync(surgeDefOutputDir, $"{surge.Name.ToDefName()}Surge.generated.xml", content));
        }

        // Generate SurgeDefOf
        var surgeDefOfContent = surgeDefOfTemplate(new { surges });
        fileWriteTasks.Add(WriteGeneratedFileAsync(FileSystem.Path.Combine(rosharModDir, "CosmereRoshar"), "SurgeDefOf.generated.cs", surgeDefOfContent));

        // Generate Radiant Order definitions
        var radiantOrderDefOutputDir = FileSystem.Path.Combine(rosharModDir, "Defs", "RadiantOrders");
        foreach (var order in orders)
        {
            var content = radiantOrderDefTemplate(new { order });
            fileWriteTasks.Add(WriteGeneratedFileAsync(radiantOrderDefOutputDir, $"{order.Name.ToDefName()}Order.generated.xml", content));
        }

        // Generate RadiantOrderDefOf
        var radiantOrderDefOfContent = radiantOrderDefOfTemplate(new { orders });
        fileWriteTasks.Add(WriteGeneratedFileAsync(FileSystem.Path.Combine(rosharModDir, "CosmereRoshar"), "RadiantOrderDefOf.generated.cs", radiantOrderDefOfContent));

        // Generate Gene definitions for Radiant Orders
        var geneDefOutputDir = FileSystem.Path.Combine(rosharModDir, "Defs", "Genes");
        foreach (var order in orders)
        {
            var content = geneDefTemplate(new { order });
            fileWriteTasks.Add(WriteGeneratedFileAsync(geneDefOutputDir, $"{order.Name.ToDefName()}Gene.generated.xml", content));
        }

        // Generate DefOf classes for genes and traits
        var geneDefOfContent = geneDefOfTemplate(new { orders });
        fileWriteTasks.Add(WriteGeneratedFileAsync(FileSystem.Path.Combine(rosharModDir, "CosmereRoshar"), "GeneDefOf.RadiantOrders.generated.cs", geneDefOfContent));

        var traitDefOfContent = traitDefOfTemplate(new { orders });
        fileWriteTasks.Add(WriteGeneratedFileAsync(FileSystem.Path.Combine(rosharModDir, "CosmereRoshar"), "TraitDefOf.RadiantOrders.generated.cs", traitDefOfContent));
        
        // Wait for all file writes to complete
        await Task.WhenAll(fileWriteTasks);
    }

}