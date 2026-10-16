using System.Collections.Generic;
using Cosmere.System.Scadrial.Util;
using RimWorld;
using Verse;

namespace Cosmere.System.Scadrial.Ritual;

/// <summary>
///     The Ministry's version of the surgery: a body on a slab and a congregation watching.
/// </summary>
/// <remarks>
///     The behaviour does the killing - this pattern inherits the prisoner sacrifice, so by the
///     time the outcome fires the victim is already dead on the spot. That suits what making a
///     koloss is: the person does not survive it either way, and what rises is a different thing
///     wearing what is left.
///     <para>
///         The surgery remains the path for a colony without Ideology. This is the same act
///         performed in front of people, which is how the Steel Ministry did it.
///     </para>
/// </remarks>
public class RitualOutcomeEffectWorker_MakeKoloss : RitualOutcomeEffectWorker_FromQuality {
    public RitualOutcomeEffectWorker_MakeKoloss() { }

    public RitualOutcomeEffectWorker_MakeKoloss(RitualOutcomeEffectDef def)
        : base(def) { }

    public override void Apply(float progress, Dictionary<Pawn, int> totalPresence, LordJob_Ritual jobRitual) {
        base.Apply(progress, totalPresence, jobRitual);

        Pawn? victim = jobRitual?.assignments?.FirstAssignedPawn("prisoner");
        if (victim == null) return;

        XenotypeDef? koloss = DefDatabase<XenotypeDef>.GetNamedSilentFail(KolossUtility.KolossXenotype);
        if (koloss == null) {
            Log.Warn("MakeKoloss ritual: the koloss xenotype is missing.");
            return;
        }

        Pawn? made = KolossUtility.MakeFrom(victim, koloss);
        if (made == null) return;

        Find.LetterStack.ReceiveLetter(
            "CS_KolossRite_Title".Translate(),
            "CS_KolossRite".Translate(victim.NameShortColored.Named("PAWN")),
            LetterDefOf.NeutralEvent,
            made
        );
    }
}
