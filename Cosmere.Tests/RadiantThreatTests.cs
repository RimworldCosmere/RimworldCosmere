using Cosmere.System.Roshar.Threat;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     A Radiant's threat contribution is a multiple of an ordinary colonist, not a flat number
///     of points. The scale has to hold at both ends of vanilla's wealth curve.
/// </summary>
/// <remarks>
///     The rule this replaced paid CurrentIdealDisplay * 40 per Surgebinder gene. That was 2.7
///     colonists at low wealth, a third of one at a million, and it doubled a second Nahel bond.
/// </remarks>
[TestClass]
public class RadiantThreatTests {
    [TestMethod]
    public void FirstIdeal_CostsMoreThanAColonist_ButNotMuchMore() {
        float worth = RadiantThreat.ForBond(0);

        Assert.IsTrue(worth > 1f, $"a First Ideal Radiant should beat a plain colonist, got {worth}");
        Assert.IsTrue(worth < 1.5f, $"a First Ideal Radiant should not be half a colonist again, got {worth}");
    }

    [TestMethod]
    public void IdealsGrowFasterThanLinear() {
        float[] steps = new float[RadiantThreat.MaxIdeal + 1];
        for (int i = 0; i <= RadiantThreat.MaxIdeal; i++) {
            steps[i] = RadiantThreat.ForBond(i);
        }

        for (int i = 2; i <= RadiantThreat.MaxIdeal; i++) {
            float previousGap = steps[i - 1] - steps[i - 2];
            float thisGap = steps[i] - steps[i - 1];
            Assert.IsTrue(
                thisGap > previousGap,
                $"ideal {i} gained {thisGap} where ideal {i - 1} gained {previousGap}"
            );
        }
    }

    /// <summary>
    ///     Five First Ideal Radiants must outweigh one Fifth Ideal. The old rule paid
    ///     CurrentIdealDisplay * 40, making both exactly 200, which is why this curve exists.
    /// </summary>
    [TestMethod]
    public void FiveFirstIdealsBeatOneFifthIdeal() {
        float five = 5f * RadiantThreat.ForBond(0);
        float one = RadiantThreat.ForBond(RadiantThreat.MaxIdeal);

        Assert.IsTrue(five > one, $"five firsts were {five}, one fifth was {one}");
    }

    [TestMethod]
    public void IdealIsClampedAtBothEnds() {
        Assert.AreEqual(RadiantThreat.ForBond(0), RadiantThreat.ForBond(-3));
        Assert.AreEqual(
            RadiantThreat.ForBond(RadiantThreat.MaxIdeal),
            RadiantThreat.ForBond(RadiantThreat.MaxIdeal + 9)
        );
    }

    [TestMethod]
    public void ShardsAreNotFree() {
        Assert.AreEqual(0f, RadiantThreat.ForShards(false, false), 0.0001f);
        Assert.AreEqual(RadiantThreat.BladeWorth, RadiantThreat.ForShards(true, false), 0.0001f);
        Assert.AreEqual(RadiantThreat.PlateWorth, RadiantThreat.ForShards(false, true), 0.0001f);
        Assert.AreEqual(
            RadiantThreat.BladeWorth + RadiantThreat.PlateWorth,
            RadiantThreat.ForShards(true, true),
            0.0001f
        );
    }

    /// <summary>
    ///     A Dead Shardblade is a hundred-silver weapon any colonist can pick up and swing, so the
    ///     price rides the pawn holding it rather than the Nahel bond.
    /// </summary>
    [TestMethod]
    public void ShardsPriceAPawnWithNoBond() {
        float plain = RadiantThreat.ForPawn(null, 0f);
        float armed = RadiantThreat.ForPawn(null, RadiantThreat.ForShards(true, true));

        Assert.AreEqual(1f, plain, 0.0001f);
        Assert.IsTrue(armed > plain + 3f, $"a bondless Shardbearer should be a squad, got {armed}");
    }

    /// <summary>
    ///     Shards are the bigger swing of the two axes. A First Ideal Radiant in full Plate with a
    ///     Blade must outweigh a Fifth Ideal Radiant carrying nothing.
    /// </summary>
    [TestMethod]
    public void ShardsOutweighIdealsAlone() {
        float armedFirst = RadiantThreat.ForPawn(
            [RadiantThreat.ForBond(0)],
            RadiantThreat.ForShards(true, true)
        );
        float bareFifth = RadiantThreat.ForPawn([RadiantThreat.ForBond(RadiantThreat.MaxIdeal)], 0f);

        Assert.IsTrue(armedFirst > bareFifth, $"armed first was {armedFirst}, bare fifth was {bareFifth}");
    }

    [TestMethod]
    public void PlateOutweighsBlade() {
        Assert.IsTrue(
            RadiantThreat.PlateWorth > RadiantThreat.BladeWorth,
            "Shardplate is the bigger combat swing of the two"
        );
    }

    [TestMethod]
    public void NoBonds_IsAPlainColonist() {
        Assert.AreEqual(1f, RadiantThreat.ForPawn(null, 0f), 0.0001f);
        Assert.AreEqual(1f, RadiantThreat.ForPawn([], 0f), 0.0001f);
    }

    [TestMethod]
    public void OneBond_CountsInFull() {
        float bond = RadiantThreat.ForBond(3);

        Assert.AreEqual(bond, RadiantThreat.ForPawn([bond], 0f), 0.0001f);
    }

    /// <summary>
    ///     The dock supports two Nahel bonds. The old loop added per Surgebinder gene, so a
    ///     second bond doubled the pawn outright.
    /// </summary>
    [TestMethod]
    public void SecondBond_AddsSomething_ButNeverDoubles() {
        float bond = RadiantThreat.ForBond(4);
        float pair = RadiantThreat.ForPawn([bond, bond], 0f);

        Assert.IsTrue(pair > bond, $"a second bond should count for something, got {pair} against {bond}");
        Assert.IsTrue(pair < bond * 2f, $"a second bond doubled the pawn: {pair} against {bond}");
    }

    [TestMethod]
    public void StrongestBondCountsInFull_RegardlessOfOrder() {
        float weak = RadiantThreat.ForBond(0);
        float strong = RadiantThreat.ForBond(4);

        Assert.AreEqual(RadiantThreat.ForPawn([weak, strong], 0f), RadiantThreat.ForPawn([strong, weak], 0f), 0.0001f);
        Assert.IsTrue(
            RadiantThreat.ForPawn([weak, strong], 0f) >= strong,
            "the strongest bond must never be discounted by a weaker one"
        );
    }

    /// <summary>
    ///     A Fifth Ideal Radiant in full Shardplate with a Blade is the strongest thing a colony
    ///     can field, and still has to stay inside a sane multiple of one colonist.
    /// </summary>
    [TestMethod]
    public void TheStrongestRadiantStaysInRange() {
        float worth = RadiantThreat.ForPawn(
            [RadiantThreat.ForBond(RadiantThreat.MaxIdeal)],
            RadiantThreat.ForShards(true, true)
        );

        Assert.IsTrue(worth > 6f, $"a Fifth Ideal Shardbearer should be worth more than six colonists, got {worth}");
        Assert.IsTrue(worth < 10f, $"no single pawn should be worth ten colonists, got {worth}");
    }
}
