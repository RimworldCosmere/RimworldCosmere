using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.System.Scadrial.Kandra;

public class CompProperties_KandraForms : CompProperties {
    /// <summary>Shapeshifting level at which a kandra can build a body from nothing.</summary>
    public int freeFormSkill = 12;

    /// <summary>Skill gained per set of bones eaten. Every body teaches something.</summary>
    public float xpPerForm = 3000f;

    public CompProperties_KandraForms() {
        compClass = typeof(CompKandraForms);
    }
}

/// <summary>
///     Every body a kandra has eaten, and which one they are wearing.
/// </summary>
/// <remarks>
///     A kandra needs the bones. Eating a corpse copies that person down to the name, and the
///     kandra can put the body back on later at will. Enough bodies and they stop needing a
///     template at all - the skill is the difference between reproducing a shape and inventing
///     one.
/// </remarks>
public class CompKandraForms : ThingComp {
    private KandraForm? current;
    private List<KandraForm> known = [];

    /// <summary>The kandra's own body, kept so they can always go back to it.</summary>
    private KandraForm? trueBody;

    /// <summary>The material the player picked for the true body. Null until someone designs it.</summary>
    private string? trueBodyMaterial;

    /// <summary>What a running reshape job will commit. Reshaping takes ten seconds, so the
    /// choice waits for the work to finish rather than snapping the moment it is confirmed.</summary>
    private string? pendingMaterial;
    private Gender? pendingGender;
    private HairDef? pendingHair;

    /// <summary>The hair colour by palette name, never as a Color. A name picks up a retuned hex
    /// later, where a stored hex freezes a colour that may leave the palette, and a nullable
    /// struct through Scribe_Values is unproven here where a string is not.</summary>
    private string? pendingHairColour;

    /// <summary>The eyes by palette name, for the same reasons the hair colour is a name. Null
    /// means the player changed nothing, so the two that need an "unset" answer carry a sentinel:
    /// "none" clears odd eyes and "off" puts the light out.</summary>
    private string? pendingEyeColour;
    private string? pendingEyeColourTwo;
    private string? pendingIrisSize;
    private string? pendingEyeLight;

    /// <summary>What the spikes were holding, kept for when a new pair goes in.</summary>
    private KandraMind mind = new KandraMind();

    /// <summary>Set once somebody has worked out what this is. Cleared by changing shape.</summary>
    private bool coverBlown;

    /// <summary>When the current shape went on, so a fight three shapes ago does not count.</summary>
    private int wornSinceTick;

    /// <summary>The work tab as it was before a shape disabled half of it.</summary>
    private Dictionary<WorkTypeDef, int> workPriorities = [];

    /// <summary>What the kandra was holding and wearing, so it all goes back the same way.</summary>
    private List<Verse.Thing> wasEquipped = [];
    private List<Verse.Thing> wasWorn = [];

    public KandraMind Mind => mind;

    public CompProperties_KandraForms Props => (CompProperties_KandraForms)props;

    public IReadOnlyList<KandraForm> Known => known;

    public KandraForm? Current => current;

    public bool IsWearingSomeoneElse => current != null;

    /// <summary>Whether the body underneath is the one it built rather than one it ate.</summary>
    public bool TrueBodyCrafted => trueBody?.crafted == true;

    /// <summary>Whether this kandra can shape a body nobody has ever worn.</summary>
    public bool CanFreeForm {
        get {
            if (parent is not Pawn pawn) return false;

            return SkillLevel(pawn) >= Props.freeFormSkill;
        }
    }

    /// <summary>
    ///     How convincing the disguise is, 0 to 1. Bronze sees through it regardless.
    /// </summary>
    public float Conviction {
        get {
            if (parent is not Pawn pawn) return 0f;

            // Twenty is the ceiling on any RimWorld skill, so this reads as "how far along".
            return Mathf.Clamp01(SkillLevel(pawn) / 20f);
        }
    }

