using Cosmere.Core.Util;
using Cosmere.System.Scadrial.Util;
using RimWorld;
using Verse;

namespace Cosmere.System.Scadrial.Hemalurgy.RecipeWorker;

/// <summary>
///     Drives a Blessing into a mistwraith and wakes a kandra up.
/// </summary>
/// <remarks>
///     Only offered on something that has lost its mind - a mistwraith the colony has caught, or
///     a kandra somebody already unmade. Spiking a person this way makes a person with spikes in
///     them, not a kandra, and the Contract would have a great deal to say about anyone who tried.
/// </remarks>
public class GiveBlessing : Recipe_Surgery {
    public override bool AvailableOnNow(Verse.Thing thing, BodyPartRecord? part = null) {
        if (!base.AvailableOnNow(thing, part)) return false;
        if (thing is not Pawn pawn) return false;
        if (!HemalurgicDefOf.Cosmere_Scadrial_Hemalurgy.IsFinished) return false;
        if (!ShardUtility.AreAnyEnabled(ShardDefOf.Ruin, ShardDefOf.Harmony)) return false;

        HediffDef? blessing = recipe?.GetModExtension<BlessingExtension>()?.blessing;
        if (blessing == null) return false;

        // not this one twice - another Blessing on top is fine, thats how a kandra carries more than one
        if (pawn.health?.hediffSet?.HasHediff(blessing) == true) return false;

        // Waking a mistwraith up, or adding to a kandra that is already awake.
        if (pawn.health?.hediffSet?.HasHediff(HediffDefOf.Cosmere_Scadrial_Hediff_Mistwraith) == true) {
            return true;
        }

        return KandraUtility.IsKandra(pawn);
    }

    public override IEnumerable<BodyPartRecord> GetPartsToApplyOn(Pawn pawn, RecipeDef recipe) {
        BodyPartRecord? torso = pawn.health.hediffSet.GetNotMissingParts()
            .FirstOrDefault(p => p.def == pawn.RaceProps.body.corePart.def);
        if (torso != null) yield return torso;
    }

    public override void ApplyOnPawn(
        Pawn pawn,
        BodyPartRecord part,
        Pawn billDoer,
        List<Verse.Thing> ingredients,
        Bill bill
    ) {
        HediffDef? blessing = recipe?.GetModExtension<BlessingExtension>()?.blessing;
        if (blessing == null) {
            Log.Warn(
                $"GiveBlessing: {recipe?.defName} has no BlessingExtension, so there is nothing to drive in."
            );
            return;
        }

        if (KandraUtility.IsKandra(pawn)) {
            // Already a kandra. It is gaining a Blessing, not being made into one.
            KandraUtility.GiveBlessing(pawn, blessing);
        } else {
            KandraUtility.Become(pawn, blessing);
        }

        Messages.Message(
            "CS_BecameKandra".Translate(
                pawn.NameShortColored.Named("PAWN"),
                blessing.LabelCap.Named("BLESSING")
            ),
            pawn,
            MessageTypeDefOf.NeutralEvent,
            false
        );
    }
}
