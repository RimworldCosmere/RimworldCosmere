using System.Collections.Generic;
using System.Linq;
using CosmereFramework.Extension;
using RimWorld;
using Verse;

namespace CosmereScadrial.Allomancy.Ability;

public class AbilitySelfTarget : AbstractAbility {
    public bool paused;
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

        if (willBurnWhileDowned && pawn.Downed && !pawn.Dead && !atLeastBurning) {
            if (gene.CanLowerReserve(gene.GetMetalNeededForBreathEquivalentUnits(def.beuPerTick))) {
                UpdateStatus(BurningStatus.Burning);
            }
        }

        if (!atLeastBurning) {
            return;
        }

        if (pawn.IsAsleep() && status == BurningStatus.Flaring && !def.canBurnWhileAsleep) {
            UpdateStatus(BurningStatus.Burning);
        }

        if (paused && !pawn.IsAsleep()) {
            paused = false;
            gene.UpdateBurnSource((def, GetDesiredBurnRateForStatus(status)));
            OnEnable();
            if (status == BurningStatus.Flaring) OnFlare();
            return;
        }

        if (pawn.Downed) {
            if (status > BurningStatus.Burning && (!def.canBurnWhileDowned || !willBurnWhileDowned)) {
                UpdateStatus(BurningStatus.Off);
            } else if (status == BurningStatus.Off && willBurnWhileDowned) {
                UpdateStatus(BurningStatus.Burning);
            }
        }

        if (!paused && pawn.IsAsleep() && status >= BurningStatus.Burning && !def.canBurnWhileAsleep) {
            paused = true;
            gene.UpdateBurnSource((def, GetDesiredBurnRateForStatus(BurningStatus.Off)));
            OnDisable();
        }
    }

    protected override void OnEnable() {
        base.OnEnable();
        GetOrAddHediff(pawn);
    }

    protected override void OnDisable() {
        base.OnDisable();
        OnDeFlare();
        RemoveHediff(pawn);
    }

    protected override void OnFlare() {
        base.OnFlare();
        flareStartTick = Find.TickManager.TicksGame;
        RemoveDrag(pawn);
        OnEnable();
    }

    protected override void OnDeFlare() {
        base.OnDeFlare();
        ApplyDrag(pawn, flareDuration / 3000f / 2);
        flareStartTick = -1;
    }
}