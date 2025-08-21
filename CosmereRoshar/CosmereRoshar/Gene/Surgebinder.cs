using System;
using Cosmere.Core.Gene;
using Cosmere.Roshar.Comp.Thing;
using Cosmere.Roshar.Def;
using Cosmere.Roshar.DefModExtension;
using RimWorld;
using UnityEngine;
using Verse;
using Logger = Cosmere.Foundation.Logger;

namespace Cosmere.Roshar.Gene;

public class Surgebinder : Invested {
    private static readonly List<int> SkillRequirements = [0, 2, 6, 10, 14];

    private int currentIdealInt;

    public int currentIdeal {
        get => currentIdealInt;
        set {
            currentIdealInt = Math.Clamp(value, 0, 4);
            OnIdealChange();
        }
    }

    public int currentIdealDisplay => currentIdeal + 1;

    public RadiantOrder radiantOrder => def.GetModExtension<RadiantOrder>();
    public RadiantOrderDef radiantOrderDef => radiantOrder.order;
    protected override Color BarColor => radiantOrderDef.color.SaturationChanged(1f);
    protected override Color BarHighlightColor => radiantOrderDef.color.SaturationChanged(2f);
    private SkillRecord skill => pawn.skills.GetSkill(SkillDefOf.Cosmere_Roshar_Skill_SurgebindingPower);
    private PawnTracker tracker => pawn.TryGetComp<PawnTracker>();
    public override List<AbilityDef> abilities => radiantOrderDef.GetAbilities(currentIdeal).ToList();

    public override float Max => investiture.MaxLevel;
    public override float Value => investiture.CurLevel;

    private void OnIdealChange() {
        pawn.story.TryAddTrait(radiantOrder.trait, currentIdealInt);
        UpdateAbilities();

        // Give the pawn a bump.... I mean, give them some investiture
        investiture.CurLevel += Mathf.Pow(10, currentIdealInt);

        // Update their drain rate
        investitureHolder.drainRate = GetDrainRate();
    }

    private float GetDrainRate() {
        if (currentIdeal >= 4) return 0f;

        float totalDrainTimeInSeconds = (10 * 60).TicksToSeconds();

        // This is the flat rate of investiture lost per real-time second
        float baseRate = 1f / totalDrainTimeInSeconds;

        // 1st = 1.0x, 2nd = 0.75x, 3rd = 0.5x, 4th = 0.25x, 5th = 0x
        float idealMultiplier = 1f - currentIdeal / 4f;

        return baseRate * idealMultiplier;
    }

    public override void TickInterval(int delta) {
        base.TickInterval(delta);

        TryLevelUp(delta);
        TrySkillUp(delta);
    }

    /// <summary>
    ///     The pawn slowly just levels up their Surgebinding skill from talking with their
    ///     spren. 5 xp every 2000 ticks is pretty slow, but will eventually get pawns
    ///     leveled up, and on their way to higher ideals
    /// </summary>
    /// <param name="delta"></param>
    private void TrySkillUp(int delta) {
        if (!pawn.IsHashIntervalTick(GenTicks.TickLongInterval, delta)) return;
        if (currentIdeal >= 2) return;

        skill.Learn(5, true, true);
    }

    private void TryLevelUp(int delta) {
        if (!pawn.IsHashIntervalTick(GenTicks.TickLongInterval, delta)) return;
        if (currentIdeal == 4) return;
        if (skill.Level < SkillRequirements[currentIdeal + 1]) return;
        if (!radiantOrderDef.idealChecker.IsSatisfied(pawn, this, currentIdeal + 1)) return;

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
        skill.Level = 0;
        foreach (AbilityDef unlockedAbilityDef in radiantOrderDef.GetAbilities(currentIdealInt)) {
            pawn.abilities.RemoveAbility(unlockedAbilityDef);
        }
    }

    internal void UpdateAbilities() {
        foreach (AbilityDef unlockedAbilityDef in radiantOrderDef.GetAbilities(currentIdealInt)) {
            pawn.abilities.GainAbility(unlockedAbilityDef);
        }
    }

    public override void ExposeData() {
        base.ExposeData();
        Scribe_Values.Look(ref currentIdealInt, "currentIdeal");
    }
}