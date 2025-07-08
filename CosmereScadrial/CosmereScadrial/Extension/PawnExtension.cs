using System.Collections.Generic;
using System.Linq;
using CosmereCore.Util;
using CosmereFramework.Extension;
using CosmereResources;
using CosmereResources.Def;
using CosmereScadrial.Allomancy.Ability;
using CosmereScadrial.Def;
using CosmereScadrial.Gene;
using CosmereScadrial.Thing;
using RimWorld;
using Verse;
using Verse.AI;

namespace CosmereScadrial.Extension;

public static class PawnExtension {
    public static AcceptanceReport CanUseMetal(this Pawn pawn, MetalDef metal) {
        if (metal.Equals(MetalDefOf.Lerasium) && pawn.IsMistborn()) {
            return new AcceptanceReport("CS_AlreadyMistborn".Translate(pawn.Named("PAWN")));
        }

        if (metal.godMetal || pawn.IsMistborn() || pawn.IsMisting(metal)) {
            return true;
        }

        return new AcceptanceReport("CS_CannotUseMetal".Translate(pawn.Named("PAWN"), metal.Named("METAL")));
    }

    public static bool IsMistborn(this Pawn pawn) {
        Need_Food? need = pawn.needs.food;

        return pawn.story.traits.HasTrait(TraitDefOf.Cosmere_Scadrial_Trait_Mistborn);
    }

    public static bool IsFullFeruchemist(this Pawn pawn) {
        return pawn.story.traits.HasTrait(TraitDefOf.Cosmere_Scadrial_Trait_FullFeruchemist);
    }

    public static bool IsAllomancer(this Pawn pawn) {
        return pawn.story.traits.HasTrait(TraitDefOf.Cosmere_Scadrial_Trait_Allomancer);
    }

    public static bool IsFeruchemist(this Pawn pawn) {
        return pawn.story.traits.HasTrait(TraitDefOf.Cosmere_Scadrial_Trait_Feruchemist);
    }

    public static bool IsMisting(this Pawn pawn, MetalDef metal) {
        return pawn.genes.HasAllomanticGeneForMetal(metal);
    }

    public static bool IsFerring(this Pawn pawn, MetalDef metal) {
        return pawn.genes.HasFeruchemicGeneForMetal(metal);
    }

    public static bool IsBurning(this Pawn pawn, MetalDef metal) {
        Allomancer? gene = pawn.genes.GetAllomanticGeneForMetal(metal);

        return gene?.Burning ?? false;
    }

    public static bool IsCompounding(this Pawn pawn, MetalDef metal) {
        return pawn.health.hediffSet.HasHediff(
            DefDatabase<HediffDef>.GetNamed("Cosmere_Scadrial_Hediff_Compound" + metal.defName, false)
        );
    }

    public static void TryConsumeVial(this Pawn pawn, MetalDef metal) {
        if (pawn.CurJobDef?.defName == RimWorld.JobDefOf.Ingest.defName &&
            pawn.CurJob.targetA.Thing is AllomanticVial) {
            return;
        }

        if (metal.IsOneOf(MetalDefOf.Duralumin, MetalDefOf.Nicrosil)) return;
        if (pawn.IsAsleep()) return;
        if (InvestitureDetector.IsShielded(pawn)) return;

        AllomanticVial? vial = pawn.GetVial(metal);
        if (vial == null || vial.stackCount == 0) return;

        Job job = JobMaker.MakeJob(RimWorld.JobDefOf.Ingest, vial);
        job.count = 1;
        job.ingestTotalCount = true;
        pawn.jobs.InterruptJobWith(job, JobTag.SatisfyingNeeds);
    }

    public static void WipeAllAllomanticReserves(this Pawn pawn) {
        foreach (Allomancer gene in pawn.genes.GetAllomanticGenes()) {
            gene.WipeReserve();
        }
    }

    public static void WipeAllomanticReserves(this Pawn pawn, MetalDef metal) {
        Allomancer? gene = pawn.genes.GetAllomanticGeneForMetal(metal);
        gene?.WipeReserve();
    }

    public static void FillAllAllomanticReserves(this Pawn pawn) {
        foreach (Allomancer gene in pawn.genes.GetAllomanticGenes()) {
            gene.FillReserve();
        }
    }

    public static void FillAllomanticReserves(this Pawn pawn, MetalDef metal) {
        Allomancer? gene = pawn.genes.GetAllomanticGeneForMetal(metal);
        gene?.FillReserve();
    }

    public static float GetAllomanticReservePercent(this Pawn pawn, MetalDef metal) {
        Allomancer? gene = pawn.genes.GetAllomanticGeneForMetal(metal);
        return gene?.GetReservePercent() ?? 0f;
    }

    public static AbstractAbility? GetAllomanticAbility(this Pawn pawn, AllomanticAbilityDef def) {
        return (AbstractAbility)pawn.abilities.GetAbility(def);
    }

    public static List<MetallicArtsMetalDef> GetAllBurningMetals(this Pawn pawn) {
        return pawn.genes.GetAllomanticGenes().Where(x => x.Burning).Select(x => x.metal).ToList();
    }
}