    /// <summary>
    ///     A kandra that spawns wearing nobody is already formless, and nothing called
    ///     <see cref="SetCurrent" /> to say so. Without this it waits out a rare tick as a human.
    /// </summary>
    public override void PostSpawnSetup(bool respawningAfterLoad) {
        base.PostSpawnSetup(respawningAfterLoad);

        Util.KandraAppearance.SyncFormless(parent as Pawn);
        Util.KandraAppearance.SyncAnimalShape(parent as Pawn);
        SyncTrueBodyHairColour();
    }

    /// <summary>
    ///     Pushes a retuned dye onto the pawn itself. KandraForm re-resolves its own colour on
    ///     load, but the map pawn renders from story.HairColor, which only a shape change writes.
    ///     Without this a retune reaches the portrait and leaves the kandra on screen unchanged.
    /// </summary>
    private void SyncTrueBodyHairColour() {
        if (parent is not Pawn pawn || pawn.story == null) return;

        // the disguise owns the hair while one is on. undesigned kandra have no name and keep theirs.
        if (current != null || trueBody?.hairColourName == null) return;
        if (Util.KandraAppearance.FindHairColour(trueBody.hairColourName) == null) return;
        if (pawn.story.HairColor == trueBody.hairColour) return;

        pawn.story.HairColor = trueBody.hairColour;
        pawn.Drawer?.renderer?.SetAllGraphicsDirty();
    }

    /// <summary>
    ///     A dead kandra drops the disguise.
    /// </summary>
    /// <remarks>
    ///     Without this the corpse keeps the borrowed face and, worse, the borrowed name, so a
    ///     colonist who is standing right there stays on the dead list forever while the thing
    ///     that ate them gets buried under their headstone.
    /// </remarks>
    public override void Notify_Killed(Map prevMap, DamageInfo? dinfo = null) {
        base.Notify_Killed(prevMap, dinfo);

        if (current == null) return;

        KandraShapeshift.Revert((Pawn)parent);
    }

    public override void PostExposeData() {
        base.PostExposeData();
        Scribe_Collections.Look(ref known, "knownForms", LookMode.Deep);
        known ??= [];
        Scribe_Deep.Look(ref current, "currentForm");
        Scribe_Deep.Look(ref trueBody, "trueBody");
        Scribe_Deep.Look(ref mind, "mind");
        Scribe_Values.Look(ref coverBlown, "coverBlown");
        Scribe_Values.Look(ref trueBodyMaterial, "trueBodyMaterial");
        Scribe_Values.Look(ref pendingMaterial, "pendingMaterial");
        Scribe_Values.Look(ref pendingGender, "pendingGender");
        Scribe_Defs.Look(ref pendingHair, "pendingHair");
        Scribe_Values.Look(ref pendingHairColour, "pendingHairColour");
        Scribe_Values.Look(ref pendingEyeColour, "pendingEyeColour");
        Scribe_Values.Look(ref pendingEyeColourTwo, "pendingEyeColourTwo");
        Scribe_Values.Look(ref pendingIrisSize, "pendingIrisSize");
        Scribe_Values.Look(ref pendingEyeLight, "pendingEyeLight");
        Scribe_Values.Look(ref wornSinceTick, "wornSinceTick");
        Scribe_Collections.Look(ref workPriorities, "workPriorities", LookMode.Def, LookMode.Value);
        workPriorities ??= [];

        // By reference: the things themselves live in this pawn's inventory.
        Scribe_Collections.Look(ref wasEquipped, "wasEquipped", LookMode.Reference);
        Scribe_Collections.Look(ref wasWorn, "wasWorn", LookMode.Reference);
        wasEquipped ??= [];
        wasWorn ??= [];
        mind ??= new KandraMind();
        known ??= [];
    }

