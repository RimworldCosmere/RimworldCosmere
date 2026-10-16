using System.CommandLine;
using System.CommandLine.Parsing;
using System.IO.Abstractions;
using Cosmere.Tools.Generation;

namespace Cosmere.Tools.Cli.Commands;

public static class GenerateCommand {
    public static Command Create() {
        var forceOption = new Option<bool>("--force") {
            Description = "Force generation of templates. Will ignore a lack of changes",
        };

        var dryRunOption = new Option<bool>("--dry-run") {
            Description = "Dry run for generation of templates",
        };

        var deleteOption = new Option<bool>("--delete") {
            Description = "Delete all generated files before starting",
        };

        var noCleanOption = new Option<bool>("--no-clean") {
            Description = "Skip deleting generated files before generation (files are deleted by default)",
        };

        var verboseOption = new Option<bool>("--verbose") {
            Description = "Extra logs",
        };

        var cleanOption = new Option<bool>("--clean") {
            Description = "Delete all generated files and exit (no generation)",
        };

        var generatorArgument = new Argument<string?>("generator") {
            Description = "Specific generator to run (optional - runs all if not specified)",
            Arity = ArgumentArity.ZeroOrOne,
        };

        var command = new Command("generate", "Run code generators");
        command.Options.Add(forceOption);
        command.Options.Add(dryRunOption);
        command.Options.Add(deleteOption);
        command.Options.Add(noCleanOption);
        command.Options.Add(verboseOption);
        command.Options.Add(cleanOption);
        command.Arguments.Add(generatorArgument);

        command.SetAction((ParseResult parseResult) => {
            var force = parseResult.GetValue(forceOption);
            var dryRun = parseResult.GetValue(dryRunOption);
            var delete = parseResult.GetValue(deleteOption);
            var noClean = parseResult.GetValue(noCleanOption);
            var verbose = parseResult.GetValue(verboseOption);
            var clean = parseResult.GetValue(cleanOption);
            var generator = parseResult.GetValue(generatorArgument);

            // By default, always clean before generating unless --no-clean is specified
            var shouldClean = !noClean || delete;
            var options = new GeneratorOptions {
                Force = force,
                DryRun = dryRun,
                Delete = delete,
                Verbose = verbose,
            };

            var fileSystem = new FileSystem();
            var registry = new GeneratorRegistry(options, fileSystem);

            try {
                if (clean) {
                    DeleteGeneratedFilesAsync(fileSystem, verbose, dryRun).Wait();
                    Console.WriteLine("Clean complete!");
                    return 0;
                }

                // Always clean before generating unless --no-clean is specified
                if (shouldClean) {
                    if (verbose) {
                        Console.WriteLine("Cleaning existing generated files...");
                    }

                    DeleteGeneratedFilesAsync(fileSystem, verbose, dryRun).Wait();
                }

                if (string.IsNullOrEmpty(generator)) {
                    Console.WriteLine("Running all generators...");
                    registry.RunAllAsync().Wait();
                } else {
                    var gen = registry.GetGenerator(generator);
                    if (gen == null) {
                        Console.WriteLine($"Generator '{generator}' not found.");
                        Console.WriteLine("Available generators:");
                        foreach (var name in registry.GetGeneratorNames()) {
                            Console.WriteLine($"  - {name}");
                        }

                        return 1;
                    }

                    Console.WriteLine($"Running {generator} generator...");
                    gen.GenerateAsync().Wait();
                }

                Console.WriteLine("Generation complete!");
                return 0;
            } catch (Exception ex) {
                Console.WriteLine($"Error during generation: {ex.Message}");
                if (verbose) {
                    Console.WriteLine(ex.ToString());
                }

                return 1;
            }
        });

        return command;
    }

    private static async Task DeleteGeneratedFilesAsync(IFileSystem fileSystem, bool verbose, bool dryRun) {
        var directories = new[]
        {
            "CosmereCore",
            "CosmereScadrial",
            "CosmereRoshar",
        };

        foreach (var dir in directories) {
            if (fileSystem.Directory.Exists(dir)) {
                await DeleteGeneratedFilesRecursiveAsync(fileSystem, dir, verbose, dryRun);
            }
        }
    }

    private static async Task DeleteGeneratedFilesRecursiveAsync(IFileSystem fileSystem, string directory, bool verbose, bool dryRun) {
        var entries = fileSystem.Directory.GetFileSystemEntries(directory);

        foreach (var entry in entries) {
            if (fileSystem.Directory.Exists(entry)) {
                await DeleteGeneratedFilesRecursiveAsync(fileSystem, entry, verbose, dryRun);
            } else if (fileSystem.File.Exists(entry)) {
                var fileName = fileSystem.Path.GetFileName(entry);
                if (fileName.EndsWith(".generated.cs", StringComparison.OrdinalIgnoreCase) ||
                    fileName.EndsWith(".generated.xml", StringComparison.OrdinalIgnoreCase)) {
                    if (verbose) {
                        var relativePath = fileSystem.Path.GetRelativePath(Environment.CurrentDirectory, entry);
                        Console.WriteLine($"{(dryRun ? "[DRY-RUN] " : string.Empty)}Deleted: {relativePath}");
                    }

                    if (!dryRun) {
                        fileSystem.File.Delete(entry);
                    }
                }
            }
        }
    }
}
