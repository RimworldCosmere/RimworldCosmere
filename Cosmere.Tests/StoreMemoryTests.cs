using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Cosmere.System.Scadrial.Feruchemy.UI;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     Picking a memory to offload is the whole job of the store-memory window, and that choice
///     was carried by a green or red label and nothing else.
/// </summary>
[TestClass]
public class StoreMemoryTests {
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

    private static string Core(params string[] parts) => File.ReadAllText(Path.Combine(
        [RepoRoot, "CosmereCore", "CosmereCore", .. parts]
    ));

    /// <summary>
    ///     Every key under CosmereCore's Keyed folder. Which file holds one is the game's business,
    ///     not this test's - it merges the whole folder.
    /// </summary>
    private static HashSet<string> CoreKeys() {
        HashSet<string> names = [];
        foreach (string file in Directory.GetFiles(
                     Path.Combine(RepoRoot, "CosmereCore", "Languages", "English", "Keyed"), "*.xml"
                 )) {
            foreach (XElement key in XDocument.Load(file).Root!.Elements()) {
                names.Add(key.Name.LocalName);
            }
        }

        return names;
    }

    /// <summary>
    ///     Red and green is the pair most colour-blind players cannot separate, so the sign has to
    ///     be readable on its own.
    /// </summary>
    [TestMethod]
    public void AMagnitudeCarriesItsSignBothWays() {
        Assert.AreEqual("+2.4", MemoryMagnitude.Format(2.4f));
        Assert.AreEqual("-1.8", MemoryMagnitude.Format(-1.8f));
        Assert.AreEqual("0.0", MemoryMagnitude.Format(0f));

        Assert.AreEqual("+12.0", MemoryMagnitude.Format(12f));
        Assert.AreEqual("-0.4", MemoryMagnitude.Format(-0.44f));
    }

    /// <summary>
    ///     Sorting by the raw offset puts the worst memory at the bottom of a thirty-row list, and
    ///     that is the one bug that would never look wrong on screen.
    /// </summary>
    [TestMethod]
    public void TheHeaviestMemoryComesFirstWhicheverWayItPulls() {
        List<float> offsets = [1.2f, -8.5f, 0.3f, 4f, -0.1f];
        offsets.Sort(MemoryMagnitude.CompareByWeight);

        CollectionAssert.AreEqual(new[] { -8.5f, 4f, 1.2f, 0.3f, -0.1f }, offsets);

        Assert.IsTrue(MemoryMagnitude.CompareByWeight(-9f, 2f) < 0, "A heavy grief outranks a light joy.");
        Assert.IsTrue(MemoryMagnitude.CompareByWeight(2f, -9f) > 0);
        Assert.AreEqual(0, MemoryMagnitude.CompareByWeight(3f, -3f), "Equal weight, either order.");
    }

    /// <summary>
    ///     A sunken box with nothing in it reads as a broken window rather than an empty one.
    /// </summary>
    [TestMethod]
    public void BothColumnsSayWhyTheyAreEmpty() {
        string dialog = Core("System", "Scadrial", "Feruchemy", "UI", "Dialog_StoreMemory.cs");
        HashSet<string> keys = CoreKeys();

        foreach (string key in new[] {
            "CC_Codex_Feruchemy_StoreMemory_Empty_Memories",
            "CC_Codex_Feruchemy_StoreMemory_Empty_Copperminds",
            "CC_Codex_Feruchemy_StoreMemory_NoRoomTip",
        }) {
            Assert.IsTrue(dialog.Contains(key), $"{key} is never shown.");
            Assert.IsTrue(keys.Contains(key), $"{key} is referenced but defined nowhere in Keyed.");
        }

        Assert.IsTrue(dialog.Contains("memories.Count == 0"), "An empty memory list needs its own line.");
        Assert.IsTrue(dialog.Contains("copperminds.Count == 0"), "So does an empty coppermind list.");
    }

    /// <summary>
    ///     A greyed row with no reason reads as a bug. CanFitMemory compares the memory against
    ///     FreeSpace, so FreeSpace is the number the player needs.
    /// </summary>
    [TestMethod]
    public void AFullCoppermindNamesTheRoomItLacks() {
        string dialog = Core("System", "Scadrial", "Feruchemy", "UI", "Dialog_StoreMemory.cs");

        Assert.IsTrue(dialog.Contains("TooltipHandler.TipRegion"), "The refusal has to be readable.");
        Assert.IsTrue(dialog.Contains("mind.FreeSpace"), "Free space is a property; do not recompute it.");

        // the key can live in any file under Keyed; the game merges the folder.
        string tip = Directory
            .EnumerateFiles(
                Path.Combine(RepoRoot, "CosmereCore", "Languages", "English", "Keyed"),
                "*.xml"
            )
            .SelectMany(f => XDocument.Load(f).Descendants("CC_Codex_Feruchemy_StoreMemory_NoRoomTip"))
            .Single()
            .Value;
        Assert.IsTrue(tip.Contains("{NEEDED}") && tip.Contains("{FREE}"), "Both numbers, or it explains nothing.");
    }

    /// <summary>
    ///     The spren strip was a click target with no hover, no sound and no name.
    /// </summary>
    [TestMethod]
    public void ASprenEntryAnnouncesItself() {
        string codex = Core("System", "Roshar", "UI", "SurgebindingCodexContent.cs");
        HashSet<string> keys = CoreKeys();

        foreach (string owed in new[] {
            "DrawHighlightIfMouseover", "MouseoverSounds.DoRegion", "TooltipHandler.TipRegion",
        }) {
            Assert.IsTrue(codex.Contains(owed), $"An interactive rect needs {owed}.");
        }

        foreach (string key in new[] {
            "CC_Codex_Surgebinding_SprenEntry_Tip", "CC_Codex_Surgebinding_SprenEntry_TipUnsworn",
        }) {
            Assert.IsTrue(codex.Contains(key), $"{key} is never shown.");
            Assert.IsTrue(keys.Contains(key), $"{key} is referenced but defined nowhere in Keyed.");
        }
    }

    /// <summary>
    ///     There are only Tiny, Small and Medium, and a scroll view that ignores the scrollbar
    ///     clips its own last column.
    /// </summary>
    [TestMethod]
    public void TheWindowKeepsToRimWorldsDrawingRules() {
        string dialog = Core("System", "Scadrial", "Feruchemy", "UI", "Dialog_StoreMemory.cs");

        Assert.IsFalse(dialog.Contains("GameFont.Large"), "There are only Tiny, Small and Medium.");
        Assert.IsTrue(dialog.Contains("ScrollbarWidth"), "A scroll view has to deduct the scrollbar.");
        Assert.IsTrue(dialog.Contains("MagnitudeColumnWidth"), "The signed figure needs a fixed column.");
    }
}
