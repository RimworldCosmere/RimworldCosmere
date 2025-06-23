using System.Collections.Generic;
using System.Linq;
using CosmereCore.Utils;
using CosmereScadrial.Defs;
using CosmereScadrial.Genes;
using RimWorld;
using UnityEngine;
using Verse;
using Log = CosmereFramework.Log;
using PawnUtility = CosmereFramework.Utils.PawnUtility;

namespace CosmereScadrial.Utils;

public static class GeneUtility {
    private static bool isHarmony => ShardUtility.AreAnyEnabled(ShardDefOf.Harmony);

    private static bool isRuin => ShardUtility.AreAnyEnabled(ShardDefOf.Ruin);

    private static bool isPreservation => ShardUtility.AreAnyEnabled(ShardDefOf.Preservation);

    public static void TryAssignScadrialGenes(Pawn pawn) {
        if (pawn?.genes == null || !pawn.RaceProps.Humanlike) {
            return;
        }

        Pawn_GeneTracker? genes = pawn.genes;
        string? xenotype = genes?.Xenotype?.defName;

        switch (xenotype) {
            case "Cosmere_Skaa":
                TryAddHeritageGene(genes, "Cosmere_Terris_Heritage", 0.10f);
                TryAddHeritageGene(genes, "Cosmere_Noble_Heritage", 0.10f);
                break;
            case "Cosmere_ScadrialNoble":
                TryAddHeritageGene(genes, "Cosmere_Terris_Heritage", 0.10f);
                break;
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

        bool parent1Skaa = parent1.genes?.HasActiveGene(DefDatabase<GeneDef>.GetNamed("Cosmere_Skaa_Purity")) ==
                           true;
        bool parent2Skaa = parent2.genes?.HasActiveGene(DefDatabase<GeneDef>.GetNamed("Cosmere_Skaa_Purity")) ==
                           true;

        bool childSkaa = generated.genes.HasActiveGene(DefDatabase<GeneDef>.GetNamed("Cosmere_Skaa_Purity"));

        // Remove if child inherited it but only one parent had it
        if (!childSkaa || parent1Skaa && parent2Skaa) return;
        GeneDef? geneDef = DefDatabase<GeneDef>.GetNamed("Cosmere_Skaa_Purity");
        Gene? gene = generated.genes.GetGene(geneDef);
        generated.genes.RemoveGene(gene);
        Log.Verbose($"Removed Cosmere_Skaa_Purity from {generated.NameFullColored} (only one parent had it)");
    }

    public static void TryAddRandomAllomanticGene(Pawn pawn, bool canSnap = true) {
        MetallicArtsMetalDef? metal = DefDatabase<MetallicArtsMetalDef>.AllDefsListForReading
            .Where(x => x.allomancy != null)
            .RandomElement();
        AddGene(pawn, $"Cosmere_Misting_{metal.defName}", canSnap);
    }

    public static void TryAddRandomFeruchemicalGene(Pawn pawn, bool canSnap = true) {
        MetallicArtsMetalDef? metal = DefDatabase<MetallicArtsMetalDef>.AllDefsListForReading
            .Where(x => x.feruchemy != null)
            .RandomElement();
        AddGene(pawn, $"Cosmere_Ferring_{metal.defName}", canSnap);
    }

    public static void AddGene(Pawn pawn, string defName, bool canSnap, bool snapped = false) {
        GeneDef? gene = DefDatabase<GeneDef>.GetNamedSilentFail(defName);
        if (gene == null || pawn.genes.HasActiveGene(gene)) return;

        if (snapped) {
            SnapUtility.TrySnap(pawn);
        } else if (canSnap && Rand.Chance(1f / 16f)) {
            SnapUtility.TrySnap(pawn);
            snapped = true;
        }

        if (snapped) {
            PawnUtility.TryAddGene(pawn, gene);
        } else {
            GeneDef? geneWrapperDef = GeneDefOf.Cosmere_Scadrial_Gene_DormantMetalborn;
            if (pawn.genes.GetGene(geneWrapperDef) is not DormantMetalborn geneWrapper) {
                geneWrapper = (DormantMetalborn)PawnUtility.TryAddGene(pawn, geneWrapperDef);
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
            AddGene(pawn, $"Cosmere_Misting_{metal.LabelCap}", false, snapped);
        }

        PawnUtility.TryAddTrait(pawn, TraitDefOf.Cosmere_Mistborn);
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
            AddGene(pawn, $"Cosmere_Ferring_{metal.LabelCap}", false, snapped);
        }

        PawnUtility.TryAddTrait(pawn, TraitDefOf.Cosmere_FullFeruchemist);
    }

    private static bool IsTerris(Pawn pawn) {
        return pawn.genes?.HasActiveGene(DefDatabase<GeneDef>.GetNamed("Cosmere_Terris_Heritage")) ==
               true;
    }

    private static bool IsNoble(Pawn pawn) {
        return pawn.genes?.HasActiveGene(DefDatabase<GeneDef>.GetNamed("Cosmere_Noble_Heritage")) == true;
    }

    private static bool IsSkaa(Pawn pawn) {
        return pawn.genes?.HasActiveGene(DefDatabase<GeneDef>.GetNamed("Cosmere_Skaa_Heritage")) == true;
    }

    private static bool IsMetalbornBlocked(Pawn pawn) {
        return !isHarmony &&
               pawn.genes?.HasActiveGene(DefDatabase<GeneDef>.GetNamed("Cosmere_Skaa_Purity")) == true;
    }

    private static bool RollChance(int oneIn) {
        return RollChance(oneIn, out _);
    }

    private static bool RollChance(int oneIn, out string rollString) {
        int roll = Rand.RangeInclusive(1, oneIn);
        rollString = $"{roll}/{oneIn}";

        return roll == 1;
    }

    private static void TryAddHeritageGene(Pawn_GeneTracker genes, string defName, float chance) {
        if (Rand.Chance(chance) && !genes.HasActiveGene(DefDatabase<GeneDef>.GetNamed(defName))) {
            genes.AddGene(DefDatabase<GeneDef>.GetNamed(defName), false);
        }
    }
}