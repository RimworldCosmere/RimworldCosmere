using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     CosmereWorldDef decides a save's Shards, ancestry floor and content gating. The tests are
///     XML-level because the test project targets net9.0 and deliberately references nothing from
///     Verse, so a Def cannot be constructed here.
/// </summary>
[TestClass]
public class CosmereWorldDefTests {
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

    private static readonly string[] Mods = ["CosmereCore", "CosmereScadrial", "CosmereRoshar"];

    private static List<XElement> DefsNamed(string tag) {
        List<XElement> defs = [];
        foreach (string mod in Mods) {
            string dir = Path.Combine(RepoRoot, mod, "Defs");
            if (!Directory.Exists(dir)) continue;

            foreach (string path in Directory.GetFiles(dir, "*.xml", SearchOption.AllDirectories)) {
                XElement? root = XDocument.Load(path).Root;
                if (root == null) continue;
                defs.AddRange(root.Descendants(tag));
            }
        }

        return defs;
    }

    private static List<XElement> Worlds() => DefsNamed("Cosmere.Core.Def.CosmereWorldDef");

    private static List<string> Items(XElement? list) {
        List<string> values = [];
        if (list == null) return values;
        foreach (XElement li in list.Elements("li")) values.Add(li.Value.Trim());
        return values;
    }

    private static bool IsCrossWorld(XElement world) {
        return (world.Element("crossWorld")?.Value.Trim() ?? "false").ToLowerInvariant() == "true";
    }

    /// <summary>Mutual exclusions, both directions, keyed by shard defName.</summary>
    private static Dictionary<string, HashSet<string>> Exclusions() {
        Dictionary<string, HashSet<string>> map = [];

        foreach (XElement shard in DefsNamed("Cosmere.Core.Def.ShardDef")) {
            string? name = shard.Element("defName")?.Value.Trim();
            if (name == null) continue;

            if (!map.TryGetValue(name, out HashSet<string>? mine)) {
                mine = [];
                map[name] = mine;
            }

            foreach (string other in Items(shard.Element("mutuallyExclusiveWith"))) {
                mine.Add(other);
                if (!map.TryGetValue(other, out HashSet<string>? theirs)) {
                    theirs = [];
                    map[other] = theirs;
                }

                theirs.Add(name);
            }
        }

        return map;
    }

    [TestMethod]
    public void AtLeastOneWorldExists() {
        Assert.IsTrue(Worlds().Count > 0, "No CosmereWorldDef found - the walk is wrong, or none ship.");
    }

    /// <summary>
    ///     The footgun this def type exists to prevent. EnableShard disables everything in
    ///     mutuallyExclusiveWith before adding, so enabling Ruin, Preservation and Harmony in
    ///     sequence leaves Harmony alone, silently, with nothing logged.
    /// </summary>
    [TestMethod]
    public void NoDefaultShardSetIsSelfDestroying() {
        Dictionary<string, HashSet<string>> exclusions = Exclusions();
        Assert.IsTrue(exclusions.Count > 0, "No ShardDefs found - the walk is wrong, not the defs.");

        List<string> offenders = [];

        foreach (XElement world in Worlds()) {
            string name = world.Element("defName")?.Value.Trim() ?? "(unnamed)";

            List<List<string>> sets = [Items(world.Element("fallbackShards"))];
            foreach (XElement entry in world.Element("defaultShardsByEra")?.Elements("li") ?? []) {
                sets.Add(Items(entry.Element("shards")));
            }

            foreach (List<string> set in sets) {
                for (int i = 0; i < set.Count; i++) {
                    for (int j = i + 1; j < set.Count; j++) {
                        if (exclusions.TryGetValue(set[i], out HashSet<string>? conflicts) &&
                            conflicts.Contains(set[j])) {
                            offenders.Add($"{name}: {set[i]} and {set[j]} are mutually exclusive");
                        }
                    }
                }
            }
        }

        string detail = "These worlds enable mutually exclusive Shards in one default set, which " +
            "silently collapses to a single Shard at runtime: " + string.Join("; ", offenders);

        Assert.AreEqual(0, offenders.Count, detail);
    }

    [TestMethod]
    public void EveryDefaultShardIsPermitted() {
        List<string> offenders = [];

        foreach (XElement world in Worlds()) {
            if (IsCrossWorld(world)) continue;

            string name = world.Element("defName")?.Value.Trim() ?? "(unnamed)";
            List<string> permitted = Items(world.Element("nativeShards"));

            List<string> used = [..Items(world.Element("fallbackShards"))];
            foreach (XElement entry in world.Element("defaultShardsByEra")?.Elements("li") ?? []) {
                used.AddRange(Items(entry.Element("shards")));
            }

            foreach (string shard in used) {
                if (!permitted.Contains(shard)) offenders.Add($"{name}: {shard} is not in nativeShards");
            }
        }

        Assert.AreEqual(0, offenders.Count, string.Join("; ", offenders));
    }

