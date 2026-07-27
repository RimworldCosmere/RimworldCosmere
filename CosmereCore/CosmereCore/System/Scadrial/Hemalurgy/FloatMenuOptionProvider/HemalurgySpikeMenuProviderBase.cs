using Cosmere.Core.Util;
using RimWorld;
using Verse;

namespace Cosmere.System.Scadrial.Hemalurgy.FloatMenuOptionProvider;

public abstract class HemalurgySpikeMenuProviderBase : RimWorld.FloatMenuOptionProvider {
    protected override bool Drafted => false;

    protected override bool Undrafted => true;

    protected override bool Multiselect => false;

    protected bool ValidateSurgeryPair(
        Verse.Thing clickedThing,
        FloatMenuContext context,
        out Pawn target,
        out Pawn surgeon
    ) {
        target = null!;
        surgeon = null!;

        if (clickedThing is not Pawn pawnTarget) return false;
        if (!pawnTarget.RaceProps.Humanlike) return false;
        if (pawnTarget.Dead) return false;

        Pawn? pawnSurgeon = context.FirstSelectedPawn;
        if (pawnSurgeon == null) return false;
        if (pawnSurgeon == pawnTarget) return false;

        if (!HemalurgicDefOf.Cosmere_Scadrial_Hemalurgy.IsFinished) return false;
        if (!ShardUtility.AreAnyEnabled(ShardDefOf.Ruin, ShardDefOf.Harmony)) return false;

        target = pawnTarget;
        surgeon = pawnSurgeon;
        return true;
    }
}
