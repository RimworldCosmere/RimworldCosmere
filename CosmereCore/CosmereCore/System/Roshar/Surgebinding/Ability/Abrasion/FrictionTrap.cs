using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Surgebinding.Ability.Abrasion;

public class FrictionTrap : SurgebindingAbility {
    private const int BaseRadius = 3;
    private const int HediffRefreshTicks = 120;
    private bool overlayActive;

    private IntVec3 zoneCenter;
    private int zoneExpiryTick = -1;

    public FrictionTrap(Pawn pawn) : base(pawn) { }

    public FrictionTrap(Pawn pawn, AbilityDef def) : base(pawn, def) { }

    private float radius => BaseRadius + Gene.CurrentIdeal;

    private int durationTicks {
        get {
            int minutes = Gene.CurrentIdeal switch {
                1 => 1,
                2 => 5,
                3 => 15,
                4 => 30,
                _ => 60,
            };
            return GenTicks.TicksPerRealSecond * 60 * minutes;
        }
    }

    private bool zoneActive => zoneExpiryTick > 0 && Find.TickManager.TicksGame < zoneExpiryTick;

    public override bool Activate(LocalTargetInfo target, LocalTargetInfo dest) {
        float cost = def.beuPerTick / (1 << Gene.CurrentIdeal);
        if (!Gene.CanLowerReserve(cost)) return false;

        Gene.RemoveFromReserve(cost);

        zoneCenter = target.Cell;
        zoneExpiryTick = Find.TickManager.TicksGame + durationTicks;

        FrictionTrapOverlay.Register(pawn.thingIDNumber, zoneCenter, radius, pawn.Map);
        overlayActive = true;

        FleckMaker.Static(zoneCenter, pawn.Map, FleckDefOf.PsycastAreaEffect);

        return true;
    }

    public override void AbilityTick() {
        base.AbilityTick();

        if (!zoneActive) {
            if (overlayActive) {
                FrictionTrapOverlay.Unregister(pawn.thingIDNumber);
                overlayActive = false;
            }

            return;
        }

        if (!pawn.IsHashIntervalTick(HediffRefreshTicks)) return;

        HediffDef? hediffDef = def.hediff;
        if (hediffDef == null) return;

        float currentRadius = radius;
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
