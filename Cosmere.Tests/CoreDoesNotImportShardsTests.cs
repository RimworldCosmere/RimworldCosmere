using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     Guards the layering rule in .agents/rules/architecture.md. KandraMapLabelPatch broke it
///     for months without anything noticing.
/// </summary>
[TestClass]
public class CoreDoesNotImportShardsTests {
    private static string RepoRoot {
        get {
            DirectoryInfo? dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "CosmereScadrial", "Defs"))) {
                dir = dir.Parent;
            }

            Assert.IsNotNull(dir, "Could not locate CosmereScadrial/Defs above the test output directory.");
            return dir.FullName;
        }
    }

    private static string CoreDir => Path.Combine(RepoRoot, "CosmereCore", "CosmereCore", "Core");

    private static string SystemDir => Path.Combine(RepoRoot, "CosmereCore", "CosmereCore", "System");

    [TestMethod]
    public void NoFileUnderCoreImportsAShard() {
        List<string> offenders = [];

        foreach (string file in Directory.EnumerateFiles(CoreDir, "*.cs", SearchOption.AllDirectories)) {
            foreach (string line in File.ReadAllLines(file)) {
                if (!line.TrimStart().StartsWith("using Cosmere.System.", StringComparison.Ordinal)) continue;

                offenders.Add($"{Path.GetRelativePath(RepoRoot, file)}: {line.Trim()}");
            }
        }

        Assert.AreEqual(
            0,
            offenders.Count,
            "Core imports a shard, which reverses the layering:\n" + string.Join("\n", offenders) +
            "\nMove the file under System/<Shard>/ or route it through a Core registry."
        );
    }

    [TestMethod]
    public void NoShardImportsAnotherShard() {
        List<string> offenders = [];

        foreach (string shard in new[] { "Roshar", "Scadrial", "Nalthis" }) {
            string dir = Path.Combine(SystemDir, shard);
            if (!Directory.Exists(dir)) continue;

            foreach (string file in Directory.EnumerateFiles(dir, "*.cs", SearchOption.AllDirectories)) {
                foreach (string line in File.ReadAllLines(file)) {
                    string trimmed = line.TrimStart();
                    if (!trimmed.StartsWith("using Cosmere.System.", StringComparison.Ordinal)) continue;
                    if (trimmed.StartsWith($"using Cosmere.System.{shard}", StringComparison.Ordinal)) continue;

                    // the Extension global usings in Cosmere.csproj are the documented exception
                    if (trimmed.Contains(".Extension", StringComparison.Ordinal)) continue;

                    offenders.Add($"{Path.GetRelativePath(RepoRoot, file)}: {trimmed}");
                }
            }
        }

        Assert.AreEqual(
            0,
            offenders.Count,
            "A shard imports another shard. Cross-shard work goes through a Core registry:\n" +
            string.Join("\n", offenders)
        );
    }
}
