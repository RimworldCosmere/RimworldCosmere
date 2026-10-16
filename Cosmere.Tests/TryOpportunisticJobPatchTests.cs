using System;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     TryOpportunisticJob's insert site sits inside the foreach's try/finally. A bare `ret` there is
///     invalid IL on CoreCLR, so the transpiler has to reuse vanilla's `stloc; leave` return spill.
/// </summary>
[TestClass]
public partial class TryOpportunisticJobPatchTests {
    private static string PatchSource {
        get {
            DirectoryInfo? dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "CosmereCore", "CosmereCore"))) {
                dir = dir.Parent;
            }

            Assert.IsNotNull(dir, "Could not locate CosmereCore above the test output directory.");
            return File.ReadAllText(
                Path.Combine(dir.FullName, "CosmereCore", "CosmereCore", "Core", "Patch", "InnerStorage", "TryOpportunisticJobPatch.cs")
            );
        }
    }

    [TestMethod]
    public void NeverEmitsRetInsideTheForeach() {
        string source = PatchSource;
        Assert.DoesNotMatchRegex(
            RetOpCodePattern(),
            source,
            "A ret inside the foreach's try/finally is invalid IL. Copy vanilla's stloc + leave spill instead."
        );
        Assert.Contains("HaulToCellStorageJob", source, "The spill is located off vanilla's HaulToCellStorageJob call.");
    }

    [GeneratedRegex(@"OpCodes\.Ret\b")]
    private static partial Regex RetOpCodePattern();
}
