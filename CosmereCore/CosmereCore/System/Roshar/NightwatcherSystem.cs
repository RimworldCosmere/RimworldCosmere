using System;
using System.Collections.Generic;
using Cosmere.Core.Comp.Game;
using Cosmere.Core.Comp.Thing;
using Cosmere.System.Roshar.Comp.Hediff;
using Cosmere.System.Roshar.Comp.Thing;
using Cosmere.System.Roshar.Def;
using Cosmere.System.Roshar.Dialog;
using Cosmere.System.Roshar.Hediff;
using RimWorld;
using Verse;
using Logger = Cosmere.Core.Logger;

namespace Cosmere.System.Roshar;

public static class NightwatcherSystem {
    private const string BoonHediffDefName = "Cosmere_Roshar_Hediff_NightwatcherBoon";
    private const string CurseHediffDefName = "Cosmere_Roshar_Hediff_NightwatcherCurse";

    public static bool IsEligible(Verse.Pawn pawn) {
        if (!pawn.IsColonist) return false;
        if (pawn.Dead || pawn.Downed) return false;
        CompNightwatcher? comp = pawn.TryGetComp<CompNightwatcher>();
        if (comp == null || comp.HasVisited) return false;
        return Current.Game.GetComponent<Shards>()?.IsEnabled("Cultivation") ?? false;
    }

    public static void InitiateSeek(Verse.Pawn pawn) {
        Find.WindowStack.Add(new Dialog_NightwatcherEncounter(pawn));
    }

    private static void EnsureHediffPersistence(HediffDef def) {
        if (def.maxSeverity <= 0f) def.maxSeverity = 1f;
        if (def.initialSeverity <= 0f) def.initialSeverity = 1f;
    }

    public static void ApplyBoon(Verse.Pawn pawn, NightwatcherBoonDef boon) {
        if (boon.hediff != null) {
            EnsureHediffPersistence(boon.hediff);
            pawn.health.AddHediff(HediffMaker.MakeHediff(boon.hediff, pawn));
        } else {
            HediffDef? boonHediffDef = DefDatabase<HediffDef>.GetNamedSilentFail(BoonHediffDefName);
            if (boonHediffDef != null) {
                EnsureHediffPersistence(boonHediffDef);
                NightwatcherBoonHediff boonHediff = (NightwatcherBoonHediff)HediffMaker.MakeHediff(boonHediffDef, pawn);
                boonHediff.Initialize(boon);
                pawn.health.AddHediff(boonHediff);
            }
        }

        for (int i = 0; i < boon.skillBoosts.Count; i++) {
            BoonSkillBoost boost = boon.skillBoosts[i];
            SkillRecord record = pawn.skills.GetSkill(boost.skill);
            record.Level = Math.Min(record.Level + boost.levels, 20);
        }

        if (boon.grantTrait != null && !pawn.story.traits.HasTrait(boon.grantTrait))
            pawn.story.traits.GainTrait(new Trait(boon.grantTrait, boon.grantTraitDegree));

        if (boon.removeTrait != null) {
            Trait? existing = pawn.story.traits.GetTrait(boon.removeTrait);
            if (existing != null) pawn.story.traits.RemoveTrait(existing);
        }

        if (boon.removeHediff != null) {
            Verse.Hediff? existing = pawn.health.hediffSet.GetFirstHediffOfDef(boon.removeHediff);
            if (existing != null) pawn.health.RemoveHediff(existing);
        }

        if (boon.investitureBonus > 0f) {
            InvestitureHolder? holder = pawn.TryGetComp<InvestitureHolder>();
            if (holder != null) holder.maxInvestitureSelf += boon.investitureBonus;
        }

        if (boon.surgebindingConnectionBoost > 0f) {
            CompNightwatcher? nwComp = pawn.TryGetComp<CompNightwatcher>();
            if (nwComp != null) nwComp.cultivationConnectionBonus += boon.surgebindingConnectionBoost;

            Comp.Game.CultivationEntity? entity = Comp.Game.CultivationEntity.Instance;
            if (entity != null) {
                Core.Comp.Game.SpiritWeb spiritWeb = Current.Game.GetComponent<Core.Comp.Game.SpiritWeb>();
                spiritWeb?.AdjustConnection(entity, pawn, boon.surgebindingConnectionBoost);
            }
        }

        if (boon.psylinkBoost) {
            HediffDef? psylinkDef = DefDatabase<HediffDef>.GetNamedSilentFail("PsychicAmplifier");
            if (psylinkDef != null) {
                Hediff_Psylink? psylink = pawn.health.hediffSet.GetFirstHediffOfDef(psylinkDef) as Hediff_Psylink;
                if (psylink == null) {
                    psylink = HediffMaker.MakeHediff(psylinkDef, pawn) as Hediff_Psylink;
                    if (psylink != null) pawn.health.AddHediff(psylink);
                }
                psylink?.ChangeLevel(1);
            }
        }

        boon.Applicator?.Apply(pawn, boon);

        Core.Comp.Game.SpiritWeb? web = Current.Game.GetComponent<Core.Comp.Game.SpiritWeb>();
        Comp.Game.CultivationEntity? cultivation = Comp.Game.CultivationEntity.Instance;
        if (web != null && cultivation != null)
            web.AdjustConnection(cultivation, pawn, 0.1f);

        Logger.Info($"NightwatcherSystem: applied boon '{boon.defName}' to {pawn.NameShortColored}");
    }

