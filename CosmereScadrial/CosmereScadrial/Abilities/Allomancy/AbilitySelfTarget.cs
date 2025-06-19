using RimWorld;
using Verse;
using PawnUtility = CosmereFramework.Utils.PawnUtility;

namespace CosmereScadrial.Abilities.Allomancy;

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

    public override void AbilityTick() {
        base.AbilityTick();

        if (!atLeastPassive) {
            return;
        }

        if (PawnUtility.IsAsleep(pawn) && status > BurningStatus.Passive) {
            UpdateStatus(def.canBurnWhileAsleep ? BurningStatus.Passive : BurningStatus.Off);
        } else if (!PawnUtility.IsAsleep(pawn) && status == BurningStatus.Passive) {
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