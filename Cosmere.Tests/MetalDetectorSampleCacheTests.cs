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
public class MetalDetectorSampleCacheTests {
    private static string Source() {
        DirectoryInfo? dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
        while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "CosmereCore", "CosmereCore"))) dir = dir.Parent;
        Assert.IsNotNull(dir);
        return File.ReadAllText(Path.Combine(dir.FullName, "CosmereCore", "CosmereCore", "Core", "Util", "MetalDetector.cs"));
    }

    [TestMethod]
    public void SamplesAreCachedByDefAndSeededAgainstCycles() {
        string source = Source();
        StringAssert.Contains(source, "SampleMassCache");
        StringAssert.Contains(source, "if (!SamplesInProgress.Add(key)) return 0f;");

        // every sample goes through the cached helper; only the helper may build one
        Assert.AreEqual(1, Regex.Matches(source, @"MakeSample\(thingDef\)").Count);
        Assert.IsFalse(source.Contains("GetMetalMass(MakeSample("));
    }
}
