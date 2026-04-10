using System;
using System.Collections.Generic;
using Cosmere.Core.Savant;
using Cosmere.System.Roshar.Comp.Game;
using Cosmere.System.Roshar.Def;
using Cosmere.System.Roshar.Gene;
using Cosmere.System.Scadrial.Def;
using Cosmere.System.Scadrial.Gene;
using RimWorld;
using Verse;
using Logger = Cosmere.Core.Logger;

namespace Cosmere.System.Roshar;

public class LighteyesApplicator : IBoonApplicator {
    public void Apply(Verse.Pawn pawn, NightwatcherBoonDef def) {
        if (!Utility.CasteUtility.IsDarkeyes(pawn)) return;
        Utility.CasteUtility.DarkeyesToLighteyes(pawn, "Cosmere_Roshar_Gene_Dahn_Low");
        Logger.Info($"LighteyesApplicator: transitioned {pawn.NameShortColored} to lighteyes");
    }
}

public class HealChronicApplicator : IBoonApplicator {
    private static readonly HashSet<string> ChronicHediffs = [
        "ChronicPain", "Asthma", "BadBack", "Carcinoma",
        "Frail", "HearingLoss", "Cataract",
        "Cosmere_Roshar_Hediff_NW_CursePassive_ChronicPain",
    ];

    public void Apply(Verse.Pawn pawn, NightwatcherBoonDef def) {
        List<Verse.Hediff> hediffs = pawn.health.hediffSet.hediffs;
        for (int i = 0; i < hediffs.Count; i++) {
            if (!ChronicHediffs.Contains(hediffs[i].def.defName)) continue;
            pawn.health.RemoveHediff(hediffs[i]);
            Logger.Info($"HealChronicApplicator: removed {hediffs[i].def.defName} from {pawn.NameShortColored}");
            return;
        }
    }
}

public class MemoryLossApplicator : ICurseApplicator {
    public void Apply(Verse.Pawn pawn, NightwatcherCurseDef def) {
        if (pawn.skills == null) return;
        List<SkillRecord> skills = pawn.skills.skills;
        for (int i = 0; i < skills.Count; i++) {
            skills[i].Level = 4;
            skills[i].xpSinceLastLevel = 0f;
            skills[i].xpSinceMidnight = 0f;
        }
        Logger.Info($"MemoryLossApplicator: reset all skills to max 4 for {pawn.NameShortColored}");
    }
}

public class NarcolepsyApplicator : ICurseApplicator {
    public void Apply(Verse.Pawn pawn, NightwatcherCurseDef def) {
        NarcolepsyTracker.Instance?.Register(pawn);
    }
}

public class GriefReliefApplicator : IBoonApplicator {
    public void Apply(Verse.Pawn pawn, NightwatcherBoonDef def) {
        if (pawn.needs?.mood?.thoughts?.memories == null) return;
        List<Thought_Memory> memories = pawn.needs.mood.thoughts.memories.Memories;
        for (int i = memories.Count - 1; i >= 0; i--) {
            if (memories[i].MoodOffset() < 0f)
                pawn.needs.mood.thoughts.memories.RemoveMemory(memories[i]);
        }
        Logger.Info($"GriefReliefApplicator: cleared negative memories for {pawn.NameShortColored}");
    }
}

public class MistbornApplicator : IBoonApplicator {
    public void Apply(Verse.Pawn pawn, NightwatcherBoonDef def) {
        Scadrial.Utility.GeneUtility.AddMistborn(pawn, false, true, "Nightwatcher's boon");
        Logger.Info($"MistbornApplicator: granted Mistborn to {pawn.NameShortColored}");
    }
}

public class MistingApplicator : IBoonApplicator {
    public static MetallicArtsMetalDef? SelectedMetal;

    public void Apply(Verse.Pawn pawn, NightwatcherBoonDef def) {
        if (SelectedMetal == null) return;
        Scadrial.Utility.GeneUtility.AddGene(
            pawn,
            Scadrial.GeneDefOf.GetMistingGeneForMetal(SelectedMetal),
            false,
            true
        );
        Logger.Info($"MistingApplicator: granted Misting ({SelectedMetal.LabelCap}) to {pawn.NameShortColored}");
        SelectedMetal = null;
    }
}

