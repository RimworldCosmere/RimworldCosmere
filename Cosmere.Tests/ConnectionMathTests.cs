using System;
using System.Collections.Generic;
using System.IO;
using Cosmere.Core.ShardConnection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     The Connection arithmetic, checked against the worked examples in the design.
///     <para>
///         The one that matters most is the refugee: PawnExtension.CanUseMetal short-circuits on
///         metal.godMetal today, so a vanilla space refugee with no Investiture at all can burn
///         atium. These numbers are what replaces that clause.
///     </para>
/// </summary>
[TestClass]
public class ConnectionMathTests {
    private const int Floor = ConnectionMath.AncestryFloor;
    private const int Misting = ConnectionMath.SingleInvestitureBonus;
    private const int Mistborn = ConnectionMath.FullInvestitureBonus;

    [TestMethod]
    public void TierBoundariesMatchTheDesign() {
        Assert.AreEqual(ConnectionTier.None, ConnectionMath.TierOf(0));
        Assert.AreEqual(ConnectionTier.Touched, ConnectionMath.TierOf(1));
        Assert.AreEqual(ConnectionTier.Touched, ConnectionMath.TierOf(30));
        Assert.AreEqual(ConnectionTier.Bonded, ConnectionMath.TierOf(31));
        Assert.AreEqual(ConnectionTier.Bonded, ConnectionMath.TierOf(70));
        Assert.AreEqual(ConnectionTier.Invested, ConnectionMath.TierOf(71));
        Assert.AreEqual(ConnectionTier.Invested, ConnectionMath.TierOf(99));
        Assert.AreEqual(ConnectionTier.Ascendant, ConnectionMath.TierOf(100));
    }

    /// <summary>Only a Shardholder reaches Ascendant. Extreme play has to stop at 99.</summary>
    [TestMethod]
    public void OrdinarySourcesCannotReachAscendant() {
        int everything = ConnectionMath.Compose(Floor, 0, Mistborn + Mistborn, 0);
        Assert.AreEqual(70, everything, "Mistborn and Full Feruchemist together is the top of Bonded.");
        Assert.AreEqual(ConnectionTier.Bonded, ConnectionMath.TierOf(everything));
    }

    [TestMethod]
    public void TheWorkedExamplesHold() {
        int plain = ConnectionMath.Compose(Floor, 0, 0, 0);
        Assert.AreEqual(30, plain);
        Assert.AreEqual(ConnectionTier.Touched, ConnectionMath.TierOf(plain));

        int misting = ConnectionMath.Compose(Floor, 0, Misting, 0);
        Assert.AreEqual(40, misting);
        Assert.AreEqual(ConnectionTier.Bonded, ConnectionMath.TierOf(misting));

        int mistborn = ConnectionMath.Compose(Floor, 0, Mistborn, 0);
        Assert.AreEqual(50, mistborn);
        Assert.AreEqual(ConnectionTier.Bonded, ConnectionMath.TierOf(mistborn));

        int refugee = ConnectionMath.Compose(0, 0, 0, 0);
        Assert.AreEqual(0, refugee);
        Assert.AreEqual(ConnectionTier.None, ConnectionMath.TierOf(refugee));
    }

    /// <summary>
    ///     The gate that fixes the reported bug. An off-world refugee must not burn atium; a
    ///     plain Scadrian must.
    /// </summary>
    [TestMethod]
    public void GodMetalNeedsTouched() {
        Assert.IsFalse(ConnectionMath.MayUseGodMetal(0), "A refugee with no Investiture cannot burn atium.");
        Assert.IsFalse(ConnectionMath.MayUseGodMetal(29));
        Assert.IsTrue(ConnectionMath.MayUseGodMetal(30), "A plain Scadrian can.");
        Assert.IsTrue(ConnectionMath.MayUseGodMetal(100));
    }

    /// <summary>
    ///     Residence naturalises a pawn toward what being born there grants. It is another route
    ///     to the same baseline, not a second helping of it - a year on Scadrial must not put a
    ///     native above a native.
    /// </summary>
    [TestMethod]
    public void ResidenceReachesTheFloorWithoutStackingOnIt() {
        Assert.AreEqual(30, ConnectionMath.Compose(0, 30, 0, 0), "A full year of residence reaches the floor.");
        Assert.AreEqual(15, ConnectionMath.Compose(0, 15, 0, 0), "Half a year gets halfway.");
        Assert.AreEqual(
            30,
            ConnectionMath.Compose(Floor, 30, 0, 0),
            "A native who has also lived there is still 30, not 60."
        );
    }

    /// <summary>Harmony holds both, so Connection to Harmony is Connection to each.</summary>
    [TestMethod]
    public void HarmonyCarriesToRuinAndPreservation() {
        Assert.AreEqual(40, ConnectionMath.WithHarmony(0, 40));
        Assert.AreEqual(
            55,
            ConnectionMath.WithHarmony(55, 40),
            "Harmony must never lower a strength the pawn already had."
        );
        Assert.AreEqual(0, ConnectionMath.WithHarmony(0, 0));
    }