    public static void ApplyCurse(Verse.Pawn pawn, NightwatcherCurseDef curse) {
        if (curse.hediff != null) {
            EnsureHediffPersistence(curse.hediff);
            HediffWithComps cursePassive = (HediffWithComps)HediffMaker.MakeHediff(curse.hediff, pawn);
            pawn.health.AddHediff(cursePassive);

            if (curse.cultivationEvolutionDays > 0) {
                HediffEvolutionComp? evoComp = cursePassive.TryGetComp<HediffEvolutionComp>();
                evoComp?.Initialize(curse);
            }
        } else {
            HediffDef? curseHediffDef = DefDatabase<HediffDef>.GetNamedSilentFail(CurseHediffDefName);
            if (curseHediffDef != null) {
                EnsureHediffPersistence(curseHediffDef);
                NightwatcherCurseHediff curseHediff = (NightwatcherCurseHediff)HediffMaker.MakeHediff(curseHediffDef, pawn);
                curseHediff.Initialize(curse);
                pawn.health.AddHediff(curseHediff);

                if (curse.cultivationEvolutionDays > 0) {
                    HediffEvolutionComp? evoComp = curseHediff.TryGetComp<HediffEvolutionComp>();
                    evoComp?.Initialize(curse);
                }
            }
        }

        if (curse.forceTrait != null && !pawn.story.traits.HasTrait(curse.forceTrait))
            pawn.story.traits.GainTrait(new Trait(curse.forceTrait, curse.forceTraitDegree));

        if (curse.stripTrait != null) {
            Trait? existing = pawn.story.traits.GetTrait(curse.stripTrait);
            if (existing != null) pawn.story.traits.RemoveTrait(existing);
        }

        if (curse.penaltySkill != null) {
            SkillRecord record = pawn.skills.GetSkill(curse.penaltySkill);
            record.Level = Math.Max(record.Level - curse.penaltySkillLevels, 0);
        }

        curse.Applicator?.Apply(pawn, curse);

        Logger.Info($"NightwatcherSystem: applied curse '{curse.defName}' to {pawn.NameShortColored}");
    }

    public static NightwatcherCurseDef DrawCurse(NightwatcherBoonDef boon) {
        List<NightwatcherCurseDef> eligible = [];
        List<NightwatcherCurseDef> all = DefDatabase<NightwatcherCurseDef>.AllDefsListForReading;
        for (int i = 0; i < all.Count; i++) {
            if (all[i].minBoonTier <= boon.powerTier) eligible.Add(all[i]);
        }

        if (eligible.Count == 0) {
            Logger.Warning($"NightwatcherSystem: no eligible curses for boon tier {boon.powerTier}");
            return DefDatabase<NightwatcherCurseDef>.AllDefsListForReading[0];
        }

        int tierIndex = boon.powerTier - 1;
        float totalWeight = 0f;
        for (int i = 0; i < eligible.Count; i++)
            totalWeight += eligible[i].curseWeights[tierIndex];

        float roll = Rand.Value * totalWeight;
        float cumulative = 0f;
        for (int i = 0; i < eligible.Count; i++) {
            cumulative += eligible[i].curseWeights[tierIndex];
            if (roll <= cumulative) return eligible[i];
        }

        return eligible[eligible.Count - 1];
    }
}
