using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     Burial hides a thing by never printing it, the way deep snow does. The pieces that make
///     that work are all Verse-bound - a SectionLayer, a Map and a Concord injection - so these
///     read the source instead. Each one guards a failure that looks fine at build time and only
///     shows up as items floating on top of two metres of ash.
/// </summary>
[TestClass]
public class AshBurialRenderTests {
    private static string RepoRoot {
        get {
            DirectoryInfo? dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "CosmereScadrial", "Defs"))) {
                dir = dir.Parent;
            }

            Assert.IsNotNull(dir, "Could not locate CosmereScadrial/Defs above the test output directory.");
            return dir!.FullName;
        }
    }

    private static string PrintPatchSource => Read(
        "CosmereCore", "CosmereCore", "System", "Scadrial", "Patch", "Rendering", "AshBurialPrintPatches.cs"
    );

    private static string TrackerSource => Read(
        "CosmereCore", "CosmereCore", "System", "Scadrial", "Comp", "Map", "AshDepthTracker.cs"
    );

    private static string Read(params string[] parts) {
        string path = Path.Combine(RepoRoot, Path.Combine(parts));
        Assert.IsTrue(File.Exists(path), $"Expected source at {path}");
        return File.ReadAllText(path);
    }

    /// <summary>
    ///     SectionLayer_ThingsGeneral prints each thing into its own submesh, so no overlay above
    ///     it can erase one. Hiding has to cancel the print itself.
    /// </summary>
    [TestMethod]
    public void BurialCancelsThePrintRatherThanDrawingOverIt() {
        string source = PrintPatchSource;

        Assert.IsTrue(
            source.Contains("SectionLayer_ThingsGeneral", StringComparison.Ordinal),
            "The patch has to target the layer that prints things, not the ash wash."
        );
        Assert.IsTrue(
            source.Contains("nameof(TakePrintFrom)", StringComparison.Ordinal),
            "TakePrintFrom is the per-thing hook. Anything else leaves the print in the mesh."
        );
        Assert.IsTrue(
            source.Contains("At.Head", StringComparison.Ordinal) &&
            source.Contains("Control.Cancel", StringComparison.Ordinal),
            "Only a Head injection can cancel, and only a cancel keeps the thing out of the mesh."
        );
    }

    /// <summary>
    ///     A wall or a floor vanishing under a drift reads as a bug rather than as weather, and
    ///     pawns are drawn dynamically anyway.
    /// </summary>
    [TestMethod]
    public void OnlyItemsAndPlantsAreHidden() {
        string source = PrintPatchSource;

        Assert.IsTrue(
            source.Contains("ThingCategory.Item", StringComparison.Ordinal) &&
            source.Contains("ThingCategory.Plant", StringComparison.Ordinal),
            "Burial covers dropped items and plants."
        );
        Assert.IsFalse(
            source.Contains("ThingCategory.Building", StringComparison.Ordinal),
            "Buildings stay drawn. Ash swallowing a wall looks broken, not deep."
        );
    }

    /// <summary>
    ///     Section builds one instance of every non-abstract SectionLayer subclass. Drop the
    ///     abstract and the patch declaration becomes a second things layer on every section,
    ///     which prints the whole map twice.
    /// </summary>
    [TestMethod]
    public void ThePatchDeclarationStaysAbstract() {
        Assert.IsTrue(
            PrintPatchSource.Contains(
                "public abstract class AshBurialPrintPatch : SectionLayer_ThingsGeneral",
                StringComparison.Ordinal
            ),
            "A concrete SectionLayer subclass gets instantiated by Section and doubles every print."
        );
    }

    /// <summary>
    ///     The things mesh rebuilds off its own flag. Dirtying only the ash flag leaves buried
    ///     items drawn until something unrelated happens to touch Things.
    /// </summary>
    [TestMethod]
    public void ABurialFlipDirtiesTheThingsMesh() {
        string source = TrackerSource;

        Assert.IsTrue(
            source.Contains("MapMeshFlagDefOf.Things", StringComparison.Ordinal),
            "Without the Things flag the print patch never gets a chance to run again."
        );
        Assert.IsTrue(
            source.Contains("if (flipped) NotifyAshChanged(true);", StringComparison.Ordinal),
            "The sweep is the only place a cell flips buried, so it is the only place that has to " +
            "ask for a things rebuild."
        );
    }

    /// <summary>
    ///     Ash falls on every stripe of every tick. Passing the Things flag on a plain depth
    ///     change would rebuild the whole map's things mesh forever.
    /// </summary>
    [TestMethod]
    public void PlainDepthChangesLeaveTheThingsMeshAlone() {
        string source = TrackerSource;

        Assert.AreEqual(
            1,
            CountOf(source, "NotifyAshChanged(true)"),
            "Only the burial flip asks for a things rebuild. Every other caller moves depth alone."
        );
    }

    private static int CountOf(string source, string needle) {
        int count = 0;
        int at = source.IndexOf(needle, StringComparison.Ordinal);
        while (at >= 0) {
            count++;
            at = source.IndexOf(needle, at + needle.Length, StringComparison.Ordinal);
        }

        return count;
    }
}