    [TestMethod]
    public void ComposeNeverLeavesTheScale() {
        Assert.AreEqual(100, ConnectionMath.Compose(Floor, 0, 200, 500));
        Assert.AreEqual(0, ConnectionMath.Compose(0, 0, 0, -50));
    }

    /// <summary>
    ///     Storage is the SpiritWeb's 0..1 edge, so the two scales have to round-trip. A drift
    ///     here would move a pawn across a tier boundary on save and reload.
    /// </summary>
    [TestMethod]
    public void EdgeConversionRoundTrips() {
        foreach (int strength in new[] { 0, 1, 30, 31, 50, 70, 71, 99, 100 }) {
            Assert.AreEqual(
                strength,
                ConnectionMath.FromEdge(ConnectionMath.ToEdge(strength)),
                $"{strength} did not survive the trip through the SpiritWeb edge."
            );
        }
    }

    /// <summary>
    ///     The clause this whole system replaces. CanUseMetal read
    ///     <c>if (metal.godMetal || ...) return true;</c>, so godMetal short-circuited to true for
    ///     any pawn at all and a vanilla space refugee could burn atium.
    /// </summary>
    [TestMethod]
    public void GodMetalNoLongerShortCircuitsToTrue() {
        string source = File.ReadAllText(
            Path.Combine(
                RepoRoot, "CosmereCore", "CosmereCore", "System", "Scadrial", "Extension", "PawnExtension.cs"
            )
        );

        int gate = source.IndexOf("ConnectionUtility.MayUse", StringComparison.Ordinal);
        int shortCircuit = source.IndexOf(
            "if (metal.godMetal || pawn.IsMistborn()", StringComparison.Ordinal
        );

        Assert.IsTrue(gate >= 0, "Expected god metal use to go through the Connection gate.");
        Assert.IsTrue(
            shortCircuit < 0 || gate < shortCircuit,
            "The Connection gate has to run before the godMetal short-circuit, or it never fires."
        );
    }

    /// <summary>
    ///     Every god metal has to name the Shard it is a piece of, or the gate has nothing to
    ///     check against and silently lets everyone through.
    /// </summary>
    [TestMethod]
    public void EveryGodMetalNamesItsShard() {
        List<string> offenders = [];
        int seen = 0;

        foreach (string path in Directory.GetFiles(
                     Path.Combine(RepoRoot, "Resources", "Data", "Metals"), "*.json")) {
            string json = File.ReadAllText(path);
            if (!json.Contains("\"godMetal\": true", StringComparison.OrdinalIgnoreCase)) continue;

            seen++;
            if (!json.Contains("\"shard\"", StringComparison.Ordinal)) {
                offenders.Add(Path.GetFileNameWithoutExtension(path));
            }
        }

        Assert.IsTrue(seen > 0, "Found no god metals - the walk is wrong, not the data.");
        Assert.AreEqual(0, offenders.Count, "These god metals name no Shard: " + string.Join(", ", offenders));
    }

    /// <summary>
    ///     Ancestry hands out floors from a world's fallback Shards, which on Scadrial are Ruin
    ///     and Preservation, so nothing ever grants a floor to Harmony. Without this a
    ///     post-Catacendre native reads 0 to the Shard their own world is held by.
    /// </summary>
    [TestMethod]
    public void BeingTiedToBothHalvesIsBeingTiedToHarmony() {
        Assert.AreEqual(
            30,
            ConnectionMath.HarmonyFrom(30, 30),
            "A native with the floor to both halves is tied to Harmony."
        );
        Assert.AreEqual(
            0,
            ConnectionMath.HarmonyFrom(30, 0),
            "Half of Harmony is not Harmony - one half alone grants nothing."
        );
        Assert.AreEqual(20, ConnectionMath.HarmonyFrom(50, 20), "It is the weaker half that decides.");
        Assert.AreEqual(0, ConnectionMath.HarmonyFrom(0, 0), "An off-worlder is tied to none of it.");
    }

    /// <summary>The two directions have to agree, or a pawn's Harmony reading depends on which way you ask.</summary>
    [TestMethod]
    public void TheHarmonyRuleIsSymmetric() {
        int viaHalves = ConnectionMath.HarmonyFrom(40, 40);
        Assert.AreEqual(40, viaHalves);
        Assert.AreEqual(
            40,
            ConnectionMath.WithHarmony(0, viaHalves),
            "Harmony derived from both halves must carry back to each of them unchanged."
        );
    }

    private static string RepoRoot {
        get {
            DirectoryInfo? dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "CosmereScadrial", "Defs"))) {
                dir = dir.Parent;
            }

            Assert.IsNotNull(dir, "Could not locate the repo root above the test output directory.");
            return dir!.FullName;
        }
    }
}
