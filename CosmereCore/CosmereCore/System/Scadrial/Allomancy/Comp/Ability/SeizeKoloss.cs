using Cosmere.System.Scadrial.Allomancy.Ability;
using Cosmere.System.Scadrial.Util;
using RimWorld;
using Verse;

namespace Cosmere.System.Scadrial.Allomancy.Comp.Ability;

public class SeizeKolossProperties : CompProperties_AbilityEffect {
    public SeizeKolossProperties() {
        compClass = typeof(SeizeKoloss);
    }
}

/// <summary>
///     Takes hold of a koloss, if the Allomancer is pushing hard enough to manage it.
/// </summary>
/// <remarks>
///     Reach comes straight from the ability's own GetStrength, which already folds in
///     AllomanticPower, the skill, the savant stage and flaring - and already gives a duralumin
///     burn its tenfold spike, because GetStrength reads the duralumin reserve directly when the
///     burn is powered that way. Duralumin is how you exceed your reach without anything here
///     knowing what duralumin is.
///     <para>
///         Holding afterwards costs a fraction of this and never checks distance again. Seizing is
///         the expensive act; the roster bills the cheap one.
///     </para>
/// </remarks>
public class SeizeKoloss : CompAbilityEffect {
    private new AllomancyAbility parent => (AllomancyAbility)base.parent;

    /// <summary>
    ///     How hard this Allomancer is about to push, rather than how hard they are pushing.
    /// </summary>
    /// <remarks>
    ///     GetStrength reads <c>(desiredStatus ?? status).power</c>, and status.power is zero until
    ///     the burn actually starts. Asking without a status therefore reported every seizure as
    ///     reach 0.0 - the readout said the Allomancer could not lift anything, and every attempt
    ///     failed. MetalCost solves the same problem the same way.
    ///     <para>
    ///         The null fallback is load-bearing: once the burn has begun nextStatus is cleared,
    ///         and GetStrength then reads the live status, which is what we want by that point.
    ///     </para>
    /// </remarks>
    private float Reach => parent.GetStrength(parent.nextStatus);

    public override void Apply(LocalTargetInfo target, LocalTargetInfo dest) {
        base.Apply(target, dest);

        if (target.Pawn is not { } koloss) return;

        AcceptanceReport report = KolossControl.TryBind(parent.pawn, koloss, Reach);
        if (report.Accepted) {
            Messages.Message(
                "CS_KolossBound".Translate(
                    parent.pawn.LabelShortCap.Named("HOLDER"),
                    koloss.LabelShortCap.Named("KOLOSS")
                ),
                koloss,
                MessageTypeDefOf.PositiveEvent
            );

            return;
        }

        Messages.Message(report.Reason, koloss, MessageTypeDefOf.RejectInput, false);
    }

    /// <summary>
    ///     Only things that can be bound at all. Resistance is zero for everything else, which is
    ///     also what keeps this off ordinary colonists.
    /// </summary>
    public override bool CanApplyOn(LocalTargetInfo target, LocalTargetInfo dest) {
        return target.Pawn != null && EmotionalResistance.Of(target.Pawn) > 0f;
    }

    public override bool AICanTargetNow(LocalTargetInfo target) {
        return false;
    }

    public override string? ExtraLabelMouseAttachment(LocalTargetInfo target) {
        if (target.Pawn is not { } koloss) return null;

        float needed = EmotionalResistance.Of(koloss);
        if (needed <= 0f) return null;

        return "CS_KolossBind_Readout".Translate(
            Reach.ToString("F1").Named("REACH"),
            needed.ToString("F1").Named("NEEDED")
        );
    }
}
