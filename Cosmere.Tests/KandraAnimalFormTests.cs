using System;
using System.Collections.Generic;
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

    /// <summary>
    ///     Stepping out of a shape must not clear the kandra's hediffs.
    /// </summary>
    /// <remarks>
    ///     Into() wipes the target first, which is right for a freshly generated animal carrying
    ///     its own scars. Running the same thing in reverse would wipe the kandra's spikes, which
    ///     are anchored to its torso, and take the Blessing and its mind with them.
    /// </remarks>
    [TestMethod]
    public void LeavingAShapeDoesNotWipeTheKandra() {
        string transfer = Kandra("KandraShapeTransfer.cs");

        int outOf = transfer.IndexOf("public static void OutOf(", StringComparison.Ordinal);
        int intoEnd = transfer.IndexOf("private static void Identity(", StringComparison.Ordinal);
        Assert.IsTrue(outOf >= 0 && intoEnd > outOf);

        string body = transfer[outOf..intoEnd];
        Assert.IsFalse(
            body.Contains("Hediffs(", StringComparison.Ordinal),
            "OutOf must not touch hediffs; it would remove the kandra's spikes."
        );
        Assert.IsTrue(body.Contains("Skills(", StringComparison.Ordinal));

        string shape = Kandra("KandraAnimalShape.cs");
        Assert.IsTrue(shape.Contains("KandraShapeTransfer.Into(kandra, animal)", StringComparison.Ordinal));
        Assert.IsTrue(shape.Contains("KandraShapeTransfer.OutOf(animal, kandra)", StringComparison.Ordinal));
    }

    /// <summary>
    ///     Losing the selection mid-shapeshift means hunting the map for your own colonist.
    /// </summary>
    [TestMethod]
    public void TheSelectionSurvivesBothTransitions() {
        string shape = Kandra("KandraAnimalShape.cs");
        Assert.AreEqual(
            2,
            shape.Split("Find.Selector.IsSelected").Length - 1,
            "Both directions should remember whether the pawn was selected."
        );
        Assert.AreEqual(
            2,
            shape.Split("Find.Selector.Select").Length - 1,
            "Both directions should reselect the pawn that replaced it."
        );
    }

    /// <summary>
    ///     A body part record belongs to one body. Carrying a part-anchored hediff into another
    ///     would point it at nothing.
    /// </summary>
    [TestMethod]
    public void OnlyWholeBodyHediffsTravel() {
        string transfer = Kandra("KandraShapeTransfer.cs");
        int start = transfer.IndexOf("private static void Hediffs(", StringComparison.Ordinal);
        Assert.IsTrue(start >= 0);

        string body = transfer[start..];
        Assert.IsTrue(body.Contains("hediff.Part != null", StringComparison.Ordinal));
        Assert.IsTrue(body.Contains("Hediff_Injury", StringComparison.Ordinal), "Injuries stay with the body.");
    }

    /// <summary>
    ///     Pawn_RecordsTracker.AddTo refuses Time records outright and logs an error, so copying
    ///     every record blindly threw on TimeAsColonistOrColonyAnimal.
    /// </summary>
    [TestMethod]
    public void TimeRecordsAreNotCopied() {
        string transfer = Kandra("KandraShapeTransfer.cs");
        int start = transfer.IndexOf("private static void Records(", StringComparison.Ordinal);
        Assert.IsTrue(start >= 0);

        Assert.IsTrue(
            transfer[start..].Contains("RecordType.Time", StringComparison.Ordinal),
            "Time records have to be skipped; AddTo rejects them."
        );
    }

    /// <summary>
    ///     A humanlike shape rolls its own xenotype at generation, so a kandra came out a Skaa
    ///     wolfhound. It is still a kandra whatever it is wearing.
    /// </summary>
    [TestMethod]
    public void TheShapeKeepsTheKandrasXenotype() {
        string transfer = Kandra("KandraShapeTransfer.cs");
        Assert.IsTrue(transfer.Contains("SetXenotypeDirect(from.genes.Xenotype)", StringComparison.Ordinal));
    }

    /// <summary>
    ///     Taking a pawn off the map clears the selection, so it has to be read before despawning
    ///     or the answer is always no.
    /// </summary>
    [TestMethod]
    public void SelectionIsReadBeforeTheKandraLeavesTheMap() {
        string shape = Kandra("KandraAnimalShape.cs");
        int selected = shape.IndexOf("bool wasSelected = Find.Selector.IsSelected(kandra)", StringComparison.Ordinal);
        int despawn = shape.IndexOf("kandra.DeSpawn()", StringComparison.Ordinal);

        Assert.IsTrue(selected >= 0 && despawn >= 0);
        Assert.IsTrue(selected < despawn, "Selection must be read before the pawn is despawned.");
    }

    /// <summary>A body with no hands cannot do work that needs them.</summary>
    [TestMethod]
    public void AnAnimalShapeCannotDoHandiwork() {
        XElement hediff = Wolfhound.Descendants("HediffDef")
            .First(d => d.Element("defName")?.Value == "Cosmere_Scadrial_Hediff_AnimalShape");

        List<string> disabled = hediff.Element("stages")!.Elements("li")
            .SelectMany(stage => stage.Element("disabledWorkTags")?.Elements("li") ?? [])
            .Select(li => li.Value)
            .ToList();

        foreach (string tag in new[] { "Crafting", "Constructing", "Cooking", "Caring", "Intellectual" }) {
            CollectionAssert.Contains(disabled, tag, $"An animal shape should not be able to do {tag} work.");
        }

        string shape = Kandra("KandraAnimalShape.cs");
        Assert.IsTrue(shape.Contains("Cosmere_Scadrial_Hediff_AnimalShape", StringComparison.Ordinal));
    }

    /// <summary>A dog cannot fire a rifle or negotiate a trade deal.</summary>
    [TestMethod]
    public void AnAnimalShapeCannotShootOrTalkPeopleRound() {
        XElement hediff = Wolfhound.Descendants("HediffDef")
            .First(d => d.Element("defName")?.Value == "Cosmere_Scadrial_Hediff_AnimalShape");

        List<string> disabled = hediff.Element("stages")!.Elements("li")
            .SelectMany(stage => stage.Element("disabledWorkTags")?.Elements("li") ?? [])
            .Select(li => li.Value)
            .ToList();

        CollectionAssert.Contains(disabled, "Shooting");
        CollectionAssert.Contains(disabled, "Social");
    }

    /// <summary>
    ///     Gear goes into the pack rather than being left on the floor, and comes back the way it
    ///     was carried. Anything dropped while a dog stays dropped, which is the point of putting
    ///     it somewhere droppable.
    /// </summary>
    [TestMethod]
    public void GearRidesAlongAndReturnsAsItLeft() {
        string shape = Kandra("KandraAnimalShape.cs");

        Assert.IsTrue(shape.Contains("private static void StowGear(", StringComparison.Ordinal));
        Assert.IsTrue(shape.Contains("private static void UnstowGear(", StringComparison.Ordinal));

        int unstow = shape.IndexOf("private static void UnstowGear(", StringComparison.Ordinal);
        string body = shape[unstow..];

        Assert.IsTrue(
            body.Contains("innerContainer.Contains(equipped[i])", StringComparison.Ordinal),
            "Only gear still in the pack comes back; dropped things stay dropped."
        );
        Assert.IsTrue(body.Contains("AddEquipment(weapon)", StringComparison.Ordinal));
        Assert.IsTrue(body.Contains("apparel?.Wear(clothing", StringComparison.Ordinal));

        string pair = Kandra("CompKandraShapePair.cs");
        Assert.IsTrue(
            pair.Contains("LookMode.Reference", StringComparison.Ordinal),
            "The remembered gear lists point at things that live in the pack."
        );
    }

    /// <summary>
    ///     Apparel needs a floor to be taken off onto, and a despawned pawn has no map. The gear
    ///     has to move while the kandra is still standing there.
    /// </summary>
    [TestMethod]
    public void GearMovesBeforeTheKandraLeavesTheMap() {
        string shape = Kandra("KandraAnimalShape.cs");
        int stow = shape.IndexOf("StowGear(kandra, animal, pair)", StringComparison.Ordinal);
        int despawn = shape.IndexOf("kandra.DeSpawn()", StringComparison.Ordinal);

        Assert.IsTrue(stow >= 0 && despawn >= 0);
        Assert.IsTrue(stow < despawn, "Gear must move while the kandra is still on the map.");
    }
}
