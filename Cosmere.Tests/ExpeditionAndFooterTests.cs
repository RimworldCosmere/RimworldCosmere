using System;
using System.IO;
using Cosmere.System.Roshar.Comp.Map;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     The plateau run and the oath footer both promise the player something before they commit,
///     and both used to hide the thing that mattered behind a button that looked like any other.
/// </summary>
[TestClass]
public class ExpeditionAndFooterTests {
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

    private static string Footer() => Core("System", "Roshar", "Dialog", "Dialog_RadiantOrderInfoDialog.cs");

    private static string ExpeditionDialog() => Core("System", "Roshar", "Dialog", "Dialog_GemheartExpedition.cs");

    [TestMethod]
    public void PawnPowerWeightsMeleeOverShootingAndIdealsOverBoth() {
        Assert.AreEqual(30f, GemheartOdds.PawnPower(10, 0, 0, 0f), 0.001f);
        Assert.AreEqual(20f, GemheartOdds.PawnPower(0, 10, 0, 0f), 0.001f);
        Assert.AreEqual(75f, GemheartOdds.PawnPower(0, 0, 3, 0f), 0.001f);
        Assert.AreEqual(24f, GemheartOdds.PawnPower(0, 0, 0, 12f), 0.001f);
        Assert.AreEqual(149f, GemheartOdds.PawnPower(10, 10, 3, 12f), 0.001f);
    }

    [TestMethod]
    public void PartyPowerAddsFivePerBody() {
        Assert.AreEqual(115f, GemheartOdds.PartyPower(100f, 3), 0.001f);
        Assert.AreEqual(130f, GemheartOdds.PartyPower(100f, 6), 0.001f);
    }

    [TestMethod]
    public void DifficultyRisesWithDaysAndWealth() {
        Assert.AreEqual(100f, GemheartOdds.Difficulty(0, 0f), 0.001f);
        Assert.AreEqual(150f, GemheartOdds.Difficulty(100, 0f), 0.001f);
        Assert.AreEqual(120f, GemheartOdds.Difficulty(0, 200000f), 0.001f);
    }

    [TestMethod]
    public void RatioTreatsZeroDifficultyAsOverwhelmingStrength() {
        Assert.AreEqual(2f, GemheartOdds.Ratio(0f, 0f), 0.001f);
        Assert.AreEqual(1.5f, GemheartOdds.Ratio(150f, 100f), 0.001f);
    }

    [TestMethod]
    public void ClassifyBandsMatchTheResolverThresholds() {
        Assert.AreEqual(GemheartOutcome.Victory, GemheartOdds.Classify(1.5f));
        Assert.AreEqual(GemheartOutcome.HardWon, GemheartOdds.Classify(1.4999f));
        Assert.AreEqual(GemheartOutcome.HardWon, GemheartOdds.Classify(1.0f));
        Assert.AreEqual(GemheartOutcome.Pyrrhic, GemheartOdds.Classify(0.9999f));
        Assert.AreEqual(GemheartOutcome.Pyrrhic, GemheartOdds.Classify(0.6f));
        Assert.AreEqual(GemheartOutcome.Failure, GemheartOdds.Classify(0.5999f));
        Assert.AreEqual(GemheartOutcome.Failure, GemheartOdds.Classify(0.3f));
        Assert.AreEqual(GemheartOutcome.Disaster, GemheartOdds.Classify(0.2999f));
        Assert.AreEqual(GemheartOutcome.Disaster, GemheartOdds.Classify(0f));
    }

    [TestMethod]
    public void SeverBondIsNotTheLeftmostFooterButton() {
        string source = Footer();
        int leftAssign = source.IndexOf("Rect leftButtonRect", StringComparison.Ordinal);
        int rightAssign = source.IndexOf("Rect rightButtonRect", StringComparison.Ordinal);
        int sever = source.IndexOf("Widgets.ButtonText(rightButtonRect, \"CRO_BreakBond_Sever\"", StringComparison.Ordinal);

        Assert.IsTrue(leftAssign > 0 && rightAssign > leftAssign, "footer rects are gone or reordered");
        Assert.IsTrue(sever > 0, "sever bond no longer sits in the right footer slot");
        Assert.IsFalse(
            source.Contains("Widgets.ButtonText(leftButtonRect, \"CRO_BreakBond_Sever\"", StringComparison.Ordinal),
            "sever bond is back in the leftmost slot"
        );
        Assert.IsTrue(
            source.Contains("TooltipHandler.TipRegion(rightButtonRect, \"CRO_BreakBond_SeverTooltip\"", StringComparison.Ordinal),
            "the destructive button lost its tooltip"
        );
    }

    [TestMethod]
    public void FooterRectsDeriveFromTheInnerRect() {
        string source = Footer();
        Assert.IsTrue(source.Contains("innerRect.xMax - unit * 2f", StringComparison.Ordinal));
        Assert.IsTrue(source.Contains("innerRect.x + unit * 4f", StringComparison.Ordinal));
    }

    [TestMethod]
    public void WholeGemheartRowIsTheClickTarget() {
        string source = ExpeditionDialog();
        Assert.IsTrue(
            source.Contains("Widgets.ButtonInvisible(rowRect)", StringComparison.Ordinal),
            "the row is not clickable as a whole");
        Assert.IsTrue(
            source.Contains("Widgets.CheckboxDraw(", StringComparison.Ordinal),
            "the checkbox must be draw-only so the row owns the click");
        Assert.IsFalse(
            source.Contains("Widgets.Checkbox(", StringComparison.Ordinal),
            "an input checkbox steals the click back from the row");
    }

    [TestMethod]
    public void GemheartRowsAnswerOnHover() {
        string source = ExpeditionDialog();
        Assert.IsTrue(source.Contains("Widgets.DrawHighlightIfMouseover(rowRect)", StringComparison.Ordinal));
        Assert.IsTrue(source.Contains("MouseoverSounds.DoRegion(rowRect)", StringComparison.Ordinal));
        Assert.IsTrue(
            source.Contains("TooltipHandler.TipRegion(", StringComparison.Ordinal)
            && source.Contains("CRO_Gemheart_Row_Tooltip", StringComparison.Ordinal),
            "rows have no tooltip");
    }

    [TestMethod]
    public void OddsReadoutUsesTheResolverMath() {
        string source = ExpeditionDialog();
        Assert.IsTrue(source.Contains("GemheartExpeditionManager.ExpeditionPower(", StringComparison.Ordinal));
        Assert.IsTrue(source.Contains("GemheartOdds.Classify(GemheartOdds.Ratio(", StringComparison.Ordinal));
    }

    [TestMethod]
    public void StripingSurvivesTheNewHover() {
        string source = ExpeditionDialog();
        Assert.IsTrue(source.Contains("if (striped) Widgets.DrawLightHighlight(rowRect)", StringComparison.Ordinal));
    }
}
