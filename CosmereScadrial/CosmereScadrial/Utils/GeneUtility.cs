using System.Collections.Generic;
using System.Linq;
using CosmereCore.Utils;
using CosmereFramework.Extensions;
using CosmereScadrial.Defs;
using CosmereScadrial.Genes;
using RimWorld;
using UnityEngine;
using Verse;
using Log = CosmereFramework.Log;

namespace CosmereScadrial.Utils;

public static class GeneUtility {
    private static bool isHarmony => ShardUtility.AreAnyEnabled(ShardDefOf.Harmony);

    private static bool isRuin => ShardUtility.AreAnyEnabled(ShardDefOf.Ruin);

    private static bool isPreservation => ShardUtility.AreAnyEnabled(ShardDefOf.Preservation);

    public static void TryAssignScadrialGenes(Pawn pawn) {
        if (pawn.genes == null || !pawn.RaceProps.Humanlike) {
            return;
        }

        Pawn_GeneTracker genes = pawn.genes;
        if (genes.Xenotype?.Equals(XenotypeDefOf.Cosmere_Scadrial_Xenotype_Skaa) ?? false) {
            TryAddHeritageGene(genes, GeneDefOf.Cosmere_Scadrial_Gene_TerrisHeritage, 0.10f);
            TryAddHeritageGene(genes, GeneDefOf.Cosmere_Scadrial_Gene_NobleHeritage, 0.10f);
        }

        if (genes.Xenotype?.Equals(XenotypeDefOf.Cosmere_Scadrial_Xenotype_Noble) ?? false) {
            TryAddHeritageGene(genes, GeneDefOf.Cosmere_Scadrial_Gene_TerrisHeritage, 0.10f);
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
                Log.Verbose(
                    $"Trying for Mistborn. Pawn={pawn.NameFullColored} Success={Log.ColoredBoolean(success ? Color.green : Color.red, success)} Roll={roll}");
                if (success) {
                    AddMistborn(pawn);
                } else {
                    success = RollChance(16, out roll);
                    Log.Verbose(
                        $"Trying for Misting. Pawn={pawn.NameFullColored} Success={Log.ColoredBoolean(success ? Color.green : Color.red, success)} Roll={roll}");
                    if (success) TryAddRandomAllomanticGene(pawn);
                }
            }

            // Ruin logic
            if (!isRuin) return;
            if (!isTerris) return;

            // Full Feruchemists were way more common, from what I can tell
            // Most Terris were Full, or nothing. There was a small chance for Ferrings, but it was rare.
            success = RollChance(16, out roll);
            Log.Verbose(
                $"Trying for full feruchemist. Pawn={pawn.NameFullColored} Success={Log.ColoredBoolean(success ? Color.green : Color.red, success)} Roll={roll}");
            if (success) {
                AddFullFeruchemist(pawn);
            } else {
                success = RollChance(64, out roll);
                Log.Verbose(
                    $"Trying for Ferring. Pawn={pawn.NameFullColored} Success={Log.ColoredBoolean(success ? Color.green : Color.red, success)} Roll={roll}");
                if (success) TryAddRandomFeruchemicalGene(pawn);
            }

            return;
        }

        // Harmony system: no nobles, no block, no mistborn/full feruchemists
        if (!isHarmony) return;

        success = RollChance(16, out roll);
        Log.Verbose(
            $"Trying for random misting. Pawn={pawn.NameFullColored} Success={Log.ColoredBoolean(success ? Color.green : Color.red, success)} Roll={roll}");
        if (RollChance(16)) {
            TryAddRandomAllomanticGene(pawn);
        }

        if (!isTerris) return;

