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

    private static XDocument ShapeDefs => XDocument.Load(Path.Combine(
        RepoRoot, "CosmereScadrial", "Defs", "Races", "KandraShape.xml"
    ));

    private static string Generator => Kandra("KandraShapeGenerator.cs");

    /// <summary>
    ///     Only humanlike pawns and colony mechs are ever handed a draft controller, so a form
    ///     meant to take orders cannot be an animal intelligence however much it looks like one.
    /// </summary>
    [TestMethod]
    public void AnAnimalFormIsHumanlikeSoItCanBeDrafted() {
        Assert.IsTrue(
            Generator.Contains("intelligence = Intelligence.Humanlike", StringComparison.Ordinal),
            "An animal intelligence is never handed a draft controller."
        );
    }

    /// <summary>
    ///     Every node in the Animal render tree reads Pawn_AgeTracker.CurKindLifeStage, which
    ///     returns null for humanlike pawns deliberately and logs an error. Our tree has to take
    ///     its graphic from somewhere else entirely.
    /// </summary>
    [TestMethod]
    public void TheFormUsesOurRenderTreeNotTheAnimalOne() {
        Assert.IsTrue(
            Generator.Contains(
                "renderTree = DefDatabase<PawnRenderTreeDef>.GetNamed(\"Cosmere_Scadrial_RenderTree_KandraShape\")",
                StringComparison.Ordinal
            ),
            "The Animal tree cannot draw a humanlike pawn."
        );

        Assert.IsNotNull(
            ShapeDefs.Descendants("PawnRenderTreeDef")
                .FirstOrDefault(d => d.Element("defName")?.Value == "Cosmere_Scadrial_RenderTree_KandraShape"),
            "The generator looks this up by name and throws if it is gone."
        );

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
        Assert.IsTrue(
            Generator.Contains("silhouetteGraphicData = new GraphicData {", StringComparison.Ordinal),
            "A humanlike pawn needs one or RenderPawnAt throws every frame."
        );
        Assert.IsTrue(
            Generator.Contains("lifeStageAges = [new LifeStageAge { def = stage", StringComparison.Ordinal),
            "The generated race has to use the stage carrying that data."
        );
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
        Assert.IsTrue(
            Generator.Contains("new CompProperties_KandraShapePair()]", StringComparison.Ordinal),
            "Without the comp there is nowhere to put the kandra."
        );
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
        XElement hediff = ShapeDefs.Descendants("HediffDef")
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
        XElement hediff = ShapeDefs.Descendants("HediffDef")
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

    /// <summary>
    ///     Release clears the record of what was equipment and what was apparel, so it has to run
    ///     after the gear is handed back. Doing it first put a re-equipped rifle in a pocket.
    /// </summary>
    [TestMethod]
    public void TheGearRecordSurvivesUntilItIsUsed() {
        string shape = Kandra("KandraAnimalShape.cs");
        int unstow = shape.IndexOf("UnstowGear(animal, kandra, pair)", StringComparison.Ordinal);
        int release = shape.IndexOf("pair.Release()", StringComparison.Ordinal);

        Assert.IsTrue(unstow >= 0 && release >= 0);
        Assert.IsTrue(unstow < release, "Release wipes the gear lists; it must come after UnstowGear.");
    }

    /// <summary>
    ///     Bronze reads the Investiture holding a shape together, so practice cannot beat it. A
    ///     first-generation kandra is exactly as visible as one made last week.
    /// </summary>
    [TestMethod]
    public void BronzeIsNotARoll() {
        string disguise = Kandra("KandraDisguise.cs");
        int seen = disguise.IndexOf("public static bool SeenByBronze(", StringComparison.Ordinal);
        Assert.IsTrue(seen >= 0);

        // Stop at the next doc comment so the following member's prose is not read as code.
        int next = disguise.IndexOf("/// <summary>", seen, StringComparison.Ordinal);
        string body = disguise[seen..next];
        Assert.IsFalse(
            body.Contains("Rand.Chance", StringComparison.Ordinal),
            "Bronze does not guess; it either hears the kandra or it does not."
        );
        Assert.IsFalse(
            body.Contains("Conviction", StringComparison.Ordinal),
            "Skill must not help against bronze."
        );
        Assert.IsTrue(body.Contains("IsBurning(bronze)", StringComparison.Ordinal));
    }

    /// <summary>
    ///     Conviction and CanFreeForm were written and never read. An unused difficulty knob is
    ///     the same as no difficulty knob.
    /// </summary>
    [TestMethod]
    public void TheShapeshiftSkillActuallyDoesSomething() {
        string disguise = Kandra("KandraDisguise.cs");
        Assert.IsTrue(
            disguise.Contains("1f - forms.Conviction", StringComparison.Ordinal),
            "Practice should make an ordinary observer less likely to notice."
        );

        string gene = File.ReadAllText(Path.Combine(
            RepoRoot, "CosmereCore", "CosmereCore", "System", "Scadrial", "Gene", "BodyAbsorption.cs"
        ));
        Assert.IsTrue(gene.Contains("forms.CanFreeForm", StringComparison.Ordinal), "CanFreeForm should gate something.");
        Assert.IsTrue(gene.Contains("FreeFormGizmo", StringComparison.Ordinal));
    }

    /// <summary>Being seen has to end the disguise, or nothing was actually at stake.</summary>
    [TestMethod]
    public void BeingCaughtDropsTheShape() {
        string watcher = File.ReadAllText(Path.Combine(
            RepoRoot, "CosmereCore", "CosmereCore", "System", "Scadrial", "Gene", "Shapeshifter.cs"
        ));

        Assert.IsTrue(watcher.Contains("KandraDisguise.SeenByBronze", StringComparison.Ordinal));
        Assert.IsTrue(watcher.Contains("KandraDisguise.Slipped", StringComparison.Ordinal));
        Assert.IsTrue(watcher.Contains("KandraAnimalShape.Revert", StringComparison.Ordinal), "Animal shapes drop too.");
        Assert.IsTrue(watcher.Contains("KandraShapeshift.Revert", StringComparison.Ordinal), "So do worn faces.");
    }

    /// <summary>
    ///     A disguise talks somebody out of a fight. It must never start one, or a kandra in a
    ///     dog becomes a reason for a friendly caravan to open fire.
    /// </summary>
    [TestMethod]
    public void TheDisguiseOnlyEverCalmsThingsDown() {
        string patch = File.ReadAllText(Path.Combine(
            RepoRoot,
            "CosmereCore",
            "CosmereCore",
            "System",
            "Scadrial",
            "Patch",
            "Kandra",
            "DisguiseHostilityPatch.cs"
        ));

        Assert.IsTrue(patch.Contains("if (!ch.ReturnValue) return;", StringComparison.Ordinal));
        Assert.IsTrue(patch.Contains("ch.ReturnValue = false;", StringComparison.Ordinal));
        Assert.IsFalse(
            patch.Contains("ch.ReturnValue = true;", StringComparison.Ordinal),
            "Nothing about wearing a face should make somebody hostile who was not."
        );
    }

    /// <summary>
    ///     Wearing an enemy uniform and shooting the raid from inside it would be free. Cover
    ///     ends the moment the kandra swings, and does not come back until it changes shape.
    /// </summary>
    [TestMethod]
    public void SwingingAtSomebodyEndsTheDisguise() {
        string disguise = Kandra("KandraDisguise.cs");
        Assert.IsTrue(disguise.Contains("forms.FoughtInThisShape(pawn)", StringComparison.Ordinal));
        Assert.IsTrue(disguise.Contains("forms.BlowCover();", StringComparison.Ordinal));

        string comp = Kandra("CompKandraForms.cs");
        Assert.IsTrue(
            comp.Contains("last > wornSinceTick", StringComparison.Ordinal),
            "A fight in a previous shape must not count against the current one."
        );
        Assert.IsTrue(
            comp.Contains("coverBlown = false;", StringComparison.Ordinal),
            "Changing shape is the way out of a blown cover."
        );
    }

    /// <summary>
    ///     The face has to belong to somebody the observer is not already fighting. Wearing a
    ///     colonist in front of a raid should do nothing at all.
    /// </summary>
    [TestMethod]
    public void AHostileFaceFoolsNobody() {
        string disguise = Kandra("KandraDisguise.cs");
        Assert.IsTrue(disguise.Contains("!live.HostileTo(theirs)", StringComparison.Ordinal));
        Assert.IsTrue(
            disguise.Contains("if (theirs == null) return false;", StringComparison.Ordinal),
            "Wildlife and unfactioned things decide hostility on their own terms."
        );
    }

    /// <summary>
    ///     GiveAllShortHashes walks every def in the game and calls Log.Error on each one that
    ///     already has a hash, which by generation time is all of them. That flooded the log with
    ///     thousands of red errors and tripped RimWorld's message limit, which then swallowed
    ///     everything logged afterwards.
    /// </summary>
    [TestMethod]
    public void TheGeneratorHashesOnlyItsOwnDefs() {
        Assert.IsFalse(
            Generator.Contains("ShortHashGiver.GiveAllShortHashes()", StringComparison.Ordinal),
            "One error per existing def is not an acceptable price for three new ones."
        );
        Assert.IsTrue(Generator.Contains("\"GiveShortHash\"", StringComparison.Ordinal));
        Assert.IsTrue(Generator.Contains("Hash(def, typeof(T));", StringComparison.Ordinal));
    }

    /// <summary>
    ///     The generator builds a shape for every animal, wolfhound included. A hand-written one
    ///     alongside it is a second entry in every list and, as it turned out, seven dead sound
    ///     references nobody was looking at.
    /// </summary>
    [TestMethod]
    public void NoHandWrittenShapeCompetesWithTheGeneratedOnes() {
        Assert.AreEqual(0, ShapeDefs.Descendants("ThingDef").Count());
        Assert.AreEqual(0, ShapeDefs.Descendants("PawnKindDef").Count());
        Assert.AreEqual(0, ShapeDefs.Descendants("LifeStageDef").Count());
    }

    /// <summary>
    ///     A generated race built from scratch has none of the comps the BasePawn patch adds, so
    ///     the shape arrived with an Investiture need and nowhere for it to write. Copying the
    ///     need threw, and taking any form failed.
    /// </summary>
    [TestMethod]
    public void AShapeKeepsTheCompsEveryPawnGets() {
        Assert.IsTrue(
            Generator.Contains("comps = [.. source.comps ?? [], new CompProperties_KandraShapePair()]", StringComparison.Ordinal),
            "The animal's comps carry the InvestitureHolder that Investiture.CurLevel writes into."
        );
    }

    /// <summary>
    ///     Animals have no Pawn_GeneTracker, and IsBurning reaches into it without checking. The
    ///     bronze sweep walks every pawn on the map, so it hit the first squirrel it found.
    /// </summary>
    [TestMethod]
    public void TheBronzeSweepSkipsPawnsWithoutGenes() {
        string disguise = Kandra("KandraDisguise.cs");
        int seen = disguise.IndexOf("public static bool SeenByBronze(", StringComparison.Ordinal);
        int next = disguise.IndexOf("/// <summary>", seen, StringComparison.Ordinal);
        string body = disguise[seen..next];

        int guard = body.IndexOf("seeker.genes == null", StringComparison.Ordinal);
        int burning = body.IndexOf("seeker.IsBurning(bronze)", StringComparison.Ordinal);

        Assert.IsTrue(guard >= 0, "Animals have no gene tracker.");
        Assert.IsTrue(guard < burning, "The guard has to come first or it does nothing.");
    }

    /// <summary>
    ///     Humanlike name generation ends in Log.Error when the race is NoName, which every
    ///     animal is. The name is overwritten with the kandra's a moment later either way.
    /// </summary>
    [TestMethod]
    public void AShapeHasSomewhereToGetAName() {
        Assert.IsTrue(Generator.Contains("PawnNameCategory.HumanStandard", StringComparison.Ordinal));
        Assert.IsTrue(Generator.Contains("nameMaker = animal.nameMaker", StringComparison.Ordinal));
    }

    /// <summary>
    ///     Vanilla draws an animal from its GraphicData, which carries the colour, the mask and
    ///     the shader. Rebuilding one from just a texture path dropped all three, and a cougar
    ///     came out white.
    /// </summary>
    [TestMethod]
    public void AShapeIsDrawnInTheAnimalsOwnColour() {
        string node = Kandra("PawnRenderNode_KandraShape.cs");

        Assert.IsTrue(node.Contains("Graphic graphic = data.Graphic;", StringComparison.Ordinal));
        Assert.IsFalse(
            node.Contains("Color.white", StringComparison.Ordinal),
            "Forcing white throws away whatever colour the animal was authored with."
        );
        Assert.IsTrue(Generator.Contains("body = picture,", StringComparison.Ordinal));
    }

    /// <summary>
    ///     The shape is holding a colonist, so the inspect pane needs a colonist's tabs. A race
    ///     built in code has none unless it is told.
    /// </summary>
    [TestMethod]
    public void AShapeKeepsTheColonistTabs() {
        Assert.IsTrue(Generator.Contains("inspectorTabs = HumanTabs()", StringComparison.Ordinal));
        Assert.IsTrue(Generator.Contains("GetNamedSilentFail(\"Human\")", StringComparison.Ordinal));
    }

    /// <summary>
    ///     SkillRecord.Level returns 0 when the skill is currently disabled, and adds aptitude on
    ///     top of what is stored. The animal shape disables twelve work tags, so reading through
    ///     the property and writing it back zeroed those skills on the kandra for good.
    /// </summary>
    [TestMethod]
    public void SteppingOutOfAShapeDoesNotEatSkills() {
        string transfer = Kandra("KandraShapeTransfer.cs");

        Assert.IsTrue(transfer.Contains("target.levelInt = source.levelInt;", StringComparison.Ordinal));
        Assert.IsFalse(
            transfer.Contains("target.Level = source.Level;", StringComparison.Ordinal),
            "The property is lossy in both directions; the backing field is not."
        );
    }
}
