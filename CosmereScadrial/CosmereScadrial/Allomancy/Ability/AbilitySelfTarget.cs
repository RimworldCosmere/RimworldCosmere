using System.Collections.Generic;
using System.Linq;
using CosmereFramework.Extension;
using CosmereScadrial.Allomancy.Comp.Thing;
using CosmereScadrial.Util;
using RimWorld;
using Verse;

namespace CosmereScadrial.Allomancy.Ability;

public class AbilitySelfTarget : AbstractAbility {
    public AbilitySelfTarget() { }

    public AbilitySelfTarget(Pawn pawn) : base(pawn) { }

    public AbilitySelfTarget(Pawn pawn, Precept sourcePrecept) : base(pawn, sourcePrecept) { }

    public AbilitySelfTarget(Pawn pawn, AbilityDef def) : base(pawn, def) {
        localTarget = pawn;
    }

    public AbilitySelfTarget(Pawn pawn, Precept sourcePrecept, AbilityDef def) : base(pawn, sourcePrecept, def) {
        localTarget = pawn;
    }

    public override string Tooltip {
        get {
            string tooltip = base.Tooltip;
            if (!willBurnWhileDowned) return tooltip;
            List<string> tooltipByLine = tooltip.Split('\n').ToList();
            tooltipByLine.Insert(1, "CS_WillBurnWhileDowned".Translate().Colorize(ColorLibrary.Green));

            return tooltipByLine.ToStringList("\n");
        }
    }

    public override void AbilityTick() {
        base.AbilityTick();

        if (willBurnWhileDowned && pawn.Downed && !pawn.Dead && !atLeastPassive) {
            MetalReserves reserves = pawn.GetComp<MetalReserves>();
            if (reserves?.CanLowerReserve(metal, AllomancyUtility.GetMetalNeededForBeu(def.beuPerTick)) ?? false) {
                UpdateStatus(BurningStatus.Passive);
            }
        }

        if (!atLeastPassive) {
            return;
        }

        if (pawn.Downed) {
            if (status > BurningStatus.Passive && (!def.canBurnWhileDowned || !willBurnWhileDowned)) {
                UpdateStatus(BurningStatus.Off);
            } else if (status == BurningStatus.Off && willBurnWhileDowned) {
                UpdateStatus(BurningStatus.Passive);
            }
        } else if (pawn.IsAsleep() && status > BurningStatus.Passive) {
            UpdateStatus(def.canBurnWhileAsleep ? BurningStatus.Passive : BurningStatus.Off);
        } else if (!pawn.IsAsleep() && status == BurningStatus.Passive) {
            UpdateStatus(BurningStatus.Burning);
        }
    }

    protected override void OnEnable() {
        base.OnEnable();
        GetOrAddHediff(pawn);
    }

    protected override void OnDisable() {
        base.OnDisable();
        ApplyDrag(pawn, flareDuration / 3000f);
        RemoveHediff(pawn);
        flareStartTick = -1;
    }

    protected override void OnFlare() {
        base.OnFlare();
        flareStartTick = Find.TickManager.TicksGame;
        GetOrAddHediff(pawn);
        RemoveDrag(pawn);
    }

    protected override void OnDeFlare() {
        base.OnDeFlare();
        ApplyDrag(pawn, flareDuration / 3000f / 2);
        flareStartTick = -1;
    }
}