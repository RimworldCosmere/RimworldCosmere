using Cosmere.Core.Util;
using Cosmere.System.Scadrial.Def;
using RimWorld;
using UnityEngine;
using Verse;
using Logger = Cosmere.Core.Logger;

namespace Cosmere.System.Scadrial.Util;

public static class GeneUtility {
    private static bool isHarmony => ShardUtility.AreAnyEnabled(ShardDefOf.Harmony);

    private static bool isRuin => ShardUtility.AreAnyEnabled(ShardDefOf.Ruin);

    private static bool isPreservation => ShardUtility.AreAnyEnabled(ShardDefOf.Preservation);

    // Genes are assigned before the pawn is named, so reading a name here throws
    // on anything freshly generated. Only pawns being redressed already have one,
    // which is why this failed on some generations and not others.
    private static string GenerationLabel(Pawn pawn) {
        return pawn.Name?.ToStringShort ?? pawn.kindDef?.defName ?? "unnamed";
    }

    public static void AssignScadrialGenes(Pawn pawn) {
        if (pawn.genes == null || !pawn.RaceProps.Humanlike) {
            return;
        }

        Pawn_GeneTracker genes = pawn.genes;
        if (genes.Xenotype?.Equals(XenotypeDefOf.Cosmere_Scadrial_Xenotype_Skaa) ?? false) {
            AddHeritageGene(genes, GeneDefOf.Cosmere_Scadrial_Gene_TerrisHeritage, 0.10f);
            AddHeritageGene(genes, GeneDefOf.Cosmere_Scadrial_Gene_NobleHeritage, 0.10f);
        }

        if (genes.Xenotype?.Equals(XenotypeDefOf.Cosmere_Scadrial_Xenotype_Noble) ?? false) {
            AddHeritageGene(genes, GeneDefOf.Cosmere_Scadrial_Gene_TerrisHeritage, 0.10f);
        }

        bool isTerris = IsTerris(pawn);
        bool isNoble = IsNoble(pawn);
        bool isMetalbornBlocked = IsMetalbornBlocked(pawn);

        if (isMetalbornBlocked) {
            return;
        }

        // Preservation logic
        string roll;
        bool success;
        if (isPreservation) {
            if (isNoble) {
                success = RollChance(128, out roll);
                Logger.Verbose(
                    $"Trying for Mistborn. Pawn={GenerationLabel(pawn)} Success={Logger.ColoredBoolean(success ? Color.green : Color.red, success)} Roll={roll}"
                );
                if (success) {
                    AddMistborn(pawn);
                } else {
                    success = RollChance(16, out roll);
                    Logger.Verbose(
                        $"Trying for Misting. Pawn={GenerationLabel(pawn)} Success={Logger.ColoredBoolean(success ? Color.green : Color.red, success)} Roll={roll}"
                    );
                    if (success) AddRandomAllomanticGene(pawn);
                }
            }

            // Ruin logic
            if (!isRuin) return;
            if (!isTerris) return;

            // Full Feruchemists were way more common, from what I can tell
            // Most Terris were Full, or nothing. There was a small chance for Ferrings, but it was rare.
            success = RollChance(16, out roll);
            Logger.Verbose(
                $"Trying for full feruchemist. Pawn={GenerationLabel(pawn)} Success={Logger.ColoredBoolean(success ? Color.green : Color.red, success)} Roll={roll}"
            );
            if (success) {
                AddFullFeruchemist(pawn);
            } else {
                success = RollChance(64, out roll);
                Logger.Verbose(
                    $"Trying for Ferring. Pawn={GenerationLabel(pawn)} Success={Logger.ColoredBoolean(success ? Color.green : Color.red, success)} Roll={roll}"
                );
                if (success) AddRandomFeruchemicalGene(pawn);
            }

            return;
        }

        // Harmony system: no nobles, no block, no mistborn/full feruchemists
        if (!isHarmony) return;

        success = RollChance(16, out roll);
        Logger.Verbose(
            $"Trying for random misting. Pawn={GenerationLabel(pawn)} Success={Logger.ColoredBoolean(success ? Color.green : Color.red, success)} Roll={roll}"
        );
        if (success) {
            AddRandomAllomanticGene(pawn);
        }

        if (!isTerris) return;

        success = RollChance(16, out roll);
        Logger.Verbose(
            $"Trying for random ferring. Pawn={GenerationLabel(pawn)} Success={Logger.ColoredBoolean(success ? Color.green : Color.red, success)} Roll={roll}"
        );
        if (success) AddRandomFeruchemicalGene(pawn);
    }

    public static void EnforceSkaaPurityInheritance(Pawn generated, Pawn other, PawnGenerationRequest request) {
        // Only applies to humanlike pawns
        if (!generated.RaceProps.Humanlike || generated.genes == null) return;

        // Identify both parents
        Pawn? parent1 = request.FixedLastName != null ? generated.GetFather() : other;
        Pawn? parent2 = request.FixedLastName != null ? other : generated.GetMother();

        if (parent1 == null || parent2 == null) return;

        GeneDef skaaPurity = GeneDefOf.Cosmere_Scadrial_Gene_SkaaPurity;
        bool parent1Skaa =
            parent1.genes?.HasActiveGene(skaaPurity) == true;
        bool parent2Skaa =
            parent2.genes?.HasActiveGene(skaaPurity) == true;
        bool childSkaa = generated.genes.HasActiveGene(skaaPurity);

        // Remove if child inherited it but only one parent had it
        if (!childSkaa || parent1Skaa && parent2Skaa) return;
        generated.genes.RemoveGene(skaaPurity);
        Logger.Verbose(
            $"Removed {skaaPurity.defName} from {generated.NameFullColored} (only one parent had it)"
        );
    }

