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

    /// <summary>What the spikes were holding, kept for when a new pair goes in.</summary>
    private KandraMind mind = new KandraMind();

    /// <summary>Set once somebody has worked out what this is. Cleared by changing shape.</summary>
    private bool coverBlown;

    /// <summary>When the current shape went on, so a fight three shapes ago does not count.</summary>
    private int wornSinceTick;

    /// <summary>The work tab as it was before a shape disabled half of it.</summary>
    private Dictionary<WorkTypeDef, int> workPriorities = [];

    public KandraMind Mind => mind;

    public CompProperties_KandraForms Props => (CompProperties_KandraForms)props;

    public IReadOnlyList<KandraForm> Known => known;

    public KandraForm? Current => current;

    public bool IsWearingSomeoneElse => current != null;

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
        Scribe_Deep.Look(ref current, "currentForm");
        Scribe_Deep.Look(ref trueBody, "trueBody");
        Scribe_Deep.Look(ref mind, "mind");
        Scribe_Values.Look(ref coverBlown, "coverBlown");
        Scribe_Values.Look(ref wornSinceTick, "wornSinceTick");
        Scribe_Collections.Look(ref workPriorities, "workPriorities", LookMode.Def, LookMode.Value);
        workPriorities ??= [];
        mind ??= new KandraMind();
        known ??= [];
    }

    /// <summary>Eats a body and remembers it. Duplicate faces are not worth storing twice.</summary>
    public void Learn(Pawn corpsePawn) {
        if (parent is not Pawn pawn) return;

        PawnKindDef? shape = KandraAnimalForms.ShapeFor(corpsePawn.kindDef);
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

    /// <summary>Remembers what the kandra actually is, the first time they wear anything else.</summary>
    public void RememberTrueBody() {
        if (trueBody != null) return;
        if (parent is Pawn pawn) trueBody = KandraForm.From(pawn);
    }

    /// <summary>The name the colony hears, or null when the kandra is being itself.</summary>
    public string? WornName => current?.Label;

    /// <summary>
    ///     Puts the impersonation in the selected pawn's panel.
    /// </summary>
    /// <remarks>
    ///     The player has to be able to tell at a glance which of their colonists is currently a
    ///     kandra wearing somebody. Nobody else in the colony gets this line.
    /// </remarks>
    public override string CompInspectStringExtra() {
        if (current == null) return string.Empty;

        return "CS_Kandra_Wearing".Translate(current.Label.Named("FORM")).Resolve();
    }

    public KandraForm? TrueBody => trueBody;

    public void SetCurrent(KandraForm? form) {
        current = form;
        coverBlown = false;
        wornSinceTick = Find.TickManager?.TicksGame ?? 0;
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

        // Shaping twice without reverting would otherwise snapshot the already-zeroed tab and
        // lose the real one for good.
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
