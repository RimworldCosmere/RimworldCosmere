using System;
using Cosmere.Core.Ability;
using Cosmere.Core.Def;
using Cosmere.Core.Util;
using Cosmere.System.Roshar.Def;
using Cosmere.System.Roshar.GameCondition;
using Cosmere.System.Roshar.Gene;
using Cosmere.System.Roshar.Savant;
using Cosmere.System.Roshar.Surgebinding.Hediff;
using Cosmere.System.Roshar.Surgebinding.Util;
using Verse;
using AbilityDef = RimWorld.AbilityDef;

namespace Cosmere.System.Roshar.Surgebinding.Ability;

public class SurgebindingAbility : AbstractAbility<Surgebinder, SurgebindingHediff> {
    private SurgeDef? cachedSurgeDef;

    public SurgebindingAbility(Pawn pawn) : base(pawn) { }

    public SurgebindingAbility(Pawn pawn, AbilityDef def) : base(pawn, def) { }

    public RadiantOrderDef radiantOrder {
        get {
            if (def.radiantOrder != null) return def.radiantOrder;
            RadiantOrderDef? order = pawn.GetRadiantOrder();
            if (order == null) throw new InvalidOperationException($"Pawn {pawn.Name} has no RadiantOrder assigned");
            return order;
        }
    }

    public GemDef gem => radiantOrder.gemstone;

    public override Surgebinder Gene => cachedGene ??= pawn.genes.GetSurgebindingGeneForOrder(radiantOrder)!;

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

    public override AcceptanceReport CanCast {
        get {
            if (!ShardUtility.AreAnyEnabled(ShardDefOf.Honor)) {
                return "CRO_Surgebinding_HonorNotPresent".Translate();
            }

            return base.CanCast;
        }
    }

    public override bool GizmosVisible() {
        return base.GizmosVisible() &&
               pawn.genes.HasSurgebindingGeneForOrder(radiantOrder) &&
               ShardUtility.AreAnyEnabled(ShardDefOf.Honor);
    }

    public override float GetStrength(Status? desiredStatus = null) {
        float baseStrength = base.GetStrength(desiredStatus);
        if (surgeDef == null) return baseStrength;
        int stage = SurgebindingSavantUtility.GetSavantStage(pawn, surgeDef);
        return baseStrength * SurgebindingSavantUtility.GetPowerMultiplier(stage);
    }

    public override float GetDesiredBurnRateForStatus(Status? desiredStatus) {
        float rate = base.GetDesiredBurnRateForStatus(desiredStatus) / (1 << Gene.CurrentIdeal);
        if (surgeDef != null) {
            int stage = SurgebindingSavantUtility.GetSavantStage(pawn, surgeDef);
            rate *= SurgebindingSavantUtility.GetCostMultiplier(stage);
        }

        if (pawn.Map != null) {
            rate *= SuppressionField.GetCostMultiplier(pawn.Map);
        }

        return rate;
    }

    protected override void OnEnable() {
        base.OnEnable();
        if (def.hediff != null && !def.targetRequired) {
            SurgebindingHediffUtility.GetOrAddHediff(pawn, this, def.hediff);
        }
    }

    protected override void OnDisable() {
        if (def.hediff != null && !def.targetRequired) {
            SurgebindingHediffUtility.RemoveHediff(pawn, this, def.hediff);
        }

        base.OnDisable();
    }
}