        success = RollChance(16, out roll);
        Log.Verbose(
            $"Trying for random ferring. Pawn={pawn.NameFullColored} Success={Log.ColoredBoolean(success ? Color.green : Color.red, success)} Roll={roll}");
        if (success) TryAddRandomFeruchemicalGene(pawn);
    }

    public static void HandleSkaaPurityGenes(Pawn generated, Pawn other, PawnGenerationRequest request) {
        // Only applies to humanlike pawns
        if (!generated.RaceProps.Humanlike || generated.genes == null) return;

        // Identify both parents
        Pawn? parent1 = request.FixedLastName != null ? generated.GetFather() : other;
        Pawn? parent2 = request.FixedLastName != null ? other : generated.GetMother();

        // Null safety
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
        Log.Verbose(
            $"Removed {skaaPurity.defName} from {generated.NameFullColored} (only one parent had it)");
    }

    public static void TryAddRandomAllomanticGene(Pawn pawn, bool canSnap = true) {
        MetallicArtsMetalDef? metal = DefDatabase<MetallicArtsMetalDef>.AllDefsListForReading
            .Where(x => x.allomancy != null)
            .RandomElement();
        AddGene(pawn, GeneDefOf.GetMistingGeneForMetal(metal), canSnap);
    }

    public static void TryAddRandomFeruchemicalGene(Pawn pawn, bool canSnap = true) {
        MetallicArtsMetalDef? metal = DefDatabase<MetallicArtsMetalDef>.AllDefsListForReading
            .Where(x => x.feruchemy != null)
            .RandomElement();
        AddGene(pawn, GeneDefOf.GetFerringGeneForMetal(metal), canSnap);
    }

    public static void AddGene(Pawn pawn, GeneDef gene, bool canSnap = true, bool snapped = false) {
        if (pawn.genes.HasActiveGene(gene)) return;

        if (snapped) {
            SnapUtility.TrySnap(pawn);
        } else if (canSnap && Rand.Chance(1f / 16f)) {
            SnapUtility.TrySnap(pawn);
            snapped = true;
        }

        if (snapped) {
            pawn.genes.TryAddGene(gene);
        } else {
            GeneDef? geneWrapperDef = GeneDefOf.Cosmere_Scadrial_Gene_DormantMetalborn;
            if (pawn.genes.GetGene(geneWrapperDef) is not DormantMetalborn geneWrapper) {
                geneWrapper = (DormantMetalborn)pawn.genes.TryAddGene(geneWrapperDef);
            }

            geneWrapper.genesToAdd.Add(gene);
        }
    }

    public static void AddMistborn(Pawn pawn, bool canSnap = true, bool snapped = false) {
        if (snapped) {
            SnapUtility.TrySnap(pawn);
        } else if (canSnap && Rand.Chance(1f / 16f)) {
            SnapUtility.TrySnap(pawn);
            snapped = true;
        }

        IEnumerable<MetallicArtsMetalDef> metals =
            DefDatabase<MetallicArtsMetalDef>.AllDefsListForReading.Where(x => x.allomancy != null);
        foreach (MetallicArtsMetalDef? metal in metals) {
            AddGene(pawn, GeneDefOf.GetMistingGeneForMetal(metal), false, snapped);
        }

        pawn.story.TryAddTrait(TraitDefOf.Cosmere_Scadrial_Trait_Mistborn);
    }

    public static void AddFullFeruchemist(Pawn pawn, bool canSnap = true, bool snapped = false) {
        if (snapped) {
            SnapUtility.TrySnap(pawn);
        } else if (canSnap && Rand.Chance(1f / 16f)) {
            SnapUtility.TrySnap(pawn);
            snapped = true;
        }

        IEnumerable<MetallicArtsMetalDef> metals =
            DefDatabase<MetallicArtsMetalDef>.AllDefsListForReading.Where(x =>
                !x.godMetal && x.allomancy != null);
        foreach (MetallicArtsMetalDef? metal in metals) {
            AddGene(pawn, GeneDefOf.GetFerringGeneForMetal(metal), false, snapped);
        }

        pawn.story.TryAddTrait(TraitDefOf.Cosmere_Scadrial_Trait_FullFeruchemist);
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

    private static void TryAddHeritageGene(Pawn_GeneTracker genes, GeneDef def, float chance) {
        if (Rand.Chance(chance) && !genes.HasActiveGene(def)) {
            genes.AddGene(def, false);
        }
    }
}