using System;
using Cosmere.Core.Nightwatcher;
using Cosmere.System.Roshar.Comp.Hediff;
using Cosmere.System.Roshar.Def;
using Cosmere.System.Roshar.Hediff;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Nightwatcher;

public class StandardCurseApplicator : ICurseApplicator {
    internal static readonly StandardCurseApplicator Instance = new();

    public void Apply(Pawn pawn, Verse.Def def, NightwatcherApplicationContext? context = null) {
        NightwatcherCurseDef curse = (NightwatcherCurseDef)def;
        ApplyHediff(pawn, curse);
        ApplyTraitForce(pawn, curse);
        ApplyTraitStrip(pawn, curse);
        ApplySkillPenalty(pawn, curse);
    }

    private static void ApplyHediff(Pawn pawn, NightwatcherCurseDef curse) {
        if (curse.hediff != null) {
            EnsureHediffPersistence(curse.hediff);
            HediffWithComps cursePassive = (HediffWithComps)HediffMaker.MakeHediff(curse.hediff, pawn);
            pawn.health.AddHediff(cursePassive);

            if (curse.cultivationEvolutionDays > 0) {
                HediffEvolution? evoComp = cursePassive.TryGetComp<HediffEvolution>();
                evoComp?.Initialize(curse);
            }
            return;
        }

        HediffDef? curseHediffDef = HediffDefOf.Cosmere_Roshar_Hediff_NightwatcherCurse;
        if (curseHediffDef != null) {
            EnsureHediffPersistence(curseHediffDef);
            NightwatcherCurseHediff curseHediff = (NightwatcherCurseHediff)HediffMaker.MakeHediff(curseHediffDef, pawn);
            curseHediff.Initialize(curse);
            pawn.health.AddHediff(curseHediff);

            if (curse.cultivationEvolutionDays > 0) {
                HediffEvolution? evoComp = curseHediff.TryGetComp<HediffEvolution>();
                evoComp?.Initialize(curse);
            }
        }
    }

    private static void ApplyTraitForce(Pawn pawn, NightwatcherCurseDef curse) {
        if (curse.forceTrait != null && !pawn.story.traits.HasTrait(curse.forceTrait)) {
            pawn.story.traits.GainTrait(new Trait(curse.forceTrait, curse.forceTraitDegree));
        }
    }

    private static void ApplyTraitStrip(Pawn pawn, NightwatcherCurseDef curse) {
        if (curse.stripTrait == null) return;
        Trait? existing = pawn.story.traits.GetTrait(curse.stripTrait);
        if (existing != null) pawn.story.traits.RemoveTrait(existing);
    }

    private static void ApplySkillPenalty(Pawn pawn, NightwatcherCurseDef curse) {
        if (curse.penaltySkill == null) return;
        SkillRecord record = pawn.skills.GetSkill(curse.penaltySkill);
        record.Level = Math.Max(record.Level - curse.penaltySkillLevels, 0);
    }

    private static void EnsureHediffPersistence(HediffDef def) {
        if (def.maxSeverity <= 0f) def.maxSeverity = 1f;
        if (def.initialSeverity <= 0f) def.initialSeverity = 1f;
    }
}
