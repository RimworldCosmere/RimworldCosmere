using System;
using Cosmere.Core.Gene;
using Cosmere.Core.Need;
using Cosmere.Roshar.Comp.Thing;
using Cosmere.Roshar.Def;
using Cosmere.Roshar.DefModExtension;
using RimWorld;
using UnityEngine;
using Verse;
using Logger = Cosmere.Framework.Logger;

namespace Cosmere.Roshar.Gene;

public class Surgebinder : Invested {
    private const float ExperienceMultiplier = 5f;
    private const float LevelNewSquire = 2;
    private const float LevelExperiencedSquire = 6;
    private const float LevelKnightRadiant = 10;
    private const float LevelKnightRadiantMaster = 14;
    private const float MaxExperience = 18;

    private int currentIdealInt;

    public int currentIdeal {
        get => currentIdealInt;
        set {
            currentIdealInt = Math.Clamp(value, 0, 4);
            OnIdealChange();
        }
    }

    public RadiantOrder radiantOrder => def.GetModExtension<RadiantOrder>();
    public RadiantOrderDef radiantOrderDef => radiantOrder.order;
    protected override Color BarColor => radiantOrderDef.gemstone.color.SaturationChanged(1f);
    protected override Color BarHighlightColor => radiantOrderDef.gemstone.color.SaturationChanged(2f);
    private Investiture investiture => pawn.needs.TryGetNeed<Investiture>();
    private SkillRecord skill => pawn.skills.GetSkill(SkillDefOf.Cosmere_Roshar_Skill_SurgebindingPower);
    private PawnTracker tracker => pawn.TryGetComp<PawnTracker>();

    public override float Max => investiture.MaxLevel;
    public override float Value => investiture.CurLevel;

    private void OnIdealChange() {
        pawn.story.TryAddTrait(radiantOrder.trait, currentIdealInt);
        UpdateAbilities();
        investiture.CurLevel += Mathf.Pow(10, currentIdealInt);
    }

    public override void TickInterval(int delta) {
        base.TickInterval(delta);

        if (!pawn.IsHashIntervalTick(GenTicks.TickLongInterval)) return;
        if (currentIdeal == 4) return;
        if (!radiantOrderDef.idealChecker.IsSatisfied(pawn, this, currentIdeal, currentIdeal + 1)) return;

        currentIdeal++;
        Messages.Message(
            "CRO_Gene_LevelUp".Translate(
                    pawn.NameFullColored.Named("PAWN"),
                    radiantOrderDef.LabelCap.Colorize(ColoredText.GeneColor).Named("ORDER")
                )
                .Resolve(),
            pawn,
            MessageTypeDefOf.PositiveEvent
        );
    }

    public override void PostAdd() {
        skill.Level = 0;
        base.PostAdd();
        OnIdealChange();
        Logger.Verbose($"{pawn.NameFullColored} has become a {radiantOrderDef.LabelCap}");
    }

    public override void PostRemove() {
        foreach (AbilityDef unlockedAbilityDef in GetUnlockedAbilityDefs()) {
            pawn.abilities.RemoveAbility(unlockedAbilityDef);
        }
    }

    internal void UpdateAbilities() {
        foreach (AbilityDef unlockedAbilityDef in GetUnlockedAbilityDefs()) {
            pawn.abilities.GainAbility(unlockedAbilityDef);
        }
    }

    private IEnumerable<AbilityDef> GetUnlockedAbilityDefs() {
        foreach (AbilityDef abilityDef in radiantOrderDef.abilities) {
            yield return abilityDef;
        }

        foreach (AbilityDef surgeDefAbility in radiantOrderDef.surges.SelectMany(surgeDef => surgeDef.abilities)) {
            yield return surgeDefAbility;
        }

        for (int i = 0; i < Math.Min(currentIdealInt, radiantOrderDef.surges.Count); i++) {
            Ideal? ideal = radiantOrderDef.ideals[i];
            foreach (AbilityDef idealAbility in ideal.abilities) {
                yield return idealAbility;
            }
        }
    }

    public override void ExposeData() {
        base.ExposeData();
        Scribe_Values.Look(ref currentIdealInt, "currentIdeal");
    }
}