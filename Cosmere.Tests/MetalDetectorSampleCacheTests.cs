using System;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     A recipe walk used to make a fresh sample Thing per ingredient def and cache by that Thing,
///     so nothing was ever a cache hit and an aura tick re-walked the whole recipe graph.
/// </summary>
[TestClass]
public partial class MetalDetectorSampleCacheTests {
    private static string Source() {
        DirectoryInfo? dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
        while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "CosmereCore", "CosmereCore"))) dir = dir.Parent;
        Assert.IsNotNull(dir);
        return File.ReadAllText(Path.Combine(dir.FullName, "CosmereCore", "CosmereCore", "Core", "Util", "MetalDetector.cs"));
    }

    [GeneratedRegex(@"MakeSample\(thingDef\)")]
    private static partial Regex SampleConstruction { get; }

    [TestMethod]
    public void SamplesAreCachedByDefAndSeededAgainstCycles() {
        string source = Source();
        Assert.Contains("SampleMassCache", source);
        Assert.Contains("if (!SamplesInProgress.Add(key)) return 0f;", source);

        // every sample goes through the cached helper; only the helper may build one
        Assert.HasCount(1, SampleConstruction.Matches(source));
        Assert.DoesNotContain("GetMetalMass(MakeSample(", source);
    }
}