    /// <summary>Eats a body and remembers it. Duplicate faces are not worth storing twice.</summary>
    public void Learn(Pawn corpsePawn) {
        if (parent is not Pawn pawn) return;

        // reading RaceProps off the corpse is what stops an eaten colonist becoming an animal form.
        PawnKindDef? shape = KandraShapeEligibility.Wearable(corpsePawn.kindDef) ? corpsePawn.kindDef : null;
        KandraForm form = shape != null
            ? KandraForm.FromAnimal(corpsePawn, shape)
            : KandraForm.From(corpsePawn);
        for (int i = 0; i < known.Count; i++) {
            if (known[i].nameFull == form.nameFull && known[i].animalKind == form.animalKind) return;
        }

        known.Add(form);
        pawn.skills?.Learn(SkillDefOf.Cosmere_Scadrial_Skill_Shapeshift, Props.xpPerForm, true);
    }

    /// <summary>Adds a shape to the repertoire without eating anybody for it.</summary>
    public void Remember(KandraForm form) {
        known.Add(form);
    }

    /// <summary>
    ///     Decides that an eaten shape is what it goes back to now. Nothing is destroyed - the old
    ///     true body joins the repertoire, so it can wear its former self whenever it likes.
    /// </summary>
    public void AdoptTrueBody(KandraForm form) {
        if (trueBody != null && trueBody != form && !known.Contains(trueBody)) known.Add(trueBody);

        trueBody = form;
        known.Remove(form);

        Util.KandraAppearance.SyncAnimalShape(parent as Pawn);
        if (parent is Pawn pawn) pawn.Drawer?.renderer?.SetAllGraphicsDirty();
    }

    /// <summary>Remembers what the kandra actually is, the first time they wear anything else.</summary>
    public void RememberTrueBody() {
        if (trueBody != null) return;
        if (parent is not Pawn pawn) return;

        trueBody = KandraForm.From(pawn);

        // What a kandra is when it is being nobody. The stored face is only a size and a shape.
        trueBody.crafted = true;
    }

    /// <summary>
    ///     The name the colony hears, or null when the kandra is being itself. A shape that
    ///     carries the kandra's own name is not an impersonation, so it reads as nothing.
    /// </summary>
    public string? WornName {
        get {
            string? worn = current?.Label;
            if (worn.NullOrEmpty()) return null;

            string? own = (parent as Pawn)?.Name?.ToStringShort;

            return worn == own ? null : worn;
        }
    }

    /// <summary>
    ///     Puts the impersonation in the selected pawn's panel.
    /// </summary>
    /// <remarks>
    ///     The player has to be able to tell at a glance which of their colonists is currently a
    ///     kandra wearing somebody. Nobody else in the colony gets this line.
    /// </remarks>
    public override string CompInspectStringExtra() {
        string? worn = WornName;

        return worn.NullOrEmpty()
            ? string.Empty
            : "CS_Kandra_Wearing".Translate(worn!.Named("FORM")).Resolve();
    }

    public KandraForm? TrueBody => trueBody;

    public string? TrueBodyMaterial => trueBodyMaterial;

    /// <summary>Records what the reshape job will produce when it finishes.</summary>
    public void BeginReshape(
        string material,
        Gender gender,
        HairDef? hair,
        string? hairColour,
        string? eyeColour = null,
        string? eyeColourTwo = null,
        string? irisSize = null,
        string? eyeLight = null
    ) {
        pendingMaterial = material;
        pendingGender = gender;
        pendingHair = hair;
        pendingHairColour = hairColour;
        pendingEyeColour = eyeColour;
        pendingEyeColourTwo = eyeColourTwo;
        pendingIrisSize = irisSize;
        pendingEyeLight = eyeLight;
    }

    /// <summary>Throws away a reshape that never finished, so it does not sit in the save.</summary>
    public void CancelReshape() {
        pendingMaterial = null;
        pendingGender = null;
        pendingHair = null;
        pendingHairColour = null;
        pendingEyeColour = null;
        pendingEyeColourTwo = null;
        pendingIrisSize = null;
        pendingEyeLight = null;
    }

