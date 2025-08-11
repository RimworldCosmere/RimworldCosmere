using System.CommandLine;
using System.CommandLine.Parsing;
using System.IO.Abstractions;
using Cosmere.Tools.Generation;
using Spectre.Console;

namespace Cosmere.Tools.Cli.Commands;

public static class GenerateCommand
{
    public static Command Create()
    {
        var forceOption = new Option<bool>("--force")
        {
            Description = "Force generation of templates. Will ignore a lack of changes"
        };

        var dryRunOption = new Option<bool>("--dry-run")
        {
            Description = "Dry run for generation of templates"
        };

        var deleteOption = new Option<bool>("--delete")
        {
            Description = "Delete all generated files before starting"
        };

        var verboseOption = new Option<bool>("--verbose")
        {
            Description = "Extra logs"
        };

        var generatorArgument = new Argument<string?>("generator")
        {
            Description = "Specific generator to run (optional - runs all if not specified)",
            Arity = ArgumentArity.ZeroOrOne
        };

        var command = new Command("generate", "Run code generators");
        command.Options.Add(forceOption);
        command.Options.Add(dryRunOption);
        command.Options.Add(deleteOption);
        command.Options.Add(verboseOption);
        command.Arguments.Add(generatorArgument);

        command.SetAction((ParseResult parseResult) =>
        {
            var force = parseResult.GetValue(forceOption);
            var dryRun = parseResult.GetValue(dryRunOption);
            var delete = parseResult.GetValue(deleteOption);
            var verbose = parseResult.GetValue(verboseOption);
            var generator = parseResult.GetValue(generatorArgument);
            var options = new GeneratorOptions
            {
                Force = force,
                DryRun = dryRun,
                Delete = delete,
                Verbose = verbose
            };

            var fileSystem = new FileSystem();
            var registry = new GeneratorRegistry(options, fileSystem);

            try
            {
                if (delete)
                {
                    DeleteGeneratedFilesAsync(fileSystem, verbose, dryRun).Wait();
                }

                if (string.IsNullOrEmpty(generator))
                {
                    AnsiConsole.MarkupLine("[green]Running all generators...[/]");
                    registry.RunAllAsync().Wait();
                }
                else
                {
                    var gen = registry.GetGenerator(generator);
                    if (gen == null)
                    {
                        AnsiConsole.MarkupLine($"[red]Generator '{generator}' not found.[/]");
                        AnsiConsole.MarkupLine("[yellow]Available generators:[/]");
                        foreach (var name in registry.GetGeneratorNames())
                        {
                            AnsiConsole.MarkupLine($"  - {name}");
                        }
                        return 1;
                    }

                    AnsiConsole.MarkupLine($"[green]Running {generator} generator...[/]");
                    gen.GenerateAsync().Wait();
                }

                AnsiConsole.MarkupLine("[green]Generation complete![/]");
                return 0;
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Error during generation: {ex.Message}[/]");
                return 1;
            }
        });

        return command;
    }

    private static async Task DeleteGeneratedFilesAsync(IFileSystem fileSystem, bool verbose, bool dryRun)
    {
        var directories = new[]
        {
            "CosmereCore",
            "CosmereFoundation", 
            "CosmereResources",
            "CosmereScadrial",
            "CosmereRoshar"
        };

        foreach (var dir in directories)
        {
            if (fileSystem.Directory.Exists(dir))
            {
                await DeleteGeneratedFilesRecursiveAsync(fileSystem, dir, verbose, dryRun);
            }
        }
    }

    private static async Task DeleteGeneratedFilesRecursiveAsync(IFileSystem fileSystem, string directory, bool verbose, bool dryRun)
    {
        var entries = fileSystem.Directory.GetFileSystemEntries(directory);

        foreach (var entry in entries)
        {
            if (fileSystem.Directory.Exists(entry))
            {
                await DeleteGeneratedFilesRecursiveAsync(fileSystem, entry, verbose, dryRun);
            }
            else if (fileSystem.File.Exists(entry))
            {
                var fileName = fileSystem.Path.GetFileName(entry);
                if (fileName.EndsWith(".generated.cs", StringComparison.OrdinalIgnoreCase) ||
                    fileName.EndsWith(".generated.xml", StringComparison.OrdinalIgnoreCase))
                {
                    if (verbose)
                    {
                        var relativePath = fileSystem.Path.GetRelativePath(Environment.CurrentDirectory, entry);
                        Console.WriteLine($"{(dryRun ? "[DRY-RUN] " : "")}Deleted: {relativePath}");
                    }

                    if (!dryRun)
                    {
                        fileSystem.File.Delete(entry);
                    }
                }
            }
        }
    }
}