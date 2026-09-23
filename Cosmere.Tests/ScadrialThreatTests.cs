using System.Collections.Generic;
using Cosmere.System.Scadrial.Threat;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     A metalborn's threat contribution is a multiple of an ordinary colonist, and the full gift
///     always outbids the fraction of it a Misting holds.
/// </summary>
/// <remarks>
///     The rule this replaced paid a flat 150 for a Mistborn and 20 a metal otherwise, billed
///     every Hemalurgic spike twice, and let one copper Misting shrink the whole raid.
/// </remarks>
[TestClass]
public class ScadrialThreatTests {
    [TestMethod]
    public void NoPowers_IsAPlainColonist() {
        Assert.AreEqual(1f, ScadrialThreat.ForMetalborn(false, false, 0, 0), 0.0001f);
    }

    [TestMethod]
    public void OneMetal_CostsMoreThanAColonist() {
        float misting = ScadrialThreat.ForMetalborn(false, false, 1, 0);

        Assert.IsTrue(misting > 1f, $"a Misting should beat a plain colonist, got {misting}");
        Assert.IsTrue(misting < 2f, $"one metal should not double a pawn, got {misting}");
    }

    /// <summary>
    ///     The headline bug in the rule this replaces. A Mistborn burns every metal, so no stack of
    ///     single metals may ever out-price one.
    /// </summary>
    [TestMethod]
    public void NoMistingStackOutbidsAMistborn() {
        float mistborn = ScadrialThreat.ForMetalborn(true, false, 0, 0);

        for (int metals = 1; metals <= 20; metals++) {
            float stack = ScadrialThreat.ForMetalborn(false, false, metals, 0);
            Assert.IsTrue(stack <= mistborn, $"{metals} metals billed {stack} against a Mistborn's {mistborn}");
        }
    }

    [TestMethod]
    public void NoFerringStackOutbidsAFullFeruchemist() {
        float full = ScadrialThreat.ForMetalborn(false, true, 0, 0);

        for (int metals = 1; metals <= 20; metals++) {
            float stack = ScadrialThreat.ForMetalborn(false, false, 0, metals);
            Assert.IsTrue(stack <= full, $"{metals} metals billed {stack} against a full Feruchemist's {full}");
        }
    }

    [TestMethod]
    public void MistbornOutweighsFullFeruchemist() {
        Assert.IsTrue(
            ScadrialThreat.ForMetalborn(true, false, 0, 0) > ScadrialThreat.ForMetalborn(false, true, 0, 0),
            "burning every metal beats storing every attribute"
        );
    }

    /// <summary>
    ///     Burning and storing feed each other, so a pawn holding one metal on each axis is worth
    ///     more than the two halves added up.
    /// </summary>
    [TestMethod]
    public void TwinbornBeatsTheSumOfItsHalves() {
        float allomancyOnly = ScadrialThreat.ForMetalborn(false, false, 1, 0);
        float feruchemyOnly = ScadrialThreat.ForMetalborn(false, false, 0, 1);
        float twinborn = ScadrialThreat.ForMetalborn(false, false, 1, 1);

        float halvesAdded = allomancyOnly + feruchemyOnly - 1f;
        Assert.IsTrue(twinborn > halvesAdded, $"twinborn billed {twinborn} against {halvesAdded} for the halves");
    }

    [TestMethod]
    public void OneAxisAloneEarnsNoTwinbornPremium() {
        float allomancyOnly = ScadrialThreat.ForMetalborn(false, false, 3, 0);

        Assert.AreEqual(1f + 3f * ScadrialThreat.AllomanticMetalWorth, allomancyOnly, 0.0001f);
    }

    [TestMethod]
    public void FullBothIsTheMostExpensivePawn() {
        float both = ScadrialThreat.ForMetalborn(true, true, 0, 0);

        Assert.IsTrue(both > ScadrialThreat.MistbornWorth, $"a Mistborn full Feruchemist billed only {both}");
        Assert.IsTrue(both < 10f, $"no single pawn should be worth ten colonists, got {both}");
    }

    [TestMethod]
    public void NegativeMetalCountsAreTreatedAsNone() {
        Assert.AreEqual(1f, ScadrialThreat.ForMetalborn(false, false, -4, -2), 0.0001f);
    }

    [TestMethod]
    public void SpikesCostSomethingAndScale() {
        Assert.AreEqual(0f, ScadrialThreat.ForSpikes(0), 0.0001f);
        Assert.AreEqual(0f, ScadrialThreat.ForSpikes(-3), 0.0001f);
        Assert.AreEqual(ScadrialThreat.SpikeWorth, ScadrialThreat.ForSpikes(1), 0.0001f);
        Assert.IsTrue(ScadrialThreat.ForSpikes(4) > ScadrialThreat.ForSpikes(3));
    }