    /// <summary>
    ///     A cross-world def must list nothing. It gathers Shards from whatever worlds are
    ///     loaded, so a hard-coded list would dangle when a shard mod is absent.
    /// </summary>
    [TestMethod]
    public void CrossWorldDefsListNoShardsOrXenotypes() {
        List<string> offenders = [];

        foreach (XElement world in Worlds()) {
            if (!IsCrossWorld(world)) continue;

            string name = world.Element("defName")?.Value.Trim() ?? "(unnamed)";
            if (Items(world.Element("nativeShards")).Count > 0) offenders.Add($"{name} lists nativeShards");
            if (Items(world.Element("fallbackShards")).Count > 0) offenders.Add($"{name} lists fallbackShards");
            if (Items(world.Element("xenotypes")).Count > 0) offenders.Add($"{name} lists xenotypes");
        }

        string detail = "A cross-world def that names a Shard from another mod dangles when that " +
            "mod is absent: " + string.Join("; ", offenders);

        Assert.AreEqual(0, offenders.Count, detail);
    }

    /// <summary>Ancestry resolution needs one answer per xenotype, not two.</summary>
    [TestMethod]
    public void NoXenotypeBelongsToTwoWorlds() {
        Dictionary<string, List<string>> owners = [];

        foreach (XElement world in Worlds()) {
            if (IsCrossWorld(world)) continue;

            string name = world.Element("defName")?.Value.Trim() ?? "(unnamed)";
            foreach (string xenotype in Items(world.Element("xenotypes"))) {
                if (!owners.TryGetValue(xenotype, out List<string>? list)) {
                    list = [];
                    owners[xenotype] = list;
                }

                list.Add(name);
            }
        }

        List<string> offenders = [];
        foreach (KeyValuePair<string, List<string>> pair in owners) {
            if (pair.Value.Count > 1) offenders.Add($"{pair.Key} claimed by {string.Join(" and ", pair.Value)}");
        }

        Assert.AreEqual(0, offenders.Count, string.Join("; ", offenders));
    }

    /// <summary>Every xenotype a world claims has to actually exist.</summary>
    [TestMethod]
    public void EveryClaimedXenotypeIsOneWeShip() {
        HashSet<string> shipped = [];
        foreach (XElement def in DefsNamed("XenotypeDef")) {
            string? name = def.Element("defName")?.Value.Trim();
            if (name != null) shipped.Add(name);
        }

        Assert.IsTrue(shipped.Count > 0, "No XenotypeDefs found - the walk is wrong, not the defs.");

        List<string> offenders = [];
        foreach (XElement world in Worlds()) {
            string name = world.Element("defName")?.Value.Trim() ?? "(unnamed)";
            foreach (string xenotype in Items(world.Element("xenotypes"))) {
                if (!shipped.Contains(xenotype)) offenders.Add($"{name} claims '{xenotype}'");
            }
        }

        Assert.AreEqual(0, offenders.Count, string.Join("; ", offenders));
    }

    /// <summary>
    ///     The regression baseline. A locked scenario's declared Shards stay authoritative - the
    ///     world does not have to reproduce them - but every one must be permitted by some world
    ///     and internally conflict-free, or enabling the set in sequence loses members.
    /// </summary>
    [TestMethod]
    public void EveryScenarioShardSetIsPermittedAndConflictFree() {
        HashSet<string> permittedAnywhere = [];
        foreach (XElement world in Worlds()) {
            foreach (string shard in Items(world.Element("nativeShards"))) permittedAnywhere.Add(shard);
        }

        Dictionary<string, HashSet<string>> exclusions = Exclusions();
        List<string> offenders = [];
        int seen = 0;

        foreach (string mod in Mods) {
            string dir = Path.Combine(RepoRoot, mod, "Defs", "Scenarios");
            if (!Directory.Exists(dir)) continue;

            foreach (string path in Directory.GetFiles(dir, "*.xml")) {
                XElement? root = XDocument.Load(path).Root;
                if (root == null) continue;

                foreach (XElement ext in root.Descendants("li")) {
                    if ((string?)ext.Attribute("Class") != "Cosmere.Core.DefModExtension.Shards") continue;

                    List<string> declared = Items(ext.Element("shards"));
                    if (declared.Count == 0) continue;

                    seen++;
                    string file = Path.GetFileName(path);

                    foreach (string shard in declared) {
                        if (!permittedAnywhere.Contains(shard)) {
                            offenders.Add($"{file}: '{shard}' is permitted by no world");
                        }
                    }

                    for (int i = 0; i < declared.Count; i++) {
                        for (int j = i + 1; j < declared.Count; j++) {
                            if (exclusions.TryGetValue(declared[i], out HashSet<string>? conflicts) &&
                                conflicts.Contains(declared[j])) {
                                offenders.Add($"{file}: {declared[i]} and {declared[j]} are mutually exclusive");
                            }
                        }
                    }
                }
            }
        }

        Assert.IsTrue(seen > 0, "Found no scenario shard declarations - the walk is wrong, not the defs.");
        Assert.AreEqual(0, offenders.Count, string.Join("; ", offenders));
    }
}
