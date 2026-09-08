using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     Guards the koloss appearance wiring. A missing direction here is silent in game: Graphic_Multi
///     substitutes another rotation rather than complaining, so a face draws sideways and nothing logs.
/// </summary>
[TestClass]
public class KolossAppearanceTests {
    private static readonly string[] Directions = ["north", "south", "east"];

    private static string RepoRoot {
        get {
            DirectoryInfo? dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "CosmereScadrial", "Defs"))) {
                dir = dir.Parent;
            }

            Assert.IsNotNull(dir);
            return dir.FullName;
        }
    }

    private static string Textures => Path.Combine(RepoRoot, "CosmereScadrial", "Assets", "Textures");

    private static XDocument Def(params string[] parts) =>
        XDocument.Load(Path.Combine(new[] { RepoRoot, "CosmereScadrial", "Defs" }.Concat(parts).ToArray()));

    private static string AppearanceSource =>
        File.ReadAllText(Path.Combine(
            RepoRoot, "CosmereCore", "CosmereCore", "System", "Scadrial", "Util", "KolossAppearance.cs"
        ));

    private static void AssertRotations(string texPath) {
        foreach (string dir in Directions) {
            string file = Path.Combine(Textures, texPath.Replace('/', Path.DirectorySeparatorChar) + "_" + dir + ".png");
            Assert.IsTrue(File.Exists(file), texPath + " is missing its " + dir + " texture");
        }
    }

    [TestMethod]
    public void ScarRenderNodesHaveEveryRotation() {
        List<string> paths = Def("Races", "Genes", "KolossAppearance.xml")
            .Descendants("texPath")
            .Select(e => e.Value.Trim())
            .ToList();

        Assert.AreEqual(5, paths.Count, "expected three body scar layers and two head scar layers");
        foreach (string path in paths) AssertRotations(path);
    }

    [TestMethod]
    public void EveryKolossHeadTypeHasEveryRotation() {
        List<string> paths = Def("Races", "KolossHeadTypes.xml")
            .Descendants("graphicPath")
            .Select(e => e.Value.Trim())
            .ToList();

        Assert.AreEqual(4, paths.Count, "expected two faces per build");
        foreach (string path in paths) AssertRotations(path);
    }

    [TestMethod]
    public void EveryKolossBodyPathHasEveryRotation() {
        List<string> paths = Regex.Matches(AppearanceSource, "\"(Things/Pawn/Humanlike/Bodies/[^\"]+)\"")
            .Select(m => m.Groups[1].Value)
            .ToList();

        Assert.AreEqual(2, paths.Count, "expected one body texture per build gene");
        foreach (string path in paths) AssertRotations(path);
    }

    [TestMethod]
    public void EveryKolossGeneIconExists() {
        List<string> icons = new[] { "Koloss.xml", "KolossAppearance.xml" }
            .SelectMany(f => Def("Races", "Genes", f).Descendants("iconPath"))
            .Select(e => e.Value.Trim())
            .ToList();

        Assert.IsTrue(icons.Count >= 12, "expected an icon on every koloss gene");
        foreach (string icon in icons) {
            string file = Path.Combine(Textures, icon.Replace('/', Path.DirectorySeparatorChar) + ".png");
            Assert.IsTrue(File.Exists(file), icon + " has no texture");
        }
    }

    /// <summary>
    ///     Heritage carrying a bodyType would pin every koloss to one build and quietly beat the
    ///     build genes, which is the whole mechanism.
    /// </summary>
    [TestMethod]
    public void BuildGenesOwnTheBodyTypeNotHeritage() {
        List<XElement> genes = Def("Races", "Genes", "Koloss.xml").Root!.Elements("GeneDef")
            .Concat(Def("Races", "Genes", "KolossAppearance.xml").Root!.Elements("GeneDef"))
            .ToList();

        XElement heritage = genes.Single(g => g.Element("defName")?.Value == "Cosmere_Scadrial_Gene_KolossHeritage");
        Assert.IsNull(heritage.Element("bodyType"), "heritage must leave the body type to the build genes");

        foreach (string build in new[] { "Young", "Mature" }) {
            XElement gene = genes.Single(g => g.Element("defName")?.Value == "Cosmere_Scadrial_Gene_KolossBuild_" + build);
            Assert.IsNotNull(gene.Element("bodyType"), build + " build gene must carry a body type");
        }
    }

    /// <summary>
    ///     Koloss are bald. The koloss-blooded are people with a big grandfather and keep their
    ///     hair, so the rule has to sit on heritage and nowhere the two genes share.
    /// </summary>
    [TestMethod]
    public void HeritageIsBaldAndKolossBloodedIsNot() {
        List<XElement> genes = Def("Races", "Genes", "Koloss.xml").Root!.Elements("GeneDef").ToList();

        XElement heritage = genes.Single(g => g.Element("defName")?.Value == "Cosmere_Scadrial_Gene_KolossHeritage");
        Assert.AreEqual("Bald", heritage.Element("forcedHair")?.Value, "a koloss grows no hair");
        Assert.IsTrue(
            heritage.Element("beardTagFilter")?.Descendants("li").Any(li => li.Value == "NoBeard") == true,
            "a koloss grows no beard either"
        );

        XElement blooded = genes.Single(g => g.Element("defName")?.Value == "Cosmere_Scadrial_Gene_KolossBlooded");
        Assert.IsNull(blooded.Element("forcedHair"), "the koloss-blooded keep their hair");
        Assert.IsNull(blooded.Element("beardTagFilter"), "the koloss-blooded keep their beards");

        foreach (string call in new[] {
                     "HairDefOf.Bald", "BeardDefOf.NoBeard", "TattooDefOf.NoTattoo_Body", "TattooDefOf.NoTattoo_Face",
                 }) {
            Assert.IsTrue(
                AppearanceSource.Contains(call),
                call + " must also be forced at runtime - PawnGenerator styles a pawn after the gene lands"
            );
        }
    }

    [TestMethod]
    public void GrowthThresholdsClimbInOrder() {
        float Read(string name) {
            Match m = Regex.Match(AppearanceSource, @"\b" + name + @"\s*=\s*([0-9.]+)f");
            Assert.IsTrue(m.Success, name + " is gone");
            return float.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture);
        }

        float low = Read("ScarsLowAt");
        float medium = Read("ScarsMediumAt");
        float heavy = Read("ScarsHeavyAt");

        Assert.AreEqual(Read("MatureAt"), low, "the skin fails at the same point the build changes");
        Assert.IsTrue(low < medium && medium < heavy, "scar thresholds must climb");
        Assert.IsTrue(heavy < 1f, "a koloss must reach failing skin before the growth hediff maxes out");
    }
}
