using System;
using System.Collections.Generic;
using System.IO;
using Cosmere.System.Scadrial.UI;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     Codex metal tables: row order, and the column offsets Aaron reviewed and pinned after
///     rejecting a version that measured the name column.
/// </summary>
[TestClass]
public class CodexTableTests {
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

    private static string Allomancy => ContentSource("AllomancyCodexContent.cs");

    private static string Feruchemy => ContentSource("FeruchemyCodexContent.cs");

    private static readonly int[] DescendingStages = [3, 1, 1, 0, 0];

    private static string ContentSource(string file) {
        return File.ReadAllText(Path.Combine(
            RepoRoot,
            "CosmereCore",
            "CosmereCore",
            "System",
            "Scadrial",
            "UI",
            file
        ));
    }

    private static string KeyedXml() {
        string[] files = Directory.GetFiles(
            Path.Combine(RepoRoot, "CosmereCore", "Languages", "English", "Keyed"),
            "*.xml"
        );

        List<string> contents = [];
        for (int i = 0; i < files.Length; i++) {
            contents.Add(File.ReadAllText(files[i]));
        }

        return string.Join("\n", contents);
    }

    [TestMethod]
    public void SavantStageOutranksTimeWorked() {
        Assert.IsLessThan(0, CodexRowOrder.Compare(2, 0, 0, 999999), "A savant metal sorts above an unworked one.");
        Assert.IsGreaterThan(0, CodexRowOrder.Compare(0, 999999, 2, 0), "Hours never overtake a higher stage.");
    }

    [TestMethod]
    public void TimeBreaksTiesWithinAStage() {
        Assert.IsLessThan(0, CodexRowOrder.Compare(1, 500, 1, 100));
        Assert.IsGreaterThan(0, CodexRowOrder.Compare(1, 100, 1, 500));
        Assert.AreEqual(0, CodexRowOrder.Compare(1, 100, 1, 100));
    }

    [TestMethod]
    public void OrdersAWholeTableHighestFirst() {
        List<(int stage, int ticks)> rows = [
            (0, 10),
            (3, 0),
            (1, 400),
            (0, 900),
            (1, 50),
        ];
        rows.Sort((a, b) => CodexRowOrder.Compare(a.stage, a.ticks, b.stage, b.ticks));

        Assert.AreSequenceEqual(
            DescendingStages,
            rows.ConvertAll(r => r.stage)
        );
        Assert.AreEqual(400, rows[1].ticks);
        Assert.AreEqual(900, rows[3].ticks);
    }

    [TestMethod]
    public void ColumnOffsetsStayHardcoded() {
        foreach (string source in new[] { Allomancy, Feruchemy }) {
            Assert.Contains("swatch.xMax + 146f", source, "The savant column offset is fixed at 146f.");
            Assert.Contains("130f", source, "The name column is fixed at 130px wide.");
            Assert.Contains("stageRect.xMax + 8f", source, "The usage text starts 8px past the stage.");
        }
    }

    [TestMethod]
    public void NameColumnTruncatesRatherThanResizing() {
        foreach (string source in new[] { Allomancy, Feruchemy }) {
            Assert.Contains("UIText.EllipsisLabel", source);
            Assert.DoesNotContain(
                "Text.CalcSize(metal",
                source,
                "Measuring the metal label would make the column dynamic, which was rejected."
            );
        }
    }

    [TestMethod]
    public void EveryRowAnswersTheMouse() {
        foreach (string source in new[] { Allomancy, Feruchemy }) {
            Assert.Contains("Widgets.DrawHighlightIfMouseover(row)", source);
            Assert.Contains("MouseoverSounds.DoRegion(row)", source);
            Assert.Contains("_RowTooltip\".Translate(", source);
        }
    }

    [TestMethod]
    public void TheWholeAllomancyRowOpensTheVialDialog() {
        Assert.Contains("Widgets.ButtonInvisible(row)", Allomancy);
        Assert.Contains("Dialog_AllomancyRestockSlider(gene)", Allomancy);
    }

    [TestMethod]
    public void CopperIsADefNotAString() {
        Assert.DoesNotContain(
            "\"Copper\"",
            Feruchemy,
            "Copper is matched against a DefOf entry, never by defName string."
        );
        Assert.Contains("fs[i].metal == MetallicArtsMetalDefOf.Copper", Feruchemy);
        Assert.Contains("mind.Metal == MetalDefOf.Copper", Feruchemy);
    }

    [TestMethod]
    public void RowTooltipKeysAreDefined() {
        string keyed = KeyedXml();
        Assert.Contains("<CC_Codex_Allomancy_RowTooltip>", keyed);
        Assert.Contains("<CC_Codex_Feruchemy_RowTooltip>", keyed);
        Assert.Contains("<CC_Codex_Savant_Stage0>", keyed);
    }
}
