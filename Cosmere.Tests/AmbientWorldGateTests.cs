using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     Which Shards are active says what exists in this cosmere; the world says where you are.
///     Ambient content needs both, and now says so once in a CosmereFeatureDef rather than
///     repeating the conditions at every call site.
///     <para>
///         Pawn abilities are deliberately not covered here: they gate on Connection, so a
///         Rosharan worldhopper keeps Surgebinding while standing on Scadrial.
///     </para>
/// </summary>
[TestClass]
public class AmbientWorldGateTests {
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

    private static string SystemDir => Path.Combine(RepoRoot, "CosmereCore", "CosmereCore", "System");

    /// <summary>Files whose content appears in the world without a pawn asking for it.</summary>
    private static readonly string[] AmbientFiles = [
        "Roshar/Comp/Map/TrueSprenSpawner.cs",
        "Roshar/Comp/Map/HighstormScheduler.cs",
        "Roshar/Comp/Map/StormlightNetwork.cs",
        "Roshar/LesserSpren/MapComponent/LesserSprenSpawner.cs",
        "Roshar/Comp/Game/NightwatcherValleyPlacer.cs",
        "Roshar/Nightwatcher/NightwatcherSystem.cs",
        "Roshar/WorldObject/NightwatcherValley.cs",
        "Scadrial/Comp/Map/MistsWatcher.cs",
        "Scadrial/Util/AshEra.cs",
        "Scadrial/Incident/Worker/IncidentWorker_PreservationBead.cs",
    ];

    /// <summary>
    ///     Xenotype patches gate on the world alone. A Rosharan is a Rosharan whether or not
    ///     Honor still exists, so they are not phenomena and get no feature def.
    /// </summary>
    private static readonly string[] WorldOnlyFiles = [
        "Roshar/Patch/World/RosharXenotypePatch.cs",
        "Scadrial/Patch/Gene/ScadrialXenotypePatch.cs",
    ];

    private static List<XElement> Features() {
        List<XElement> defs = [];
        foreach (string mod in new[] { "CosmereCore", "CosmereScadrial", "CosmereRoshar" }) {
            string dir = Path.Combine(RepoRoot, mod, "Defs");
            if (!Directory.Exists(dir)) continue;

            foreach (string path in Directory.GetFiles(dir, "*.xml", SearchOption.AllDirectories)) {
                XElement? root = XDocument.Load(path).Root;
                if (root == null) continue;
                defs.AddRange(root.Descendants("Cosmere.Core.Def.CosmereFeatureDef"));
            }
        }

        return defs;
    }

    [TestMethod]
    public void AmbientContentGatesOnANamedFeature() {
        List<string> offenders = [];
        int seen = 0;

        foreach (string relative in AmbientFiles) {
            string path = Path.Combine(SystemDir, relative.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(path)) {
                offenders.Add($"{relative} is missing - was it moved?");
                continue;
            }

            seen++;
            string source = File.ReadAllText(path);

            if (!source.Contains("FeatureUtility.IsActive", StringComparison.Ordinal)) {
                offenders.Add($"{relative} does not gate on a named feature");
            }

            // belongs in the def now - a bare Shard check here means the world half was forgotten (Honor's spren bug).
            if (source.Contains("ShardUtility.AreAnyEnabled", StringComparison.Ordinal)) {
                offenders.Add($"{relative} still checks a Shard directly");
            }
        }

        Assert.IsTrue(seen > 0, "Checked no files - the paths are wrong, not the code.");
        Assert.AreEqual(0, offenders.Count, string.Join("; ", offenders));
    }

    [TestMethod]
    public void WorldOnlyContentGatesOnTheWorld() {
        List<string> offenders = [];

        foreach (string relative in WorldOnlyFiles) {
            string path = Path.Combine(SystemDir, relative.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(path)) {
                offenders.Add($"{relative} is missing");
                continue;
            }

            string source = File.ReadAllText(path);

            // through the arbiter, not IsActive - true for every world on the sentinel, so load order decided xenotype.
            if (!source.Contains("XenotypeArbiter.MayAnswer", StringComparison.Ordinal)) {
                offenders.Add($"{relative} does not ask the arbiter whether it may answer");
            }

            if (source.Contains("WorldUtility.IsActive", StringComparison.Ordinal)) {
                offenders.Add($"{relative} still gates on IsActive, which cannot resolve a tie");
            }
        }

        Assert.AreEqual(0, offenders.Count, string.Join("; ", offenders));
    }

    /// <summary>
    ///     The draw has to be taken once per pawn, ahead of the patches that read it. Two patches
    ///     each rolling their own would collide exactly as before, just less predictably.
    /// </summary>
    [TestMethod]
    public void TheXenotypeDrawHappensOncePerPawn() {
        string patch = Path.Combine(
            RepoRoot, "CosmereCore", "CosmereCore", "Core", "Patch", "Gene", "XenotypeArbiterPatch.cs"
        );

        Assert.IsTrue(File.Exists(patch), $"Expected the arbiter's head patch at {patch}");

        string source = File.ReadAllText(patch);
        Assert.IsTrue(
            source.Contains("At.Head", StringComparison.Ordinal),
            "The draw has to run at the head, ahead of every At.Return injection on the same method."
        );
        Assert.IsTrue(
            source.Contains("XenotypeArbiter.Draw", StringComparison.Ordinal),
            "Expected the head patch to take the draw."
        );
    }

