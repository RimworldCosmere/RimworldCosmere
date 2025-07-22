using Cosmere.Core.Ability;
using Cosmere.Core.Hediff;
using Cosmere.Core.Need;
using Cosmere.Roshar.Gene;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.Roshar.Surgebinding.Hediff;

public class BreathStormlight(HediffDef hediffDef, Pawn pawn, IAbility<Surgebinder, IHediff<Surgebinder>> ability)
    : SurgebindingHediff(hediffDef, pawn, ability) {
    private const float maxDrawDistance = 3f;
    private const float baseAbsorbAmount = 25f;

    private Investiture? investiture => pawn.needs.TryGetNeed<Investiture>();

    public override void PostTickInterval(int delta) {
        base.PostTickInterval(delta);

        if (!pawn.IsHashIntervalTick(GenTicks.TicksPerRealSecond, delta)) return;
        if (investiture == null || investiture.IsMaxLevel) return;

        TryAbsorbFromPouch();
        //TryAbsorbFromInventory();
        //TryAbsorbFromNearbyThings();
    }

    private void TryAbsorbFromPouch() {
        Verse.Thing? pouch =
            pawn.apparel?.WornApparel?.FirstOrDefault(x => x.def.Equals(ThingDefOf.Cosmere_Roshar_Apparel_SpherePouch));
        if (pouch == null) return;

        StatDef? stat = Core.StatDefOf.Cosmere_Investiture;
        float pouchInvestiture = pouch.GetStatValue(Core.StatDefOf.Cosmere_Investiture);
        if (Mathf.Approximately(pouchInvestiture, 0f)) return;
        //pouch.
    }
}