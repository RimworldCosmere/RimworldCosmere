using Cosmere.Core;
using Cosmere.Core.Def;
using Cosmere.Core.Hediff;
using Cosmere.Core.Savant;
using Cosmere.Core.ShardConnection;
using Cosmere.System.Scadrial.Allomancy.Ability;
using Cosmere.System.Scadrial.Allomancy.Hediff;
using Cosmere.System.Scadrial.Def;
using Cosmere.System.Scadrial.Gene;
using Cosmere.System.Scadrial.Savant;
using Cosmere.System.Scadrial.Thing;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using AbilityDef = RimWorld.AbilityDef;
using GeneUtility = Cosmere.System.Scadrial.Util.GeneUtility;
using HediffUtility = Cosmere.System.Scadrial.Util.HediffUtility;

namespace Cosmere.System.Scadrial.Extension;

public static class PawnExtension {
    public static AcceptanceReport CanUseMetal(this Pawn pawn, MetalDef metal) {
        if (metal.Equals(MetalDefOf.Lerasium) && pawn.IsMistborn()) {
            return new AcceptanceReport("CS_AlreadyMistborn".Translate(pawn.Named("PAWN")));
        }

        // Lerasium is the deliberate exception. Burning it is how someone with no Connection at
        // all gains one, so gating it would deny it to exactly the people it exists for.
        if (metal.godMetal && !metal.Equals(MetalDefOf.Lerasium)) {
            if (!ConnectionUtility.MayUse(pawn, metal.shard)) {
                return new AcceptanceReport(
                    "CS_NotConnectedToShard".Translate(
                        pawn.Named("PAWN"),
                        metal.Named("METAL"),
                        (metal.shard?.label ?? metal.label).Named("SHARD")
                    )
                );
            }

            return true;
        }

        if (metal.godMetal || pawn.IsMistborn() || pawn.IsMisting(metal)) {
            return true;
        }

        return new AcceptanceReport("CS_CannotUseMetal".Translate(pawn.Named("PAWN"), metal.Named("METAL")));
    }

    public static bool IsMistborn(this Pawn pawn) {
        return pawn.story?.traits?.HasTrait(TraitDefOf.Cosmere_Scadrial_Trait_Mistborn) == true;
    }

    public static bool IsFullFeruchemist(this Pawn pawn) {
        return pawn.story?.traits?.HasTrait(TraitDefOf.Cosmere_Scadrial_Trait_FullFeruchemist) == true;
    }

    public static bool IsAllomancer(this Pawn pawn) {
        return pawn.story?.traits?.HasTrait(TraitDefOf.Cosmere_Scadrial_Trait_Allomancer) == true;
    }

    public static bool IsFeruchemist(this Pawn pawn) {
        return pawn.story?.traits?.HasTrait(TraitDefOf.Cosmere_Scadrial_Trait_Feruchemist) == true;
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

    public static bool IsSnapped(this Pawn pawn) {
        return pawn.needs?.mood?.thoughts?.memories?.GetFirstMemoryOfDef(ThoughtDefOf.Cosmere_Scadrial_Snapped) != null;
    }

    public static void TryConsumeVialIfNeeded(this Pawn pawn, MetalDef metal) {
        Allomancer? gene = pawn.genes.GetAllomanticGeneForMetal(metal);
        if (gene == null || !pawn.HasVial(metal) || !gene.ShouldConsumeVialNow) return;

        AllomanticVial? vial = pawn.GetVial(gene);
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

    public static void SetAllAllomanticReserves(this Pawn pawn, float amount) {
        foreach (Allomancer gene in pawn.genes.GetAllomanticGenes()) {
            gene.SetReserve(amount);
        }
    }

    public static void FillAllomanticReserves(this Pawn pawn, MetalDef metal) {
        Allomancer? gene = pawn.genes.GetAllomanticGeneForMetal(metal);
        gene?.FillReserve();
    }

    public static float GetAllomanticReservePercent(this Pawn pawn, MetalDef metal) {
        Allomancer? gene = pawn.genes.GetAllomanticGeneForMetal(metal);
        return gene?.ValuePercent ?? 0f;
    }

    public static AllomancyAbility? GetAllomanticAbility(this Pawn pawn, AbilityDef def) {
        if (def is not AllomanticAbilityDef allomanticDef) return null;
        return pawn.GetAllomanticAbility(allomanticDef);
    }

    public static AllomancyAbility? GetAllomanticAbility(this Pawn pawn, AllomanticAbilityDef def) {
        return pawn.abilities.GetAbility(def) as AllomancyAbility;
    }

    public static List<MetallicArtsMetalDef> GetAllBurningMetals(this Pawn pawn) {
        List<Allomancer> genes = pawn.genes.GetAllomanticGenes();
        List<MetallicArtsMetalDef> result = [];
        for (int i = 0; i < genes.Count; i++) {
            if (genes[i].Burning) result.Add(genes[i].metal);
        }

        return result;
    }

    public static float GetRawAllomanticPower(this Pawn pawn, MetalDef? metal = null) {
        float power = Mathf.Clamp01(pawn.GetStatValue(StatDefOf.Cosmere_Scadrial_Stat_AllomanticPower));
        float skill = Mathf.Clamp(
            pawn.skills.GetSkill(SkillDefOf.Cosmere_Scadrial_Skill_AllomanticPower).Level,
            0.1f,
            20
        );

        // Normalize against baseline: 0.25 power, 4 skill → target ~1 severity
        float normalizedPower = power * 2.5f;
        float normalizedSkill = skill * 0.10f;

        // sqrt curve with diminishing returns
        float raw = Mathf.Sqrt(normalizedPower * normalizedSkill);

        // Scale it to hit your desired range
        float scaled = raw * 1.5f; // tweak this if severity is still too high or low

        float result = Mathf.Clamp(scaled, 0.1f, 5f);

        if (metal != null) {
            int savantStage = ScadrialSavantUtility.GetAllomanticSavantStage(pawn, metal);
            result *= SavantUtility.GetAllomanticPowerMultiplier(savantStage);
        }

        return result;
    }

    public static AllomanticHediff? GetOrAddHediff(
        this Pawn target,
        AllomancyAbility ability,
        HediffDef? hediffDef,
        Pawn? caster = null
    ) {
        return HediffUtility.GetOrAddHediff(caster ?? ability.pawn, target, ability, hediffDef);
    }

    public static AllomanticHediff? GetOrAddHediff(
        this Pawn target,
        AllomancyAbility ability,
        IMultiTypeHediff def,
        Pawn? caster = null
    ) {
        return HediffUtility.GetOrAddHediff(caster ?? ability.pawn, target, ability, def);
    }

    public static void RemoveHediff(this Pawn target, AllomancyAbility ability, HediffDef? hediffDef) {
        HediffUtility.RemoveHediff(ability.pawn, target, ability, hediffDef);
    }

    public static void BecomeMistborn(
        this Pawn pawn,
        bool canSnap = false,
        bool snapped = true,
        string? cause = null
    ) {
        GeneUtility.AddMistborn(pawn, canSnap, snapped, cause);
        pawn.SetAllAllomanticReserves(float.PositiveInfinity);
    }

    public static void BecomeFullFeruchemist(
        this Pawn pawn,
        bool canSnap = false,
        bool snapped = true,
        string? cause = null
    ) {
        GeneUtility.AddFullFeruchemist(pawn, canSnap, snapped, cause);
    }
}
