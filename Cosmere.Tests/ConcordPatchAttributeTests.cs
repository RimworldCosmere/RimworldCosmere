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
public class ConcordPatchAttributeTests {
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

    [TestMethod]
    public void EveryInjectionLivesInAPatchedClass() {
        string coreDir = Path.Combine(RepoRoot, "CosmereCore", "CosmereCore");
        Assert.IsTrue(Directory.Exists(coreDir), $"Expected source at {coreDir}");

        Regex injection = new Regex(@"\[\s*Inject(Method|Field|Property|Instance|New)?\s*[\(\]]");
        Regex patched = new Regex(@"\[\s*Patch\s*[\(\]]");

        List<string> offenders = [];
        int seen = 0;

        foreach (string path in Directory.GetFiles(coreDir, "*.cs", SearchOption.AllDirectories)) {
            string source = File.ReadAllText(path);
            if (!injection.IsMatch(source)) continue;

            seen++;
            if (!patched.IsMatch(source)) offenders.Add(Path.GetRelativePath(coreDir, path));
        }

        Assert.IsTrue(seen > 0, "Found no Concord injections at all - the walk is wrong, not the code.");

        string detail = "These files declare a Concord injection but no [Patch], so the injection " +
            "is never applied and the behaviour silently does not happen: " + string.Join(", ", offenders);

        Assert.AreEqual(0, offenders.Count, detail);
    }
}
