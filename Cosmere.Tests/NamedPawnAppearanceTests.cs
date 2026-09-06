using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     Guards that a named story pawn comes out looking like the person it names.
/// </summary>
[TestClass]
public class NamedPawnAppearanceTests {
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

    private static string Source => File.ReadAllText(
        Path.Combine(RepoRoot, "CosmereCore", "CosmereCore", "Core", "ScenarioPart", "Parts", "ScenPart_NamedPawns.cs")
    );

    /// <summary>
    ///     Setting gender after generation flips the label and leaves the body and head that were
    ///     rolled. Kelsier came out a woman that way. The template has to correct both.
    /// </summary>
    [TestMethod]
    public void SettingGenderAlsoCorrectsBodyAndHead() {
        string source = Source;

        int start = source.IndexOf("private static void ApplyAppearance(", StringComparison.Ordinal);
        Assert.IsTrue(start >= 0, "ApplyAppearance is missing.");

        string body = source[start..];
        Assert.IsTrue(body.Contains("pawn.story.bodyType = body", StringComparison.Ordinal));
        Assert.IsTrue(body.Contains("pawn.story.headType = head", StringComparison.Ordinal));

        // filter moved into HeadTypeUtility, which also drops offered heads - old filter gave Sazed Stump.
        Assert.IsTrue(
            body.Contains("HeadTypeUtility.RandomFor(template.gender", StringComparison.Ordinal),
            "A replacement head must match the template's gender."
        );

        int apply = source.IndexOf("private void ApplyTemplate(", StringComparison.Ordinal);
        Assert.IsTrue(
            source[apply..].Contains("ApplyAppearance(pawn, template)", StringComparison.Ordinal),
            "ApplyTemplate must run the appearance pass."
        );
    }

    /// <summary>
    ///     Every named pawn declares a gender, which is what the body and head correction keys
    ///     off. One without it would silently keep whatever was rolled.
    /// </summary>
    [TestMethod]
    public void EveryNamedPawnDeclaresAGender() {
        foreach (string mod in new[] { "CosmereScadrial", "CosmereRoshar" }) {
            string dir = Path.Combine(RepoRoot, mod, "Defs", "Scenarios");
            if (!Directory.Exists(dir)) continue;

            foreach (string file in Directory.GetFiles(dir, "*.xml")) {
                XDocument doc = XDocument.Load(file);
                XElement? named = doc.Descendants("li")
                    .FirstOrDefault(li => (string?)li.Attribute("Class")
                        == "Cosmere.Core.ScenarioPart.Parts.ScenPart_NamedPawns");
                if (named == null) continue;

                foreach (XElement pawn in named.Element("pawns")?.Elements("li") ?? []) {
                    string? name = pawn.Element("firstName")?.Value;
                    if (name == null) continue;

                    Assert.IsNotNull(
                        pawn.Element("gender")?.Value,
                        $"{name} in {Path.GetFileName(file)} declares no gender."
                    );
                }
            }
        }
    }

    /// <summary>
    ///     Hair and beards carry a style gender of their own, so correcting only the body and the
    ///     head leaves a man in a woman's haircut. Ham stayed that way through the first fix.
    /// </summary>
    [TestMethod]
    public void HairAndBeardsAlsoFollowTheDeclaredGender() {
        string source = Source;
        int start = source.IndexOf("private static void ApplyAppearance(", StringComparison.Ordinal);
        Assert.IsTrue(start >= 0);

        string body = source[start..];
        Assert.IsTrue(
            body.Contains("pawn.story.hairDef =", StringComparison.Ordinal),
            "A contradicting haircut must be replaced."
        );
        Assert.IsTrue(
            body.Contains("beardDef = BeardDefOf.NoBeard", StringComparison.Ordinal),
            "A woman should not keep a rolled beard."
        );
        Assert.IsTrue(
            source.Contains("private static bool SuitsGender(", StringComparison.Ordinal),
            "Style gender needs one place that decides what suits."
        );
    }

    /// <summary>
    ///     Terris are lean, never heavyset, so they take the ordinary body for their gender.
    /// </summary>
    /// <remarks>
    ///     A gene's bodyType field forces one def on everyone carrying it, which would put every
    ///     Terris woman in a male body. The rule needs code, and it has to reach every Terris
    ///     pawn rather than only the named ones.
    /// </remarks>
    [TestMethod]
    public void TerrisAlwaysTakeTheNormalBody() {
        XDocument doc = XDocument.Load(
            Path.Combine(RepoRoot, "CosmereScadrial", "Defs", "Races", "Genes", "Terris.xml")
        );

        XElement heritage = doc.Descendants("GeneDef")
            .First(g => g.Element("defName")?.Value == "Cosmere_Scadrial_Gene_TerrisHeritage");

        Assert.AreEqual(
            "Cosmere.System.Scadrial.Gene.NormalBodyType",
            heritage.Element("geneClass")?.Value
        );

        Assert.IsNull(
            heritage.Element("bodyType"),
            "A fixed bodyType would put every Terris woman in a male body."
        );

        string gene = File.ReadAllText(Path.Combine(
            RepoRoot, "CosmereCore", "CosmereCore", "System", "Scadrial", "Gene", "NormalBodyType.cs"
        ));
        Assert.IsTrue(gene.Contains("BodyTypeDefOf.Female", StringComparison.Ordinal));
        Assert.IsTrue(gene.Contains("BodyTypeDefOf.Male", StringComparison.Ordinal));
    }
}
