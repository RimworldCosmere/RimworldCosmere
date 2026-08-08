using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     Guards the kandra animal forms, and the two RimWorld facts that shape them.
/// </summary>
[TestClass]
public class KandraAnimalFormTests {
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

    private static string Kandra(string file) => File.ReadAllText(Path.Combine(
        RepoRoot, "CosmereCore", "CosmereCore", "System", "Scadrial", "Kandra", file
    ));

    private static XDocument Wolfhound => XDocument.Load(Path.Combine(
        RepoRoot, "CosmereScadrial", "Defs", "Races", "KandraWolfhound.xml"
    ));

    /// <summary>
    ///     Only humanlike pawns and colony mechs are ever handed a draft controller, so a form
    ///     meant to take orders cannot be an animal intelligence however much it looks like one.
    /// </summary>
    [TestMethod]
    public void AnAnimalFormIsHumanlikeSoItCanBeDrafted() {
        XElement race = Wolfhound.Descendants("ThingDef")
            .First(d => d.Element("defName")?.Value == "Cosmere_Scadrial_Race_KandraWolfhound");

        Assert.AreEqual("Humanlike", race.Element("race")?.Element("intelligence")?.Value);
    }

    /// <summary>
    ///     Every node in the Animal render tree reads Pawn_AgeTracker.CurKindLifeStage, which
    ///     returns null for humanlike pawns deliberately and logs an error. Our tree has to take
    ///     its graphic from somewhere else entirely.
    /// </summary>
    [TestMethod]
    public void TheFormUsesOurRenderTreeNotTheAnimalOne() {
        XElement race = Wolfhound.Descendants("ThingDef")
            .First(d => d.Element("defName")?.Value == "Cosmere_Scadrial_Race_KandraWolfhound");

        string? tree = race.Element("race")?.Element("renderTree")?.Value;
        Assert.AreNotEqual("Animal", tree, "The Animal tree cannot draw a humanlike pawn.");
        Assert.AreEqual("Cosmere_Scadrial_RenderTree_KandraShape", tree);

        string node = Kandra("PawnRenderNode_KandraShape.cs");

        // Only the body. The doc comment names CurKindLifeStage to explain why it is avoided.
        int body = node.IndexOf("public override Graphic? GraphicFor(", StringComparison.Ordinal);
        Assert.IsTrue(body >= 0, "GraphicFor is missing.");

        Assert.IsFalse(
            node[body..].Contains("CurKindLifeStage", StringComparison.Ordinal),
            "The node must not touch the age tracker; that is the whole reason it exists."
        );
        Assert.IsTrue(node.Contains("GetModExtension<KandraShapeGraphic>", StringComparison.Ordinal));
    }

    /// <summary>
    ///     PawnRenderer.RenderPawnAt reads CurLifeStage.silhouetteGraphicData unguarded for any
    ///     humanlike pawn. The animal life stages have none, which threw once a frame.
    /// </summary>
    [TestMethod]
    public void TheFormsLifeStageCarriesSilhouetteData() {
        XElement stage = Wolfhound.Descendants("LifeStageDef")
            .First(d => d.Element("defName")?.Value == "Cosmere_Scadrial_LifeStage_KandraShape");

        Assert.IsNotNull(stage.Element("silhouetteGraphicData"), "A humanlike pawn needs one or it throws every frame.");

        XElement race = Wolfhound.Descendants("ThingDef")
            .First(d => d.Element("defName")?.Value == "Cosmere_Scadrial_Race_KandraWolfhound");
        string? used = race.Element("race")?.Element("lifeStageAges")?.Elements("li").First().Element("def")?.Value;
        Assert.AreEqual("Cosmere_Scadrial_LifeStage_KandraShape", used);
    }

    /// <summary>
    ///     The kandra is despawned rather than destroyed while its animal walks around, so
    ///     everything about it survives the trip. Losing that pawn loses the colonist.
    /// </summary>
    [TestMethod]
    public void TheKandraIsHeldNotDestroyedWhileWearingAnAnimal() {
        string shape = Kandra("KandraAnimalShape.cs");
        Assert.IsTrue(shape.Contains("kandra.DeSpawn()", StringComparison.Ordinal));
        Assert.IsTrue(shape.Contains("pair.Hold(kandra)", StringComparison.Ordinal));
        Assert.IsFalse(
            shape.Contains("kandra.Destroy()", StringComparison.Ordinal),
            "Destroying the kandra would delete the colonist."
        );

        string pair = Kandra("CompKandraShapePair.cs");
        Assert.IsTrue(pair.Contains("Scribe_Deep.Look(ref held", StringComparison.Ordinal), "It has to survive a save.");
        Assert.IsTrue(
            pair.Contains("Notify_Killed", StringComparison.Ordinal),
            "Killing the animal must not silently delete the person inside it."
        );
    }

    /// <summary>The animal needs the comp, or wearing it would strand the kandra.</summary>
    [TestMethod]
    public void TheFormCarriesTheShapePairComp() {
        XElement race = Wolfhound.Descendants("ThingDef")
            .First(d => d.Element("defName")?.Value == "Cosmere_Scadrial_Race_KandraWolfhound");

        bool has = race.Element("comps")?.Elements("li")
            .Any(li => (string?)li.Attribute("Class")
                == "Cosmere.System.Scadrial.Kandra.CompProperties_KandraShapePair") == true;

        Assert.IsTrue(has, "Without the comp there is nowhere to put the kandra.");
    }
}
