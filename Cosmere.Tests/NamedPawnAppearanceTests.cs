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
            return dir!.FullName;
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
        Assert.IsTrue(
            body.Contains("candidate.gender == template.gender", StringComparison.Ordinal),
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
}
