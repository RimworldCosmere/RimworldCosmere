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

    public RadiantOrderDef RadiantOrder {
        get {
            if (Def.radiantOrder != null) return Def.radiantOrder;
            RadiantOrderDef? order = pawn.GetRadiantOrder();
            if (order == null) throw new InvalidOperationException($"Pawn {pawn.Name} has no RadiantOrder assigned");
            return order;
        }
    }

    public GemDef Gem => RadiantOrder.gemstone;

    public override Surgebinder Gene => cachedGene ??= pawn.genes.GetSurgebindingGeneForOrder(RadiantOrder)!;

    private SurgeDef? SurgeDef {
        get {
            if (cachedSurgeDef != null) return cachedSurgeDef;
            List<SurgeDef> surges = RadiantOrder.surges;
            for (int i = 0; i < surges.Count; i++) {
                List<AbilityDef> surgeAbilities = surges[i].abilities;
                for (int j = 0; j < surgeAbilities.Count; j++) {
                    if (surgeAbilities[j] == base.Def) {
                        cachedSurgeDef = surges[i];
                        return cachedSurgeDef;
                    }
                }
            }

            return null;
        }
    }

    public new SurgebindingAbilityDef Def {
        get => (SurgebindingAbilityDef)base.Def;
        set => base.Def = value;
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
               pawn.genes.HasSurgebindingGeneForOrder(RadiantOrder) &&
               ShardUtility.AreAnyEnabled(ShardDefOf.Honor);
    }

    public override float GetStrength(Status? desiredStatus = null) {
        float baseStrength = base.GetStrength(desiredStatus);
        if (SurgeDef == null) return baseStrength;
        int stage = SurgebindingSavantUtility.GetSavantStage(pawn, SurgeDef);
        return baseStrength * SurgebindingSavantUtility.GetPowerMultiplier(stage);
    }

    public override float GetDesiredBurnRateForStatus(Status? desiredStatus) {
        float rate = base.GetDesiredBurnRateForStatus(desiredStatus) / (1 << Gene.CurrentIdeal);
        if (SurgeDef != null) {
            int stage = SurgebindingSavantUtility.GetSavantStage(pawn, SurgeDef);
            rate *= SurgebindingSavantUtility.GetCostMultiplier(stage);
        }

        if (pawn.Map != null) {
            rate *= SuppressionField.GetCostMultiplier(pawn.Map);
        }

        return rate;
    }

    protected override void OnEnable() {
        base.OnEnable();
        if (Def.hediff != null && !Def.targetRequired) {
            SurgebindingHediffUtility.GetOrAddHediff(pawn, this, Def.hediff);
        }
    }

    protected override void OnDisable() {
        if (Def.hediff != null && !Def.targetRequired) {
            SurgebindingHediffUtility.RemoveHediff(pawn, this, Def.hediff);
        }

        base.OnDisable();
    }
}
