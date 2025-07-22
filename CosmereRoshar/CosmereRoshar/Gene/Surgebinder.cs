using System;
using Cosmere.Core.Gene;
using Cosmere.Core.Need;
using Cosmere.Framework.Extension;
using Cosmere.Roshar.Def;
using Cosmere.Roshar.DefModExtension;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.Roshar.Gene;

public class Surgebinder : Invested {
    private int currentIdealInt;

    public int currentIdeal {
        get => currentIdealInt;
        set {
            currentIdealInt = value;
            OnIdealChange();
        }
    }

    public RadiantOrder radiantOrder => def.GetModExtension<RadiantOrder>();
    public RadiantOrderDef radiantOrderDef => radiantOrder.order;
    protected override Color BarColor => radiantOrderDef.gemstone.color.SaturationChanged(1f);
    protected override Color BarHighlightColor => radiantOrderDef.gemstone.color.SaturationChanged(2f);
    private Investiture investiture => pawn.needs.TryGetNeed<Investiture>();

    public override float Max => investiture.MaxLevel;
    public override float Value => investiture.CurLevel;

    private void OnIdealChange() {
        pawn.story.TryAddTrait(radiantOrder.trait, currentIdealInt);
        UpdateAbilities();
    }

    internal void UpdateAbilities() {
        foreach (AbilityDef abilityDef in radiantOrderDef.abilities) {
            pawn.abilities.GainAbility(abilityDef);
        }

        foreach (SurgeDef surgeDef in radiantOrderDef.surges) {
            foreach (AbilityDef surgeDefAbility in surgeDef.abilities) {
                pawn.abilities.GainAbility(surgeDefAbility);
            }
        }

        for (var i = 0; i < Math.Min(currentIdealInt, radiantOrderDef.surges.Count); i++) {
            var ideal = radiantOrderDef.ideals[i];
            foreach (AbilityDef idealAbility in ideal.abilities) {
                pawn.abilities.GainAbility(idealAbility);
            }
        }
    }

    public override void ExposeData() {
        base.ExposeData();
        Scribe_Values.Look(ref currentIdealInt, "currentIdeal");
    }
}