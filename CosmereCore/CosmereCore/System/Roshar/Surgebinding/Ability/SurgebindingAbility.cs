using Cosmere.Core.Ability;
using Cosmere.Core.Def;
using Cosmere.Core.Savant;
using Cosmere.Core.Util;
using Cosmere.System.Roshar.Def;
using Cosmere.System.Roshar.Gene;
using Cosmere.System.Roshar.Surgebinding.Hediff;
using RimWorld;
using Verse;
using AbilityDef = RimWorld.AbilityDef;

namespace Cosmere.System.Roshar.Surgebinding.Ability;

public class SurgebindingAbility : AbstractAbility<Surgebinder, SurgebindingHediff> {
    public SurgebindingAbility(Pawn pawn) : base(pawn) { }
    public SurgebindingAbility(Pawn pawn, AbilityDef def) : base(pawn, def) { }

    public RadiantOrderDef radiantOrder => def.radiantOrder ?? pawn.GetRadiantOrder()!;
    public GemDef gem => radiantOrder.gemstone;

    public override Surgebinder gene => cachedGene ??= pawn.genes.GetSurgebindingGeneForOrder(radiantOrder)!;

    private SurgeDef? cachedSurgeDef;

    private SurgeDef? surgeDef {
        get {
            if (cachedSurgeDef != null) return cachedSurgeDef;
            List<SurgeDef> surges = radiantOrder.surges;
            for (int i = 0; i < surges.Count; i++) {
                List<AbilityDef> surgeAbilities = surges[i].abilities;
                for (int j = 0; j < surgeAbilities.Count; j++) {
                    if (surgeAbilities[j] == base.def) {
                        cachedSurgeDef = surges[i];
                        return cachedSurgeDef;
                    }
                }
            }
            return null;
        }
    }

    public new SurgebindingAbilityDef def {
        get => (SurgebindingAbilityDef)base.def;
        set => base.def = value;
    }

    public new bool GizmosVisible() {
        return base.GizmosVisible()
               && pawn.genes.HasSurgebindingGeneForOrder(radiantOrder)
               && ShardUtility.AreAnyEnabled(ShardDefOf.Honor);
    }

    public override AcceptanceReport CanCast {
        get {
            if (!ShardUtility.AreAnyEnabled(ShardDefOf.Honor)) {
                return "Honor is not present.";
            }
            return base.CanCast;
        }
    }

    public override float GetStrength(Status? desiredStatus = null) {
        float baseStrength = base.GetStrength(desiredStatus);
        if (surgeDef == null) return baseStrength;
        int stage = SavantUtility.GetSurgebindingSavantStage(pawn, surgeDef);
        return baseStrength * SavantUtility.GetSurgebindingPowerMultiplier(stage);
    }

    public override float GetDesiredBurnRateForStatus(Status? desiredStatus) {
        float rate = base.GetDesiredBurnRateForStatus(desiredStatus) / (1 << gene.currentIdeal);
        if (surgeDef != null) {
            int stage = SavantUtility.GetSurgebindingSavantStage(pawn, surgeDef);
            rate *= SavantUtility.GetSurgebindingCostMultiplier(stage);
        }
        if (pawn.Map != null) {
            rate *= GameCondition.SuppressionField.GetCostMultiplier(pawn.Map);
        }
        return rate;
    }

    protected override void OnEnable() {
        base.OnEnable();
        if (def.hediff != null && !def.targetRequired) {
            Utility.SurgebindingHediffUtility.GetOrAddHediff(pawn, this, def.hediff);
        }
    }

    protected override void OnDisable() {
        if (def.hediff != null && !def.targetRequired) {
            Utility.SurgebindingHediffUtility.RemoveHediff(pawn, this, def.hediff);
        }
        base.OnDisable();
    }
}