    /// <summary>Applies a finished reshape. False when there was nothing waiting.</summary>
    public bool CommitReshape() {
        if (pendingMaterial == null && pendingGender == null && pendingHair == null &&
            pendingHairColour == null && pendingEyeColour == null && pendingEyeColourTwo == null &&
            pendingIrisSize == null && pendingEyeLight == null) {
            return false;
        }

        if (trueBody == null) return false;

        trueBodyMaterial = pendingMaterial;
        if (pendingGender != null) trueBody.gender = pendingGender.Value;
        if (pendingHair != null) trueBody.hair = pendingHair;

        // the name rides along so a retune reaches this body; a dropped name leaves both alone.
        if (Util.KandraAppearance.FindHairColour(pendingHairColour) != null) {
            trueBody.hairColourName = pendingHairColour;
            trueBody.hairColour = Util.KandraAppearance.HairColorFor(pendingHairColour);
        }

        // the none sentinel puts the eyes out entirely; the guard below clears what hung off them.
        if (pendingEyeColour == Util.KandraAppearance.EyeColourNone) {
            trueBody.eyeColourName = null;
        } else if (Util.KandraAppearance.FindEyeColour(pendingEyeColour) != null) {
            trueBody.eyeColourName = pendingEyeColour;
        }

        // the none sentinel is how the player says both eyes match again.
        if (pendingEyeColourTwo == Util.KandraAppearance.EyeColourNone) {
            trueBody.eyeColourTwoName = null;
        } else if (Util.KandraAppearance.FindEyeColour(pendingEyeColourTwo) != null) {
            trueBody.eyeColourTwoName = pendingEyeColourTwo;
        }

        if (Util.KandraAppearance.FindIrisSize(pendingIrisSize) != null) {
            trueBody.irisSizeName = pendingIrisSize;
        }

        if (pendingEyeLight == Util.KandraAppearance.EyeLightOff) {
            trueBody.eyeLightName = Util.KandraAppearance.EyeLightOff;
        } else if (Util.KandraAppearance.FindEyeLight(pendingEyeLight) != null) {
            trueBody.eyeLightName = pendingEyeLight;
        }

        // a second stone and a light describe eyes that are not there, so they go with the first one.
        if (trueBody.eyeColourName == null) {
            trueBody.eyeColourTwoName = null;
            trueBody.eyeLightName = null;
            trueBody.irisSizeName = null;
        }

        // A right eye cut from the left eye's stone is a matched pair, whatever the picker said.
        if (trueBody.eyeColourTwoName == trueBody.eyeColourName) trueBody.eyeColourTwoName = null;

        pendingMaterial = null;
        pendingGender = null;
        pendingHair = null;
        pendingHairColour = null;
        pendingEyeColour = null;
        pendingEyeColourTwo = null;
        pendingIrisSize = null;
        pendingEyeLight = null;

        return true;
    }

    public void SetCurrent(KandraForm? form) {
        current = form;
        coverBlown = false;
        wornSinceTick = Find.TickManager?.TicksGame ?? 0;
        Util.KandraAppearance.SyncFormless(parent as Pawn);
        Util.KandraAppearance.SyncAnimalShape(parent as Pawn);
    }

    /// <summary>
    ///     Whether anybody has worked out what this is while it wears the current shape.
    /// </summary>
    /// <remarks>
    ///     Stays true for as long as the shape does. Changing shape is the way out, which is the
    ///     whole reason a kandra keeps more than one face.
    /// </remarks>
    public bool CoverBlown => coverBlown;

    public void BlowCover() {
        coverBlown = true;
    }

    public List<Verse.Thing> WasEquipped => wasEquipped;

    public List<Verse.Thing> WasWorn => wasWorn;