    public static void AddRandomAllomanticGene(Pawn pawn, bool canSnap = true, bool snapped = false) {
        List<MetallicArtsMetalDef> allDefs = DefDatabase<MetallicArtsMetalDef>.AllDefsListForReading;
        List<MetallicArtsMetalDef> candidates = [];
        for (int i = 0; i < allDefs.Count; i++) {
            if (allDefs[i].allomancy != null) candidates.Add(allDefs[i]);
        }

        if (candidates.Count == 0) return;
        MetallicArtsMetalDef metal = candidates.RandomElement();
        AddGene(pawn, GeneDefOf.GetMistingGeneForMetal(metal), canSnap, snapped);
    }

    public static void AddRandomFeruchemicalGene(Pawn pawn, bool canSnap = true, bool snapped = false) {
        List<MetallicArtsMetalDef> allDefs = DefDatabase<MetallicArtsMetalDef>.AllDefsListForReading;
        List<MetallicArtsMetalDef> candidates = [];
        for (int i = 0; i < allDefs.Count; i++) {
            if (allDefs[i].feruchemy != null) candidates.Add(allDefs[i]);
        }

        if (candidates.Count == 0) return;
        MetallicArtsMetalDef metal = candidates.RandomElement();
        AddGene(pawn, GeneDefOf.GetFerringGeneForMetal(metal), canSnap, snapped);
    }

    public static void AddGene(Pawn pawn, GeneDef? gene, bool canSnap = true, bool snapped = false) {
        if (gene == null) return;
        if (pawn.genes.HasActiveGene(gene)) return;

        if (!pawn.TryGetComp(out Core.Comp.Thing.DormantConnection dormantConnection)) snapped = true;
        if (snapped) {
            SnapUtility.Snap(pawn);
        } else if (canSnap && Rand.Chance(1f / 16f)) {
            SnapUtility.Snap(pawn);
            snapped = true;
        }

        if (snapped) {
            pawn.genes.EnsureGene(gene);
        } else {
            dormantConnection.AddHiddenGene(gene);
        }
    }

    public static void AddMistborn(Pawn pawn, bool canSnap = true, bool snapped = false, string? snapCause = null) {
        if (snapped) {
            SnapUtility.Snap(pawn, snapCause);
        } else if (canSnap && Rand.Chance(1f / 16f)) {
            SnapUtility.Snap(pawn, snapCause);
            snapped = true;
        }

        List<MetallicArtsMetalDef> allDefs = DefDatabase<MetallicArtsMetalDef>.AllDefsListForReading;
        for (int i = 0; i < allDefs.Count; i++) {
            if (allDefs[i].allomancy?.userName != null) {
                AddGene(pawn, GeneDefOf.GetMistingGeneForMetal(allDefs[i]), false, snapped);
            }
        }

        pawn.story.EnsureTrait(TraitDefOf.Cosmere_Scadrial_Trait_Mistborn);
    }

    public static void AddFullFeruchemist(
        Pawn pawn,
        bool canSnap = true,
        bool snapped = false,
        string? snapCause = null
    ) {
        if (snapped) {
            SnapUtility.Snap(pawn, snapCause);
        } else if (canSnap && Rand.Chance(1f / 16f)) {
            SnapUtility.Snap(pawn, snapCause);
            snapped = true;
        }

        List<MetallicArtsMetalDef> allDefs = DefDatabase<MetallicArtsMetalDef>.AllDefsListForReading;
        for (int i = 0; i < allDefs.Count; i++) {
            if (allDefs[i].feruchemy != null) {
                AddGene(pawn, GeneDefOf.GetFerringGeneForMetal(allDefs[i]), false, snapped);
            }
        }

        pawn.story.EnsureTrait(TraitDefOf.Cosmere_Scadrial_Trait_FullFeruchemist);
    }

    private static bool IsTerris(Pawn pawn) {
        return pawn.genes?.HasActiveGene(GeneDefOf.Cosmere_Scadrial_Gene_TerrisHeritage) == true;
    }

    private static bool IsNoble(Pawn pawn) {
        return pawn.genes?.HasActiveGene(GeneDefOf.Cosmere_Scadrial_Gene_NobleHeritage) == true;
    }

    private static bool IsSkaa(Pawn pawn) {
        return pawn.genes?.HasActiveGene(GeneDefOf.Cosmere_Scadrial_Gene_SkaaHeritage) == true;
    }

    private static bool IsMetalbornBlocked(Pawn pawn) {
        return !isHarmony && pawn.genes?.HasActiveGene(GeneDefOf.Cosmere_Scadrial_Gene_SkaaPurity) == true;
    }

    private static bool RollChance(int oneIn) {
        return RollChance(oneIn, out _);
    }

    private static bool RollChance(int oneIn, out string rollString) {
        int roll = Rand.RangeInclusive(1, oneIn);
        rollString = $"{roll}/{oneIn}";

        return roll == 1;
    }

    private static void AddHeritageGene(Pawn_GeneTracker genes, GeneDef def, float chance) {
        if (Rand.Chance(chance) && !genes.HasActiveGene(def)) {
            genes.AddGene(def, false);
        }
    }
}
