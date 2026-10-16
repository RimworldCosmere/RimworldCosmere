using System.IO.Abstractions;
using Cosmere.Tools.Data;
using Cosmere.Tools.Extensions;
using Cosmere.Tools.Models;

namespace Cosmere.Tools.Generation.Generators;

public class FeruchemicalHediffsGenerator : BaseGenerator {
    private readonly DataLoader _dataLoader;

    public FeruchemicalHediffsGenerator(GeneratorOptions options, IFileSystem fileSystem) : base(options, fileSystem) {
        _dataLoader = new DataLoader(fileSystem);
    }

    public override async Task GenerateAsync() {
        var metals = await _dataLoader.LoadAllAsync<MetalInfo>("Metals");
        var feruchemicalMetals = metals.Where(m => m.Feruchemy?.UserName != null).ToList();

        var templatesDir = FileSystem.Path.Combine("Resources", "Templates", "FeruchemicalHediffs");
        var scadrialXmlDir = FileSystem.Path.Combine("CosmereScadrial");
        var scadrialCsDir = FileSystem.Path.Combine("CosmereCore", "CosmereCore", "System", "Scadrial");

        // Compile templates in parallel
        var defTemplateTask = CompileTemplateAsync(templatesDir, "HediffDef.xml.template");
        var defOfTemplateTask = CompileTemplateAsync(templatesDir, "HediffDefOf.cs.template");

        await Task.WhenAll(defTemplateTask, defOfTemplateTask);

        var defTemplate = defTemplateTask.Result;
        var defOfTemplate = defOfTemplateTask.Result;

        // Generate all files in parallel
        var fileWriteTasks = new List<Task>();

        // Generate individual hediff definitions
        foreach (var metal in feruchemicalMetals) {
            var outputDir = FileSystem.Path.Combine(scadrialXmlDir, "Defs", "Feruchemy", metal.Name.ToDefName());
            var content = defTemplate(new { metal });
            fileWriteTasks.Add(WriteGeneratedFileAsync(outputDir, metal.Name.ToDefName() + "Hediff.generated.xml", content));
        }

        // Generate HediffDefOf class
        var defOfContent = defOfTemplate(new { metals = feruchemicalMetals });
        fileWriteTasks.Add(WriteGeneratedFileAsync(scadrialCsDir, "HediffDefOf.Feruchemy.generated.cs", defOfContent));

        // Wait for all file writes to complete
        await Task.WhenAll(fileWriteTasks);
    }
}
