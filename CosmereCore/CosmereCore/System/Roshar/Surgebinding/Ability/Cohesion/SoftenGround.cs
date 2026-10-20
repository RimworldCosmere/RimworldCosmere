using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Surgebinding.Ability.Cohesion;

public class SoftenGround : SurgebindingAbility {
    private const int BaseRadius = 3;
    private const int HediffRefreshTicks = 120;

    private IntVec3 zoneCenter;
    private int zoneExpiryTick = -1;

    public SoftenGround(Pawn pawn) : base(pawn) { }

    public SoftenGround(Pawn pawn, AbilityDef def) : base(pawn, def) { }

    private float Radius => BaseRadius + Gene.CurrentIdeal;

    private int DurationTicks => (int)(GenTicks.TicksPerRealSecond * (10f + Gene.CurrentIdeal * 5f));

    private bool ZoneActive => zoneExpiryTick > 0 && Find.TickManager.TicksGame < zoneExpiryTick;

    public override bool Activate(LocalTargetInfo target, LocalTargetInfo dest) {
        float cost = Def.beuPerTick / (1 << Gene.CurrentIdeal);
        if (!Gene.CanLowerReserve(cost)) return false;

        Gene.RemoveFromReserve(cost);

        zoneCenter = target.Cell;
        zoneExpiryTick = Find.TickManager.TicksGame + DurationTicks;

        FleckMaker.Static(zoneCenter, pawn.Map, FleckDefOf.PsycastAreaEffect);

        return true;
    }

    public override void AbilityTick() {
        base.AbilityTick();
        if (!ZoneActive) return;
        if (!pawn.IsHashIntervalTick(HediffRefreshTicks)) return;

        HediffDef? hediffDef = Def.hediff;
        if (hediffDef == null) return;

        float currentRadius = Radius;
        foreach (Verse.Thing thing in GenRadial.RadialDistinctThingsAround(
                     zoneCenter,
                     pawn.Map,
                     currentRadius,
                     true
                 )) {
            if (thing is not Pawn targetPawn) continue;
            if (targetPawn.Dead) continue;
            if (targetPawn.Faction == pawn.Faction) continue;

            if (!targetPawn.health.hediffSet.HasHediff(hediffDef)) {
                HediffWithComps hediff = (HediffWithComps)HediffMaker.MakeHediff(hediffDef, targetPawn);
                targetPawn.health.AddHediff(hediff);
            }
        }
    }

    public override void ExposeData() {
        base.ExposeData();
        Scribe_Values.Look(ref zoneCenter, "zoneCenter");
        Scribe_Values.Look(ref zoneExpiryTick, "zoneExpiryTick", -1);
    }
}
