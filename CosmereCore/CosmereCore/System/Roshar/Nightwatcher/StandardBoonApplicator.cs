using System;
using Cosmere.Core.Comp.Game;
using Cosmere.Core.Comp.Thing;
using Cosmere.Core.Nightwatcher;
using Cosmere.System.Roshar.Comp.Game;
using Cosmere.System.Roshar.Comp.Thing;
using Cosmere.System.Roshar.Def;
using Cosmere.System.Roshar.Hediff;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Nightwatcher;

public class StandardBoonApplicator : IBoonApplicator {
    internal static readonly StandardBoonApplicator Instance = new();

    public void Apply(Pawn pawn, Verse.Def def, NightwatcherApplicationContext? context = null) {
        NightwatcherBoonDef boon = (NightwatcherBoonDef)def;
        ApplyHediff(pawn, boon);
        ApplySkillBoosts(pawn, boon);
        ApplyTraitGrant(pawn, boon);
        ApplyTraitRemove(pawn, boon);
        ApplyHediffRemove(pawn, boon);
        ApplyInvestiture(pawn, boon);
        ApplySurgebindingConnection(pawn, boon);
        ApplyPsylink(pawn, boon);
    }

    private static void ApplyHediff(Pawn pawn, NightwatcherBoonDef boon) {
        if (boon.hediff != null) {
            EnsureHediffPersistence(boon.hediff);
            pawn.health.AddHediff(HediffMaker.MakeHediff(boon.hediff, pawn));
            return;
        }

        HediffDef? boonHediffDef = HediffDefOf.Cosmere_Roshar_Hediff_NightwatcherBoon;
        if (boonHediffDef != null) {
            EnsureHediffPersistence(boonHediffDef);
            NightwatcherBoonHediff boonHediff = (NightwatcherBoonHediff)HediffMaker.MakeHediff(boonHediffDef, pawn);
            boonHediff.Initialize(boon);
            pawn.health.AddHediff(boonHediff);
        }
    }

    private static void ApplySkillBoosts(Pawn pawn, NightwatcherBoonDef boon) {
        for (int i = 0; i < boon.skillBoosts.Count; i++) {
            BoonSkillBoost boost = boon.skillBoosts[i];
            SkillRecord record = pawn.skills.GetSkill(boost.skill);
            record.Level = Math.Min(record.Level + boost.levels, 20);
        }
    }

    private static void ApplyTraitGrant(Pawn pawn, NightwatcherBoonDef boon) {
        if (boon.grantTrait != null && !pawn.story.traits.HasTrait(boon.grantTrait)) {
            pawn.story.traits.GainTrait(new Trait(boon.grantTrait, boon.grantTraitDegree));
        }
    }

    private static void ApplyTraitRemove(Pawn pawn, NightwatcherBoonDef boon) {
        if (boon.removeTrait == null) return;
        Trait? existing = pawn.story.traits.GetTrait(boon.removeTrait);
        if (existing != null) pawn.story.traits.RemoveTrait(existing);
    }

    private static void ApplyHediffRemove(Pawn pawn, NightwatcherBoonDef boon) {
        if (boon.removeHediff == null) return;
        Verse.Hediff? existing = pawn.health.hediffSet.GetFirstHediffOfDef(boon.removeHediff);
        if (existing != null) pawn.health.RemoveHediff(existing);
    }

    private static void ApplyInvestiture(Pawn pawn, NightwatcherBoonDef boon) {
        if (boon.investitureBonus <= 0f) return;
        InvestitureHolder? holder = pawn.TryGetComp<InvestitureHolder>();
        if (holder != null) holder.maxInvestitureSelf += boon.investitureBonus;
    }

    private static void ApplySurgebindingConnection(Pawn pawn, NightwatcherBoonDef boon) {
        if (boon.surgebindingConnectionBoost <= 0f) return;
        NightwatcherVisit? nwComp = pawn.TryGetComp<NightwatcherVisit>();
        if (nwComp != null) nwComp.cultivationConnectionBonus += boon.surgebindingConnectionBoost;

        CultivationEntity? entity = CultivationEntity.Instance;
        if (entity != null) {
            SpiritWeb spiritWeb = Current.Game.GetComponent<SpiritWeb>();
            spiritWeb?.AdjustConnection(entity, pawn, boon.surgebindingConnectionBoost);
        }
    }

    private static void ApplyPsylink(Pawn pawn, NightwatcherBoonDef boon) {
        if (!boon.psylinkBoost) return;
        HediffDef? psylinkDef = DefDatabase<HediffDef>.GetNamedSilentFail("PsychicAmplifier");
        if (psylinkDef == null) return;

        Hediff_Psylink? psylink = pawn.health.hediffSet.GetFirstHediffOfDef(psylinkDef) as Hediff_Psylink;
        if (psylink == null) {
            psylink = HediffMaker.MakeHediff(psylinkDef, pawn) as Hediff_Psylink;
            if (psylink != null) pawn.health.AddHediff(psylink);
        }

        psylink?.ChangeLevel(1);
    }

    private static void EnsureHediffPersistence(HediffDef def) {
        if (def.maxSeverity <= 0f) def.maxSeverity = 1f;
        if (def.initialSeverity <= 0f) def.initialSeverity = 1f;
    }
}