    /// <summary>
    ///     Every phenomenon names the world it belongs to. Without one it would happen everywhere,
    ///     including on a world that has never heard of it.
    /// </summary>
    [TestMethod]
    public void EveryFeatureNamesItsWorld() {
        List<XElement> features = Features();
        Assert.IsTrue(features.Count > 0, "No CosmereFeatureDefs found - the walk is wrong, or none ship.");

        List<string> offenders = [];
        foreach (XElement feature in features) {
            string name = feature.Element("defName")?.Value.Trim() ?? "(unnamed)";
            if (feature.Element("world") == null) offenders.Add(name);
        }

        Assert.AreEqual(0, offenders.Count, "These features name no world: " + string.Join(", ", offenders));
    }

    /// <summary>
    ///     Highstorms need Honor, mists need one of Scadrial's three. Ashfall is the deliberate
    ///     exception - the Ashmounts are Rashek's engineering, so no Shard is required.
    /// </summary>
    [TestMethod]
    public void ShardCausedFeaturesNameTheirShard() {
        string[] noShardNeeded = ["Cosmere_Feature_Ashfall"];

        List<string> offenders = [];
        foreach (XElement feature in Features()) {
            string name = feature.Element("defName")?.Value.Trim() ?? "(unnamed)";
            if (global::System.Array.IndexOf(noShardNeeded, name) >= 0) continue;

            XElement? shards = feature.Element("anyOfShards");
            bool hasAny = false;
            if (shards != null) {
                foreach (XElement li in shards.Elements("li")) hasAny = true;
            }

            if (!hasAny) offenders.Add(name);
        }

        string detail = "These features name no Shard, so they would still happen on a save where " +
            "the Shard that causes them was switched off: " + string.Join(", ", offenders);

        Assert.AreEqual(0, offenders.Count, detail);
    }

    /// <summary>
    ///     A faction that names its own people outranks the world its pawns stand on.
    /// </summary>
    /// <remarks>
    ///     The arbiter draws a world at random per pawn on a cross-world save, which gave an
    ///     Alethkar soldier even odds of coming out Skaa while Alethkar's own set said darkeyes
    ///     and lighteyes.
    /// </remarks>
    [TestMethod]
    public void TheFactionOutranksTheWorld() {
        List<string> offenders = [];

        foreach (string relative in WorldOnlyFiles) {
            string path = Path.Combine(SystemDir, relative.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(path)) {
                offenders.Add($"{relative} is missing");
                continue;
            }

            string source = File.ReadAllText(path);
            int defers = source.IndexOf("FactionSpeaksForItself", StringComparison.Ordinal);
            int answers = source.IndexOf("MayAnswer", StringComparison.Ordinal);

            if (defers < 0) {
                offenders.Add($"{relative} overrides a faction that named its own people");
            } else if (answers >= 0 && defers > answers) {
                offenders.Add($"{relative} checks the faction after the world, which is too late");
            }
        }

        Assert.AreEqual(0, offenders.Count, string.Join("; ", offenders));
    }

    /// <summary>
    ///     Before a world is chosen, nobody answers. IsActive is permissive about a null world
    ///     and returns true for everything, which let both shards through and left composition
    ///     order to pick the xenotype - the exact tie the arbiter exists to break.
    /// </summary>
    [TestMethod]
    public void NobodyAnswersBeforeAWorldIsChosen() {
        string source = File.ReadAllText(
            Path.Combine(RepoRoot, "CosmereCore", "CosmereCore", "Core", "Util", "XenotypeArbiter.cs")
        );

        int guard = source.IndexOf("if (primary == null) return false;", StringComparison.Ordinal);
        int isActive = source.IndexOf("WorldUtility.IsActive", StringComparison.Ordinal);

        Assert.IsTrue(guard >= 0, "Expected MayAnswer to refuse before a world is set.");
        Assert.IsTrue(
            isActive < 0 || guard < isActive,
            "The null check has to come before IsActive, which answers true for every world."
        );
    }

    /// <summary>
    ///     The cached shard check never invalidated, and MistsWatcher read it cached in one place
    ///     and uncached in another so it could disagree with itself. It must not come back.
    /// </summary>
    [TestMethod]
    public void NoCachedShardGateExists() {
        List<string> offenders = [];
        string coreDir = Path.Combine(RepoRoot, "CosmereCore", "CosmereCore");

        foreach (string path in Directory.GetFiles(coreDir, "*.cs", SearchOption.AllDirectories)) {
            if (File.ReadAllText(path).Contains("CachedAreAnyEnabled", StringComparison.Ordinal)) {
                offenders.Add(Path.GetRelativePath(coreDir, path));
            }
        }

        Assert.AreEqual(0, offenders.Count, "CachedAreAnyEnabled is back: " + string.Join(", ", offenders));
    }
}