    /// <summary>
    ///     ImplantSpike grants the gene, so the gene shows up in the metal count on its own. A
    ///     spike may only be priced once, as the spike.
    /// </summary>
    [TestMethod]
    public void ASpikeCostsLessThanTheGiftItGrants() {
        Assert.IsTrue(
            ScadrialThreat.SpikeWorth < ScadrialThreat.MistbornWorth - 1f,
            "a single spike must not approach the full Allomantic gift"
        );
    }

    [TestMethod]
    public void CoppercloudShrinksTheWholeValue() {
        Assert.AreEqual(100f, ScadrialThreat.Coppercloud(100f, 0f), 0.0001f);
        Assert.AreEqual(75f, ScadrialThreat.Coppercloud(100f, 0.25f), 0.0001f);
        Assert.AreEqual(0f, ScadrialThreat.Coppercloud(100f, 1f), 0.0001f);
    }

    [TestMethod]
    public void CoppercloudCoverageIsClamped() {
        Assert.AreEqual(100f, ScadrialThreat.Coppercloud(100f, -2f), 0.0001f);
        Assert.AreEqual(0f, ScadrialThreat.Coppercloud(100f, 4f), 0.0001f);
    }

    /// <summary>
    ///     A cloud only hides what stands inside it. GetCoppercloudStrength ignored position
    ///     entirely, so a Smoker burning copper across the map concealed the whole colony.
    /// </summary>
    [TestMethod]
    public void ACloudOverNobodyHidesNothing() {
        Assert.AreEqual(0f, ScadrialThreat.Coverage(null), 0.0001f);
        Assert.AreEqual(0f, ScadrialThreat.Coverage([]), 0.0001f);
        Assert.AreEqual(0f, ScadrialThreat.Coverage([0f, 0f, 0f]), 0.0001f);
    }

    [TestMethod]
    public void CoverageIsTheColonyAverage_NotTheStrongestCloud() {
        Assert.AreEqual(0.25f, ScadrialThreat.Coverage([0.25f, 0.25f]), 0.0001f);
        Assert.AreEqual(0.125f, ScadrialThreat.Coverage([0.25f, 0f]), 0.0001f);
        Assert.AreEqual(0.0625f, ScadrialThreat.Coverage([0.25f, 0f, 0f, 0f]), 0.0001f);
    }

    /// <summary>
    ///     Hiding one pawn out of twenty must not hide the colony. The old map-wide rule gave the
    ///     full reduction no matter how many colonists stood out in the open.
    /// </summary>
    [TestMethod]
    public void OneShelteredPawnDoesNotHideAColony() {
        List<float> colony = [];
        colony.Add(0.25f);
        for (int i = 0; i < 19; i++) colony.Add(0f);

        float coverage = ScadrialThreat.Coverage(colony);
        Assert.IsTrue(coverage < 0.02f, $"one pawn in twenty hid {coverage} of the colony");
        Assert.IsTrue(ScadrialThreat.Coppercloud(1000f, coverage) > 985f);
    }

    [TestMethod]
    public void GearIsNotFree() {
        Assert.AreEqual(0f, ScadrialThreat.ForGear(0, 0), 0.0001f);
        Assert.IsTrue(ScadrialThreat.ForGear(1, 0) > 0f);
        Assert.IsTrue(ScadrialThreat.ForGear(0, 1) > 0f);
        Assert.IsTrue(ScadrialThreat.ForGear(3, 3) > ScadrialThreat.ForGear(1, 1));
    }

    /// <summary>
    ///     A pawn carrying a mule's load of metalminds is still one pawn, so each axis tops out.
    /// </summary>
    [TestMethod]
    public void GearCeilingsHold() {
        Assert.AreEqual(ScadrialThreat.MetalmindCeiling, ScadrialThreat.ForGear(99, 0), 0.0001f);
        Assert.AreEqual(ScadrialThreat.VialCeiling, ScadrialThreat.ForGear(0, 99), 0.0001f);
        Assert.AreEqual(
            ScadrialThreat.MetalmindCeiling + ScadrialThreat.VialCeiling,
            ScadrialThreat.ForGear(99, 99),
            0.0001f
        );
    }

    [TestMethod]
    public void NegativeGearCountsAreTreatedAsNone() {
        Assert.AreEqual(0f, ScadrialThreat.ForGear(-5, -5), 0.0001f);
    }

    /// <summary>
    ///     Kit must matter without out-pricing the gift it serves. A fully loaded Misting should
    ///     never approach a bare Mistborn.
    /// </summary>
    [TestMethod]
    public void GearNeverOutweighsTheGift() {
        float loadedMisting = ScadrialThreat.ForMetalborn(false, false, 1, 0) + ScadrialThreat.ForGear(99, 99);
        float bareMistborn = ScadrialThreat.ForMetalborn(true, false, 0, 0);

        Assert.IsTrue(loadedMisting < bareMistborn, $"loaded Misting {loadedMisting} vs Mistborn {bareMistborn}");
    }
}
