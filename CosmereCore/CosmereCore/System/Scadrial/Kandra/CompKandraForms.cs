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
        known ??= [];
    }

    /// <summary>Eats a body and remembers it. Duplicate faces are not worth storing twice.</summary>
    public void Learn(Pawn corpsePawn) {
        if (parent is not Pawn pawn) return;

        KandraForm form = KandraForm.From(corpsePawn);
        for (int i = 0; i < known.Count; i++) {
            if (known[i].nameFull == form.nameFull) return;
        }

        known.Add(form);
        pawn.skills?.Learn(SkillDefOf.Cosmere_Scadrial_Skill_Shapeshift, Props.xpPerForm, true);
    }

    /// <summary>Remembers what the kandra actually is, the first time they wear anything else.</summary>
    public void RememberTrueBody() {
        if (trueBody != null) return;
        if (parent is Pawn pawn) trueBody = KandraForm.From(pawn);
    }

    public KandraForm? TrueBody => trueBody;

    public void SetCurrent(KandraForm? form) {
        current = form;
    }

    private static int SkillLevel(Pawn pawn) {
        return pawn.skills?.GetSkill(SkillDefOf.Cosmere_Scadrial_Skill_Shapeshift)?.Level ?? 0;
    }
}
