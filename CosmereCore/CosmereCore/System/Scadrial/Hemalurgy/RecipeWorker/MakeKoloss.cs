using Cosmere.Core.Util;
using Cosmere.System.Scadrial.Util;
using RimWorld;
using Verse;

namespace Cosmere.System.Scadrial.Hemalurgy.RecipeWorker;

/// <summary>
///     Drives four spikes into a person and leaves a koloss.
/// </summary>
/// <remarks>
///     Serves both routes. The Lord Ruler's version is butchery on someone who did not agree to
///     it and kills most of its subjects; the post-Catacendre version is the same operation on a
///     koloss-blooded youth whose body already expects the spikes. The recipe defs carry that
///     difference in their success and death chances - the work here is identical either way,
///     because the spikes do not care why they were driven.
/// </remarks>
public class MakeKoloss : Recipe_Surgery {
    /// <summary>A koloss-blooded youth may choose this for themselves at twelve.</summary>
    public const int ChoosingAge = 12;

    public override bool AvailableOnNow(Verse.Thing thing, BodyPartRecord? part = null) {
        if (!base.AvailableOnNow(thing, part)) return false;
        if (thing is not Pawn pawn) return false;
        if (!HemalurgicDefOf.Cosmere_Scadrial_Hemalurgy.IsFinished) return false;
        if (!ShardUtility.AreAnyEnabled(ShardDefOf.Ruin, ShardDefOf.Harmony)) return false;

        // Nothing to do to something that is already a koloss.
        if (IsKoloss(pawn)) return false;

        if (recipe?.defName == "Cosmere_Scadrial_Recipe_TakeTheSpikes") {
            return IsKolossBlooded(pawn) && pawn.ageTracker.AgeBiologicalYears >= ChoosingAge;
        }

        return true;
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
        if (pawn.genes == null) return;

        XenotypeDef? koloss = DefDatabase<XenotypeDef>.GetNamedSilentFail(
            "Cosmere_Scadrial_Xenotype_Koloss"
        );
        if (koloss == null) {
            Cosmere.Core.Logger.Warning("MakeKoloss: the koloss xenotype is missing.");
            return;
        }

        // the name has to be read before the subject is consumed - afterwards theres nobody left to ask
        TaggedString who = pawn.NameShortColored;

        Pawn? made = KolossUtility.Make(pawn, koloss);
        if (made == null) return;

        Messages.Message(
            "CS_BecameKoloss".Translate(who.Named("PAWN")),
            made,
            MessageTypeDefOf.NeutralEvent,
            false
        );
    }

    private static bool IsKoloss(Pawn pawn) {
        return pawn.genes?.Xenotype?.defName == "Cosmere_Scadrial_Xenotype_Koloss";
    }

    private static bool IsKolossBlooded(Pawn pawn) {
        return pawn.genes?.Xenotype?.defName == "Cosmere_Scadrial_Xenotype_KolossBlooded";
    }
}
