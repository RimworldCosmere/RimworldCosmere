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
///     SetNextStatus only runs from QueueCastingJob, which is the confirm and not the hover, so
///     both nextStatus and status.power read zero while the player is still choosing a target.
///     Asking plainly reports every push as strength zero, which is what made the koloss readout
///     say 0.0 and refuse every seizure.
/// </remarks>
public abstract class EmotionalPush : CompAbilityEffect {
    protected new AllomancyAbility parent => (AllomancyAbility)base.parent;

    protected float Reach {
        get {
            float chosen = parent.GetStrength(parent.nextStatus);

            return chosen > 0f ? chosen : parent.GetStrength((Status)1);
        }
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

        MentalState? state = troubled.MentalState;
        if (state == null) return;

        // A loose koloss is not calmed, it is taken. SeizeKoloss runs from the same cast and will
        // either get hold of it - which ends the bloodlust on its own - or say how far short the
        // push fell. Refusing here just talked over that.
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