    public void RememberGear(IEnumerable<Verse.Thing> equipped, IEnumerable<Verse.Thing> worn) {
        wasEquipped = [.. equipped];
        wasWorn = [.. worn];
    }

    public void ForgetGear() {
        wasEquipped = [];
        wasWorn = [];
    }

    /// <summary>Whether the kandra has raised a hand to anyone since putting this shape on.</summary>
    public bool FoughtInThisShape(Pawn wearing) {
        int last = wearing.mindState?.lastAttackTargetTick ?? 0;

        return last > wornSinceTick;
    }

    /// <summary>
    ///     Remembers the work tab before a shape disables half of it.
    /// </summary>
    /// <remarks>
    ///     <c>Pawn_HealthTracker.CheckForStateChange</c> calls
    ///     <c>Notify_DisabledWorkTypesChanged</c> for any hediff stage carrying
    ///     <c>disabledWorkTags</c>, which walks the newly disabled types and calls
    ///     <c>SetPriority(w, 0)</c> on each. RimWorld has no inverse: removing the hediff re-enables
    ///     the work type but leaves the priority at zero forever. Twelve columns would go blank
    ///     every time the kandra shaped.
    /// </remarks>
    public void RememberWorkPriorities() {
        if (parent is not Pawn pawn || pawn.workSettings is not { EverWork: true }) return;

        // shaping twice without reverting would snapshot the already-zeroed tab and lose the real one.
        if (workPriorities.Count > 0) return;

        DefMap<WorkTypeDef, int>? stored = StoredPriorities(pawn);
        if (stored == null) return;

        workPriorities = [];
        List<WorkTypeDef> all = DefDatabase<WorkTypeDef>.AllDefsListForReading;
        for (int i = 0; i < all.Count; i++) {
            int priority = stored[all[i]];
            if (priority > 0) workPriorities[all[i]] = priority;
        }
    }

    /// <summary>
    ///     The real stored priorities, not what the work tab is willing to admit to.
    /// </summary>
    /// <remarks>
    ///     <c>Pawn_WorkSettings.GetPriority</c> returns a flat 3 for any humanlike pawn with a
    ///     non-zero priority while <c>Find.PlaySettings.useWorkPriorities</c> is off, which is the
    ///     default. Snapshotting through it would store 3 for everything and hand 3 back on
    ///     restore, quietly flattening a player's tuned 1s and 4s the first time their kandra
    ///     changed shape.
    /// </remarks>
    private static DefMap<WorkTypeDef, int>? StoredPriorities(Pawn pawn) {
        prioritiesField ??= typeof(Pawn_WorkSettings).GetField(
            "priorities",
            global::System.Reflection.BindingFlags.NonPublic | global::System.Reflection.BindingFlags.Instance
        );

        return prioritiesField?.GetValue(pawn.workSettings) as DefMap<WorkTypeDef, int>;
    }

    private static global::System.Reflection.FieldInfo? prioritiesField;

    /// <summary>
    ///     Puts the work tab back, after the shape is gone.
    /// </summary>
    /// <remarks>
    ///     Order is not optional. <c>SetPriority</c> logs an error when given a non-zero priority
    ///     for a work type that is still disabled, so this must run after the hediff is removed,
    ///     never before.
    /// </remarks>
    public void RestoreWorkPriorities() {
        if (workPriorities.Count == 0) return;
        if (parent is not Pawn pawn || pawn.workSettings is not { EverWork: true }) return;

        foreach (KeyValuePair<WorkTypeDef, int> remembered in workPriorities) {
            if (pawn.WorkTypeIsDisabled(remembered.Key)) continue;

            pawn.workSettings.SetPriority(remembered.Key, remembered.Value);
        }

        workPriorities = [];
    }

    private static int SkillLevel(Pawn pawn) {
        return pawn.skills?.GetSkill(SkillDefOf.Cosmere_Scadrial_Skill_Shapeshift)?.Level ?? 0;
    }
}
