using System.IO.Abstractions;

namespace Cosmere.Tools.Generation.Generators;

/// <summary>
///     Writes the BetaHub project token into a gitignored generated file. The repo is public,
///     so the token reaches source only through this generator and never through a commit.
/// </summary>
public class BetaHubTokenGenerator : BaseGenerator {
    private const string EnvVariable = "BETA_HUB_IN_GAME_KEY";

    public BetaHubTokenGenerator(GeneratorOptions options, IFileSystem fileSystem)
        : base(options, fileSystem) { }

    public override async Task GenerateAsync() {
        var token = Environment.GetEnvironmentVariable(EnvVariable) ?? string.Empty;
        token = token.Trim();

        if (token.Length == 0) {
            Console.WriteLine($"{EnvVariable} is unset. Emitting an empty token, feedback UI will stay hidden.");
        } else if (!token.StartsWith("tkn-", StringComparison.Ordinal)) {
            throw new InvalidOperationException(
                $"{EnvVariable} must be a project auth token starting with 'tkn-'. A 'pat-' token must never ship in the mod."
            );
        }

        var outputDir = FileSystem.Path.Combine("CosmereCore", "CosmereCore", "Core", "BetaHub");
        var content = $$"""
                        namespace Cosmere.Core.BetaHub;

                        internal static class BetaHubToken {
                            public const string Value = "{{token}}";
                        }

                        """;

        await WriteGeneratedFileAsync(outputDir, "BetaHubToken.generated.cs", content);
    }
}
