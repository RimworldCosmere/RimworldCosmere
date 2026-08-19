using System;
using Cosmere.Core.Ability;
using Cosmere.Core.Comp.Hediff;
using Cosmere.System.Scadrial.Allomancy.Ability;
using Cosmere.System.Scadrial.Util;
using RimWorld;
using Verse;
using Verse.AI;

namespace Cosmere.System.Scadrial.Allomancy.Comp.Ability;

/// <summary>
///     Shared reach for the one-shot emotional pushes.
/// </summary>
/// <remarks>
///     Three places hold a number and none of them is reliable alone. BurnToggle puts flaring on
///     the live status as power 2. QueueCastingJob then hardcodes power 1 into nextStatus, which
///     throws that flare away. And during targeting neither is set at all, because SetNextStatus
///     only runs on the confirm, so asking plainly reported every push as strength zero.
///     <para>
///         Taking the highest of the three reads the flare at the hover and again at the cast, and
///         never falls below the one QueueCastingJob would have used. SteelJumpRange already
///         matches the default the same way, for the same reason.
///     </para>
/// </remarks>
public abstract class EmotionalPush : CompAbilityEffect {
    protected new AllomancyAbility parent => (AllomancyAbility)base.parent;

    protected float Reach {
        get {
            int power = Math.Max(parent.status.power, parent.nextStatus?.power ?? 0);

            return parent.GetStrength((Status)Math.Max(power, 1));
        }
    }

    /// <summary>
    ///     Whether a coppercloud over the target swallows this push.
    /// </summary>
    /// <remarks>
    ///     Said out loud, unlike the aura's version of the same check. One deliberate cast that
    ///     quietly does nothing reads as a broken ability, and the player has no other way to learn
    ///     that the Smoker across the room is the reason.
    /// </remarks>
    protected bool Blocked(Pawn target) {
        if (!Coppercloud.Hides(parent.pawn, target, parent.def.metal)) return false;

        Messages.Message(
            "CS_Coppercloud_Blocked".Translate(target.LabelShortCap.Named("TARGET")),
            target,
            MessageTypeDefOf.RejectInput,
            false
        );

        return true;
    }
}

public class InciteBreakProperties : CompProperties_AbilityEffect {
    public InciteBreakProperties() {
        compClass = typeof(InciteBreak);
    }
}

/// <summary>
///     Rioting a person until something in them gives.
/// </summary>
/// <remarks>
///     A Rioter pushes emotion up. Held on a target for minutes it was a slow debuff nobody
///     watched; as one push it is a decision - pick the man on the wall and break him now.
/// </remarks>
public class InciteBreak : EmotionalPush {
    public override void Apply(LocalTargetInfo target, LocalTargetInfo dest) {
        base.Apply(target, dest);

        if (target.Pawn is not { } victim) return;
        if (Blocked(victim)) return;
        if (victim.InMentalState) return;

        MentalBreakDef breakDef = MentalBreakDefOf.Berserk;
        if (!breakDef.Worker.BreakCanOccur(victim)) {
            Messages.Message(
                "CS_Riot_NothingToBreak".Translate(victim.LabelShortCap.Named("TARGET")),
                victim,
                MessageTypeDefOf.RejectInput,
                false
            );

            return;
        }

        victim.jobs?.EndCurrentJob(JobCondition.InterruptForced);
        breakDef.Worker.TryStart(victim, "CS_Riot_Reason".Translate().Resolve(), true);
    }

    public override bool CanApplyOn(LocalTargetInfo target, LocalTargetInfo dest) {
        return target.Pawn is { RaceProps.Humanlike: true };
    }
}

public class QuellBreakProperties : CompProperties_AbilityEffect {
    public QuellBreakProperties() {
        compClass = typeof(QuellBreak);
    }
}

/// <summary>
///     Soothing somebody back out of whatever they went into.
/// </summary>
/// <remarks>
///     The mirror of rioting, and the reason a Soother is worth having in a colony that keeps
///     breaking. It cannot reach a koloss in bloodlust - that one is loose because nobody holds
///     it, not because it is upset, and the registry says so without Core learning what a koloss
///     is.
/// </remarks>
public class QuellBreak : EmotionalPush {
    public override void Apply(LocalTargetInfo target, LocalTargetInfo dest) {
        base.Apply(target, dest);

        if (target.Pawn is not { } troubled) return;
        if (Blocked(troubled)) return;

        MentalState? state = troubled.MentalState;
        if (state == null) return;

        // a loose koloss isn't calmed, it's taken; SeizeKoloss (same cast) already handles that outcome.
        if (UnbreakableStateRegistry.Guards(troubled)) return;

        state.RecoverFromState();
        troubled.jobs?.EndCurrentJob(JobCondition.InterruptForced);

        Messages.Message(
            "CS_Soothe_Calmed".Translate(troubled.LabelShortCap.Named("TARGET")),
            troubled,
            MessageTypeDefOf.PositiveEvent
        );
    }

    public override bool CanApplyOn(LocalTargetInfo target, LocalTargetInfo dest) {
        return target.Pawn is { RaceProps.Humanlike: true };
    }
}