public class FullFeruchemistApplicator : IBoonApplicator {
    public void Apply(Verse.Pawn pawn, NightwatcherBoonDef def) {
        Scadrial.Utility.GeneUtility.AddFullFeruchemist(pawn, false, true, "Nightwatcher's boon");
        Logger.Info($"FullFeruchemistApplicator: granted Full Feruchemist to {pawn.NameShortColored}");
    }
}

public class FerringApplicator : IBoonApplicator {
    public static MetallicArtsMetalDef? SelectedMetal;

    public void Apply(Verse.Pawn pawn, NightwatcherBoonDef def) {
        if (SelectedMetal == null) return;
        Scadrial.Utility.GeneUtility.AddGene(
            pawn,
            Scadrial.GeneDefOf.GetFerringGeneForMetal(SelectedMetal),
            false,
            true
        );
        Logger.Info($"FerringApplicator: granted Ferring ({SelectedMetal.LabelCap}) to {pawn.NameShortColored}");
        SelectedMetal = null;
    }
}

public class SavantCurseApplicator : ICurseApplicator {
    public void Apply(Verse.Pawn pawn, NightwatcherCurseDef def) {
        if (pawn.genes == null) return;

        List<Verse.Gene> allGenes = pawn.genes.GenesListForReading;

        List<Allomancer> allomancerGenes = [];
        List<Feruchemist> feruchemistGenes = [];
        List<Surgebinder> surgebinderGenes = [];

        for (int i = 0; i < allGenes.Count; i++) {
            if (allGenes[i] is Allomancer allomancer && SavantUtility.CanBeSavant(allomancer.metal)) {
                allomancerGenes.Add(allomancer);
            } else if (allGenes[i] is Feruchemist feruchemist && SavantUtility.CanBeSavant(feruchemist.metal)) {
                feruchemistGenes.Add(feruchemist);
            } else if (allGenes[i] is Surgebinder surgebinder) {
                surgebinderGenes.Add(surgebinder);
            }
        }

        int totalCandidates = allomancerGenes.Count + feruchemistGenes.Count + surgebinderGenes.Count;
        if (totalCandidates == 0) return;

        int pick = Rand.Range(0, totalCandidates);

        if (pick < allomancerGenes.Count) {
            Allomancer chosen = allomancerGenes[pick];
            ApplySavantHediffs(pawn,
                SavantUtility.GetAllomanticSavantHediffDef(chosen.metal),
                SavantUtility.GetAllomanticPermanentHediffDef(chosen.metal));
            Logger.Info($"SavantCurseApplicator: forced {pawn.NameShortColored} to allomantic savant for {chosen.metal.defName}");
            return;
        }
        pick -= allomancerGenes.Count;

        if (pick < feruchemistGenes.Count) {
            Feruchemist chosen = feruchemistGenes[pick];
            ApplySavantHediffs(pawn,
                SavantUtility.GetFeruchemicalSavantHediffDef(chosen.metal),
                SavantUtility.GetFeruchemicalPermanentHediffDef(chosen.metal));
            Logger.Info($"SavantCurseApplicator: forced {pawn.NameShortColored} to feruchemical savant for {chosen.metal.defName}");
            return;
        }
        pick -= feruchemistGenes.Count;

        Surgebinder chosenSurgebinder = surgebinderGenes[pick];
        List<SurgeDef> surges = chosenSurgebinder.radiantOrderDef.surges;
        if (surges.Count == 0) return;
        SurgeDef chosenSurge = surges[Rand.Range(0, surges.Count)];
        ApplySavantHediffs(pawn,
            SavantUtility.GetSurgeSavantHediffDef(chosenSurge.defName),
            SavantUtility.GetSurgePermanentHediffDef(chosenSurge.defName));
        Logger.Info($"SavantCurseApplicator: forced {pawn.NameShortColored} to surgebinding savant for {chosenSurge.defName}");
    }

    private static void ApplySavantHediffs(Verse.Pawn pawn, HediffDef? savantDef, HediffDef? permanentDef) {
        if (savantDef != null) {
            Verse.Hediff savant = pawn.health.hediffSet.GetFirstHediffOfDef(savantDef)
                ?? HediffMaker.MakeHediff(savantDef, pawn);
            savant.Severity = SavantUtility.SeverityForStage(3);
            if (!pawn.health.hediffSet.HasHediff(savantDef)) pawn.health.AddHediff(savant);
        }

        if (permanentDef != null && !pawn.health.hediffSet.HasHediff(permanentDef)) {
            pawn.health.AddHediff(HediffMaker.MakeHediff(permanentDef, pawn));
        }
    }
}
