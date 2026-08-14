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
    ///     Ancestry is the only thing that grants a connection floor and it keys off the xenotype.
    ///     Kandra and koloss were missing from Scadrial's list, so both read as Connected to
    ///     nothing at all.
    /// </summary>
    [TestMethod]
    public void EveryScadrianXenotypeBelongsToScadrial() {
        XDocument worlds = XDocument.Load(Path.Combine(RepoRoot, "CosmereScadrial", "Defs", "Worlds.xml"));
        List<string> listed = worlds.Descendants("xenotypes")
            .Elements("li")
            .Select(e => e.Value)
            .ToList();

        Assert.IsTrue(listed.Contains("Cosmere_Scadrial_Xenotype_Kandra"));
        Assert.IsTrue(listed.Contains("Cosmere_Scadrial_Xenotype_Koloss"));
    }

    /// <summary>
    ///     There are 117 animals and one hediff, so the stage is built from the worn animal at
    ///     runtime. HediffStage has no IExposable, so nothing about it survives a save and it has
    ///     to be rebuilt on load - miss that and a shaped kandra loads with human stats silently.
    /// </summary>
    [TestMethod]
    public void TheShapeHediffSynthesisesTheAnimalsNumbers() {
        string hediff = Kandra("Hediff_KandraAnimalShape.cs");

        Assert.IsTrue(hediff.Contains("public override HediffStage? CurStage", StringComparison.Ordinal));
        Assert.IsTrue(hediff.Contains("RimWorld.StatDefOf.MoveSpeed", StringComparison.Ordinal));
        Assert.IsTrue(hediff.Contains("ComfyTemperatureMin", StringComparison.Ordinal));
        Assert.IsTrue(hediff.Contains("hungerRateFactor", StringComparison.Ordinal));
        Assert.IsTrue(
            hediff.Contains("LoadSaveMode.PostLoadInit", StringComparison.Ordinal),
            "The cached stage is not saved, so it must be dropped and rebuilt after load."
        );
        Assert.IsTrue(
            hediff.Contains("builtFor == worn", StringComparison.Ordinal),
            "CurStage is hit once per stat lookup; it must not rebuild every time."
        );

        XDocument defs = XDocument.Load(Path.Combine(
            RepoRoot, "CosmereScadrial", "Defs", "Races", "KandraShape.xml"
        ));
        XElement shape = defs.Descendants("HediffDef")
            .First(d => d.Element("defName")?.Value == "Cosmere_Scadrial_Hediff_AnimalShape");
        Assert.AreEqual(
            "Cosmere.System.Scadrial.Kandra.Hediff_KandraAnimalShape",
            shape.Element("hediffClass")?.Value
        );
    }

    /// <summary>
    ///     Notify_DisabledWorkTypesChanged calls SetPriority(w, 0) for every newly disabled work
    ///     type and RimWorld has no inverse, so twelve columns of the Work tab would go blank on
    ///     every shape and stay blank.
    /// </summary>
    [TestMethod]
    public void ShapingDoesNotEatTheWorkTab() {
        string comp = Kandra("CompKandraForms.cs");

        Assert.IsTrue(comp.Contains("RememberWorkPriorities", StringComparison.Ordinal));
        Assert.IsTrue(comp.Contains("RestoreWorkPriorities", StringComparison.Ordinal));
        Assert.IsTrue(
            comp.Contains("if (pawn.WorkTypeIsDisabled(remembered.Key)) continue;", StringComparison.Ordinal),
            "SetPriority logs an error for a work type that is still disabled, so restore comes after removal."
        );
        Assert.IsTrue(
            comp.Contains("Scribe_Collections.Look(ref workPriorities", StringComparison.Ordinal),
            "A kandra saved mid-shape must still get its work tab back."
        );
    }

    /// <summary>A wolf has paws and full Manipulation; halving melee hit chance was not intended.</summary>
    [TestMethod]
    public void AShapeFightsLikeTheAnimal() {
        XDocument defs = XDocument.Load(Path.Combine(
            RepoRoot, "CosmereScadrial", "Defs", "Races", "KandraShape.xml"
        ));
        XElement manipulation = defs.Descendants("li")
            .First(li => li.Element("capacity")?.Value == "Manipulation");

        Assert.IsTrue(float.Parse(manipulation.Element("setMax")!.Value) >= 0.9f);
    }

    /// <summary>
    ///     DynamicPawnRenderNodeSetup_Hediffs skips any hediff whose Visible is false, so hiding
    ///     the shape hediff from the health tab also deletes its render node and the pawn draws as
    ///     nothing at all.
    /// </summary>
    [TestMethod]
    public void TheShapeHediffStaysVisibleOrNothingDraws() {
        string hediff = Kandra("Hediff_KandraAnimalShape.cs");

        Assert.IsTrue(hediff.Contains("becomeVisible = true", StringComparison.Ordinal));
        Assert.IsFalse(
            hediff.Contains("becomeVisible = false", StringComparison.Ordinal),
            "An invisible hediff has no render node, and the kandra vanishes."
        );
    }

    /// <summary>
    ///     VerbProperties.GetDamageFactorFor returns 0 for a body part group the body does not
    ///     have, and Verb.IsStillUsableBy then drops the verb silently - so a wolf's paw attack on
    ///     a human body would never once land.
    /// </summary>
    [TestMethod]
    public void TheShapesTeethAttachToSomethingTheBodyHas() {
        string verbs = Kandra("HediffComp_KandraShapeVerbs.cs");

        Assert.IsTrue(verbs.Contains("linkedBodyPartsGroup = Bite", StringComparison.Ordinal));
        Assert.IsFalse(
            verbs.Contains("FrontLeftPaw", StringComparison.Ordinal),
            "A human body has no paws, and the verb would be dropped without a word."
        );
        Assert.IsTrue(
            verbs.Contains("verbTracker = new VerbTracker(this)", StringComparison.Ordinal),
            "VerbTracker caches its list, so a kandra would keep biting like the first shape it wore."
        );
        Assert.IsTrue(
            verbs.Contains(": HediffComp_VerbGiver, IVerbOwner", StringComparison.Ordinal),
            "Re-declaring the interface is what re-maps Tools away from the def's copy. VerbTracker"
            + " reads directOwner.Tools through IVerbOwner, so the subclass wins."
        );

        // HediffComp_VerbGiver.Props hard-casts to HediffCompProperties_VerbGiver, and
        // VerbProperties reads through it. The wrong base throws InvalidCastException every time
        // anything asks the pawn for a melee verb.
        Assert.IsTrue(
            verbs.Contains("HediffCompProperties_KandraShapeVerbs : HediffCompProperties_VerbGiver", StringComparison.Ordinal),
            "Deriving from plain HediffCompProperties makes every melee lookup throw."
        );
    }

    /// <summary>
    ///     Pawn_WorkSettings.GetPriority returns a flat 3 for any humanlike pawn with a non-zero
    ///     priority while Find.PlaySettings.useWorkPriorities is off, which is the default. Reading
    ///     through it stores 3 for everything and hands 3 back, flattening a player's tuned 1s and
    ///     4s the first time their kandra changes shape - invisibly, until they turn manual
    ///     priorities back on.
    /// </summary>
    [TestMethod]
    public void TheWorkSnapshotReadsTheRealPrioritiesNotTheDisplayedOnes() {
        string comp = Kandra("CompKandraForms.cs");

        Assert.IsTrue(comp.Contains("StoredPriorities(pawn)", StringComparison.Ordinal));
        Assert.IsFalse(
            comp.Contains("pawn.workSettings.GetPriority(", StringComparison.Ordinal),
            "GetPriority lies while manual priorities are off."
        );
        Assert.IsTrue(
            comp.Contains("if (workPriorities.Count > 0) return;", StringComparison.Ordinal),
            "Shaping twice without reverting would snapshot the already-zeroed tab."
        );
    }

    /// <summary>
    ///     A human fist at power 8.2 survives VerbUtility's 25%-of-best-DPS prune against a wolf
    ///     bite at power 12, so a shaped kandra punches about as often as it bites and the combat
    ///     log says so.
    /// </summary>
    [TestMethod]
    public void AShapedKandraFightsWithTheAnimalsMouthOnly() {
        string patch = File.ReadAllText(Path.Combine(
            RepoRoot,
            "CosmereCore",
            "CosmereCore",
            "System",
            "Scadrial",
            "Patch",
            "Kandra",
            "ShapedMeleeVerbsPatch.cs"
        ));

        Assert.IsTrue(patch.Contains("entries.RemoveAll(e => e.verb?.DirectOwner is Verse.Pawn)", StringComparison.Ordinal));
        Assert.IsTrue(
            patch.Contains("if (borrowed == 0) return;", StringComparison.Ordinal),
            "Stripping the last melee verb makes ChooseMeleeVerb log an error on every swing."
        );
    }

    /// <summary>
    ///     The mental-break warning icon is parented under Head, so vetoing Head to hide the human
    ///     face also hides the warning on exactly the colonist least likely to be watched.
    /// </summary>
    [TestMethod]
    public void TheBreakWarningSurvivesBeingShaped() {
        XDocument patch = XDocument.Load(Path.Combine(
            RepoRoot, "CosmereScadrial", "Patches", "KandraHumanlikeRenderTree.xml"
        ));

        Assert.IsTrue(
            patch.Descendants("Operation")
                .Any(o => (string?)o.Attribute("Class") == "PatchOperationRemove"
                          && o.Element("xpath")!.Value.Contains("Status overlay")),
            "It has to come out from under Head."
        );
        Assert.IsTrue(
            patch.Descendants("li").Any(li => li.Element("debugLabel")?.Value == "Status overlay"),
            "And go back on at the root, or the warning is simply gone."
        );
    }

    /// <summary>
    ///     BattleLogEntry_MeleeCombat builds its grammar from the verb owner's
    ///     ImplementOwnerTypeDef, and HediffComp_VerbGiver reports Hediff - which produced
    ///     "OreSeur, wielding his wearing an animal deftly, nipped the turkey". Bodypart is what a
    ///     real animal's bite uses and drops the wielding clause.
    /// </summary>
    [TestMethod]
    public void TheShapesBiteReadsAsABodyPart() {
        string verbs = Kandra("HediffComp_KandraShapeVerbs.cs");

        Assert.IsTrue(
            verbs.Contains("ImplementOwnerTypeDef => ImplementOwnerTypeDefOf.Bodypart", StringComparison.Ordinal),
            "Otherwise the combat log names the hediff as if it were a weapon."
        );
    }

    /// <summary>
    ///     Vanilla draws an animal from its GraphicData, which carries the colour, the mask and
    ///     the shader. Rebuilding one from just a texture path dropped all three and a cougar came
    ///     out white.
    /// </summary>
    [TestMethod]
    public void AShapeIsDrawnInTheAnimalsOwnColour() {
        string node = Kandra("PawnRenderNode_KandraShape.cs");

        Assert.IsTrue(node.Contains("Graphic graphic = data.Graphic;", StringComparison.Ordinal));
        Assert.IsFalse(
            node.Contains("Color.white", StringComparison.Ordinal),
            "Forcing white throws away whatever colour the animal was authored with."
        );
    }

    /// <summary>
    ///     PawnRenderNode.MeshSetFor returns a fixed 1.5 by 1.5 human body quad from the pool, so
    ///     the graphic's own drawSize is never consulted and a bluebird authored at 0.6 was
    ///     stretched over a person.
    /// </summary>
    [TestMethod]
    public void AShapeIsTheSameSizeAsTheAnimal() {
        string node = Kandra("PawnRenderNode_KandraShape.cs");

        Assert.IsTrue(node.Contains("public override GraphicMeshSet MeshSetFor", StringComparison.Ordinal));
        Assert.IsTrue(node.Contains("MeshPool.GetMeshSetForSize(size.x, size.y)", StringComparison.Ordinal));
    }

    /// <summary>
    ///     Revert calls ApplyTo with the true body, which KandraForm.From builds without an
    ///     animalKind - so a branch on the incoming form's IsAnimal would never fire on the way
    ///     out and the shape hediff would survive being human again.
    /// </summary>
    [TestMethod]
    public void ApplyToStripsTheOldShapeWhateverIsGoingOn() {
        string shift = Kandra("KandraShapeshift.cs");

        int unshape = shift.IndexOf("Unshape(pawn, forms);", StringComparison.Ordinal);
        int branch = shift.IndexOf("if (form.IsAnimal)", StringComparison.Ordinal);

        Assert.IsTrue(unshape >= 0, "The old shape has to come off unconditionally.");
        Assert.IsTrue(unshape < branch, "And before anything decides what goes on next.");
        Assert.IsTrue(
            shift.Contains("forms?.SetCurrent(form);", StringComparison.Ordinal),
            "The render node reads the worn form in its constructor, so it is set before the hediff."
        );
    }

    /// <summary>
    ///     KandraForm.FromAnimal stores the eaten animal's gender and leaves the colours at
    ///     default(Color), which is transparent black. Applying those unguarded turned the
    ///     colonist's gender and painted their skin invisible.
    /// </summary>
    [TestMethod]
    public void WearingAnAnimalDoesNotRepaintTheColonist() {
        string shift = Kandra("KandraShapeshift.cs");

        int branch = shift.IndexOf("if (form.IsAnimal)", StringComparison.Ordinal);
        int skin = shift.IndexOf("skinColorOverride", StringComparison.Ordinal);

        Assert.IsTrue(branch >= 0 && skin > branch, "Appearance is only applied on the human path.");
    }

    /// <summary>
    ///     ShapeFor used to answer both "which generated race" and "is this an animal at all".
    ///     Only the second question survives, and getting it wrong turns every eaten colonist into
    ///     an animal form, dropping their face, colours, xenotype, faction and ideo.
    /// </summary>
    [TestMethod]
    public void EatingAPersonStillMakesAPersonShapedForm() {
        string comp = Kandra("CompKandraForms.cs");

        Assert.IsTrue(
            comp.Contains("KandraShapeEligibility.Wearable(corpsePawn.kindDef)", StringComparison.Ordinal),
            "The discriminator has to read the corpse's own race, not a generated-shape lookup."
        );

        string eligibility = Kandra("KandraShapeEligibility.cs");
        Assert.IsTrue(eligibility.Contains("kind.race.race.Animal", StringComparison.Ordinal));
        Assert.IsTrue(
            eligibility.Contains("MaxDrawSize", StringComparison.Ordinal),
            "A shaped kandra is humanlike, so anything over two world units is cropped when zoomed out."
        );
    }

    /// <summary>
    ///     Fixing the Torso lookups made surgery reachable on a shaped kandra, which is what lets
    ///     a spike come out - and also opened the full human operations list on a wolf.
    /// </summary>
    [TestMethod]
    public void AShapeIsNotOfferedHumanSurgeryOrClothes() {
        string gates = File.ReadAllText(Path.Combine(
            RepoRoot,
            "CosmereCore",
            "CosmereCore",
            "System",
            "Scadrial",
            "Patch",
            "Kandra",
            "ShapedBodyGatesPatch.cs"
        ));

        Assert.IsTrue(gates.Contains("DrawMedOperationsTab", StringComparison.Ordinal));
        Assert.IsTrue(gates.Contains("HasPartsToWear", StringComparison.Ordinal));
    }

    /// <summary>
    ///     IsColonistPlayerControlled wants MentalStateDef to be null, so a kandra that broke down
    ///     while wearing a wolf had no way back out of the thing making the colony treat it as an
    ///     animal.
    /// </summary>
    [TestMethod]
    public void ABreakingKandraCanStillChangeShape() {
        string gene = File.ReadAllText(Path.Combine(
            RepoRoot, "CosmereCore", "CosmereCore", "System", "Scadrial", "Gene", "BodyAbsorption.cs"
        ));

        Assert.IsFalse(gene.Contains("if (!pawn.IsColonistPlayerControlled) yield break;", StringComparison.Ordinal));
        Assert.IsTrue(gene.Contains("if (!pawn.IsColonist || pawn.Downed) yield break;", StringComparison.Ordinal));
    }

    /// <summary>
    ///     Which face a colonist wears is the player's decision. Being spotted used to strip the
    ///     shape at random, mid-job, with no way to refuse - so now it costs the kandra its cover
    ///     and nothing else.
    /// </summary>
    [TestMethod]
    public void BeingSpottedCostsCoverNotTheShape() {
        string watcher = File.ReadAllText(Path.Combine(
            RepoRoot, "CosmereCore", "CosmereCore", "System", "Scadrial", "Gene", "Shapeshifter.cs"
        ));

        Assert.IsTrue(watcher.Contains("forms.BlowCover();", StringComparison.Ordinal));
        Assert.IsFalse(
            watcher.Contains("KandraShapeshift.Revert", StringComparison.Ordinal),
            "Nothing automatic may take a colonist's chosen face off."
        );
    }

    /// <summary>
    ///     A kandra is a thing that impersonates. One the player does not control should never be
    ///     standing around in its own shape where the colony can see it.
    /// </summary>
    [TestMethod]
    public void AKandraNobodyPlaysIsAlwaysWearingAFace() {
        string watcher = File.ReadAllText(Path.Combine(
            RepoRoot, "CosmereCore", "CosmereCore", "System", "Scadrial", "Gene", "Shapeshifter.cs"
        ));

        Assert.IsTrue(watcher.Contains("if (!pawn.IsColonist)", StringComparison.Ordinal));
        Assert.IsTrue(watcher.Contains("KandraShapeshift.WearAnyFace(pawn)", StringComparison.Ordinal));

        string shift = Kandra("KandraShapeshift.cs");
        Assert.IsTrue(
            shift.Contains("public static void WearAnyFace", StringComparison.Ordinal),
            "And it needs a way in that does not go through the player's skill gate."
        );
    }
}
