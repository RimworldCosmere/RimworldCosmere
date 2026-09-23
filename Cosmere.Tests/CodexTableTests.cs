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

    private static string ContentSource(string file) => File.ReadAllText(Path.Combine(
        RepoRoot,
        "CosmereCore",
        "CosmereCore",
        "System",
        "Scadrial",
        "UI",
        file
    ));

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
        Assert.IsTrue(CodexRowOrder.Compare(2, 0, 0, 999999) < 0, "A savant metal sorts above an unworked one.");
        Assert.IsTrue(CodexRowOrder.Compare(0, 999999, 2, 0) > 0, "Hours never overtake a higher stage.");
    }

    [TestMethod]
    public void TimeBreaksTiesWithinAStage() {
        Assert.IsTrue(CodexRowOrder.Compare(1, 500, 1, 100) < 0);
        Assert.IsTrue(CodexRowOrder.Compare(1, 100, 1, 500) > 0);
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

        CollectionAssert.AreEqual(
            new[] { 3, 1, 1, 0, 0 },
            rows.ConvertAll(r => r.stage)
        );
        Assert.AreEqual(400, rows[1].ticks);
        Assert.AreEqual(900, rows[3].ticks);
    }

    [TestMethod]
    public void ColumnOffsetsStayHardcoded() {
        foreach (string source in new[] { Allomancy, Feruchemy }) {
            StringAssert.Contains(source, "swatch.xMax + 146f", "The savant column offset is fixed at 146f.");
            StringAssert.Contains(source, "130f", "The name column is fixed at 130px wide.");
            StringAssert.Contains(source, "stageRect.xMax + 8f", "The usage text starts 8px past the stage.");
        }
    }

    [TestMethod]
    public void NameColumnTruncatesRatherThanResizing() {
        foreach (string source in new[] { Allomancy, Feruchemy }) {
            StringAssert.Contains(source, "UIText.EllipsisLabel");
            Assert.IsFalse(
                source.Contains("Text.CalcSize(metal"),
                "Measuring the metal label would make the column dynamic, which was rejected."
            );
        }
    }

    [TestMethod]
    public void EveryRowAnswersTheMouse() {
        foreach (string source in new[] { Allomancy, Feruchemy }) {
            StringAssert.Contains(source, "Widgets.DrawHighlightIfMouseover(row)");
            StringAssert.Contains(source, "MouseoverSounds.DoRegion(row)");
            StringAssert.Contains(source, "_RowTooltip\".Translate(");
        }
    }

    [TestMethod]
    public void TheWholeAllomancyRowOpensTheVialDialog() {
        StringAssert.Contains(Allomancy, "Widgets.ButtonInvisible(row)");
        StringAssert.Contains(Allomancy, "Dialog_AllomancyRestockSlider(gene)");
    }

    [TestMethod]
    public void CopperIsADefNotAString() {
        Assert.IsFalse(
            Feruchemy.Contains("\"Copper\""),
            "Copper is matched against a DefOf entry, never by defName string."
        );
        StringAssert.Contains(Feruchemy, "fs[i].metal == MetallicArtsMetalDefOf.Copper");
        StringAssert.Contains(Feruchemy, "mind.Metal == MetalDefOf.Copper");
    }

    [TestMethod]
    public void RowTooltipKeysAreDefined() {
        string keyed = KeyedXml();
        StringAssert.Contains(keyed, "<CC_Codex_Allomancy_RowTooltip>");
        StringAssert.Contains(keyed, "<CC_Codex_Feruchemy_RowTooltip>");
        StringAssert.Contains(keyed, "<CC_Codex_Savant_Stage0>");
    }
}
