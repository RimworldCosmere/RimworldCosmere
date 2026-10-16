using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     A Concord injection only applies if its declaring class carries [Patch]. Without it the
///     file still compiles, the injection is simply never woven in, and nothing complains at
///     load - the patched behaviour just silently does not happen.
/// </summary>
/// <remarks>
///     This cost a real regression: PreSelectShardForScenarioPatch lost its [Patch] during a
///     rewrite, so no scenario's Shards were enabled at all, and the build stayed green.
/// </remarks>
[TestClass]
public partial class ConcordPatchAttributeTests {
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

    [GeneratedRegex(@"\[\s*Inject(Method|Field|Property|Instance|New)?\s*[\(\]]")]
    private static partial Regex InjectionAttribute { get; }

    [GeneratedRegex(@"\[\s*Patch\s*[\(\]]")]
    private static partial Regex PatchAttribute { get; }

    [TestMethod]
    public void EveryInjectionLivesInAPatchedClass() {
        string coreDir = Path.Combine(RepoRoot, "CosmereCore", "CosmereCore");
        Assert.IsTrue(Directory.Exists(coreDir), $"Expected source at {coreDir}");

        List<string> offenders = [];
        int seen = 0;

        foreach (string path in Directory.GetFiles(coreDir, "*.cs", SearchOption.AllDirectories)) {
            string source = File.ReadAllText(path);
            if (!InjectionAttribute.IsMatch(source)) continue;

            seen++;
            if (!PatchAttribute.IsMatch(source)) offenders.Add(Path.GetRelativePath(coreDir, path));
        }

        Assert.IsGreaterThan(0, seen, "Found no Concord injections at all - the walk is wrong, not the code.");

        string detail = "These files declare a Concord injection but no [Patch], so the injection " +
            "is never applied and the behaviour silently does not happen: " + string.Join(", ", offenders);

        Assert.IsEmpty(offenders, detail);
    }
}
