using System.Collections.Generic;
using Cosmere.Core.Comp.Thing;
using Cosmere.System.Scadrial.Extension;
using Cosmere.System.Scadrial.Gene;
using Cosmere.System.Scadrial.Hemalurgy.Comp.Thing;
using RimWorld;
using Verse;

namespace Cosmere.System.Scadrial.Hemalurgy;

public static class HemalurgicChargeUtility {
    public static void PerformCharge(
        Pawn donor, Pawn surgeon, HemalurgicSpike spikeComp,
        HemalurgicStealType stealType, GeneDef? selectedGene,
        float strengthMultiplier, bool applyInjury, bool isThinNeedle
    ) {
        HemalurgicChargeData chargeData = new HemalurgicChargeData {
            stealType = stealType,
            chargedTick = Find.TickManager.TicksGame,
            strength = strengthMultiplier,
        };

        bool genesWereStolen = false;

        switch (stealType) {
            case HemalurgicStealType.HumanStrength:
            case HemalurgicStealType.HumanSenses:
            case HemalurgicStealType.EmotionalFortitude:
            case HemalurgicStealType.MentalFortitude:
                chargeData.stolenDefName = stealType.ToString();
                break;

            case HemalurgicStealType.PhysicalAllomancy:
            case HemalurgicStealType.MentalAllomancy:
            case HemalurgicStealType.TemporalAllomancy:
            case HemalurgicStealType.EnhancementAllomancy:
            case HemalurgicStealType.PhysicalFeruchemy:
            case HemalurgicStealType.CognitiveFeruchemy:
            case HemalurgicStealType.HybridFeruchemy:
            case HemalurgicStealType.SpiritualFeruchemy:
            case HemalurgicStealType.AnyPower:
                if (selectedGene == null) return;
                chargeData.stolenDefName = selectedGene.defName;
                Verse.Gene? activeGene = donor.genes?.GetGene(selectedGene);
                if (activeGene != null) {
                    donor.genes?.RemoveGene(activeGene);
                    genesWereStolen = true;
                }
                break;

            case HemalurgicStealType.Investiture:
                chargeData.stolenDefName = "Investiture";
                break;

            case HemalurgicStealType.ConnectionIdentity:
                chargeData.stolenDefName = "Connection";
                ApplyConnectionSteal(donor);
                break;

            case HemalurgicStealType.RemoveAllPowers:
                RemoveAllPowers(donor);
                genesWereStolen = true;
                break;

            case HemalurgicStealType.AllAbilities:
                chargeData.stolenDefNames = StealAllAbilities(donor);
                genesWereStolen = true;
                break;
        }

        StealInvestiture(donor, chargeData);
        spikeComp.Charge(chargeData);

        if (applyInjury) {
            ApplyDonorInjury(donor, surgeon, isThinNeedle);
        }

        if (genesWereStolen && !donor.Dead) {
            CheckAndApplyDrab(donor);
        }

        Messages.Message(
            "CS_Hemalurgy_ChargeSuccess".Translate(
                surgeon.Named("SURGEON"),
                (spikeComp.metal?.Named("METAL") ?? "unknown".Named("METAL")),
                donor.Named("DONOR")
            ),
            donor, MessageTypeDefOf.PositiveEvent
        );

        TaleRecorder.RecordTale(TaleDefOf.DidSurgery, surgeon, donor);
    }

    public static void StealInvestiture(Pawn donor, HemalurgicChargeData chargeData) {
        InvestitureHolder? holder = donor.TryGetComp<InvestitureHolder>();
        if (holder == null) return;

        float stolen = holder.currentInvestitureSelf * HemalurgicConstants.InvestitureTheftFraction;
        if (stolen <= 0f) return;

        holder.currentInvestitureSelf -= stolen;
        chargeData.storedInvestiture = stolen;
    }

    public static void ApplyConnectionSteal(Pawn donor) {
        Roshar.Gene.Surgebinder? surgebinder = donor.genes?.GetFirstGeneOfType<Roshar.Gene.Surgebinder>();
        if (surgebinder != null) {
            surgebinder.currentIdeal = 0;
            ILoadReferenceable? bondTarget = surgebinder.GetBondTarget();
            if (bondTarget != null) {
                Core.Comp.Game.SpiritWeb.Instance?.SetConnection(donor, bondTarget, 0.1f);
                Verse.Hediff? strainedBond = donor.health.hediffSet.GetFirstHediffOfDef(
                    Roshar.HediffDefOf.Cosmere_Roshar_Hediff_StrainedBond
                );
                if (strainedBond == null) {
                    strainedBond = HediffMaker.MakeHediff(
                        Roshar.HediffDefOf.Cosmere_Roshar_Hediff_StrainedBond, donor
                    );
                    donor.health.AddHediff(strainedBond);
                }
            }
        }
    }

    public static void RemoveAllPowers(Pawn donor) {
        List<Allomancer> allomantic = donor.genes.GetAllomanticGenes();
        for (int i = allomantic.Count - 1; i >= 0; i--) {
            donor.genes.RemoveGene(allomantic[i]);
        }

        List<Feruchemist> feruchemic = donor.genes.GetFeruchemicGenes();
        for (int i = feruchemic.Count - 1; i >= 0; i--) {
            donor.genes.RemoveGene(feruchemic[i]);
        }

        InvestitureHolder? holder = donor.TryGetComp<InvestitureHolder>();
        if (holder != null) {
            holder.currentInvestitureSelf = 0f;
        }

        RemoveInvestedTrait(donor);
    }

    public static List<string> StealAllAbilities(Pawn donor) {
        List<string> stolenDefNames = [];

        List<Allomancer> allomantic = donor.genes.GetAllomanticGenes();
        for (int i = allomantic.Count - 1; i >= 0; i--) {
            stolenDefNames.Add(allomantic[i].def.defName);
            donor.genes.RemoveGene(allomantic[i]);
        }

        List<Feruchemist> feruchemic = donor.genes.GetFeruchemicGenes();
        for (int i = feruchemic.Count - 1; i >= 0; i--) {
            stolenDefNames.Add(feruchemic[i].def.defName);
            donor.genes.RemoveGene(feruchemic[i]);
        }

        InvestitureHolder? holder = donor.TryGetComp<InvestitureHolder>();
        if (holder != null) {
            holder.currentInvestitureSelf = 0f;
        }

        RemoveInvestedTrait(donor);

        return stolenDefNames;
    }

    public static void CheckAndApplyDrab(Pawn donor) {
        if (donor.Dead) return;

        List<Allomancer> allomantic = donor.genes.GetAllomanticGenes();
        if (allomantic.Count > 0) return;

        List<Feruchemist> feruchemic = donor.genes.GetFeruchemicGenes();
        if (feruchemic.Count > 0) return;

        if (!donor.health.hediffSet.HasHediff(HemalurgicDefOf.Cosmere_Scadrial_Hediff_Drab)) {
            Verse.Hediff drab = HediffMaker.MakeHediff(HemalurgicDefOf.Cosmere_Scadrial_Hediff_Drab, donor);
            donor.health.AddHediff(drab);
        }

        if (donor.story?.traits != null && !donor.story.traits.HasTrait(Scadrial.TraitDefOf.Cosmere_Scadrial_Trait_Drab)) {
            donor.story.traits.GainTrait(new Trait(Scadrial.TraitDefOf.Cosmere_Scadrial_Trait_Drab));
        }
    }

    private static void RemoveInvestedTrait(Pawn donor) {
        if (donor.story?.traits == null) return;
        Trait? invested = donor.story.traits.GetTrait(Core.TraitDefOf.Cosmere_Invested);
        if (invested != null) {
            donor.story.traits.RemoveTrait(invested);
        }
    }

    private static void ApplyDonorInjury(Pawn donor, Pawn surgeon, bool isThinNeedle) {
        if (!isThinNeedle) {
            donor.Kill(new DamageInfo(DamageDefOf.SurgicalCut, 9999f, 999f, -1f, surgeon));
            Messages.Message(
                "CS_Hemalurgy_DonorDied".Translate(donor.Named("DONOR")),
                donor, MessageTypeDefOf.PawnDeath
            );
            return;
        }

        bool hasMastery = HemalurgicDefOf.Cosmere_Scadrial_HemalurgicMastery?.IsFinished ?? false;
        bool hasPrecision = HemalurgicDefOf.Cosmere_Scadrial_HemalurgicPrecision?.IsFinished ?? false;
        float baseDeathChance = hasMastery ? 0.10f : hasPrecision ? 0.40f : 0.90f;
        int medicalSkill = surgeon?.skills?.GetSkill(RimWorld.SkillDefOf.Medicine)?.Level ?? 0;
        float deathChance = baseDeathChance * (1f - medicalSkill * 0.02f);
        deathChance = UnityEngine.Mathf.Clamp01(deathChance);

        if (Rand.Chance(deathChance)) {
            donor.Kill(new DamageInfo(DamageDefOf.SurgicalCut, 9999f, 999f, -1f, surgeon));
            Messages.Message(
                "CS_Hemalurgy_DonorDied".Translate(donor.Named("DONOR")),
                donor, MessageTypeDefOf.PawnDeath
            );
            return;
        }

        float injurySeverity = 20f;
        if (medicalSkill >= 10) injurySeverity = 8f;
        else if (medicalSkill >= 7) injurySeverity = 12f;

        BodyPartRecord? torso = donor.health.hediffSet.GetNotMissingParts()
            .FirstOrDefault(p => p.def == BodyPartDefOf.Torso);
        if (torso != null) {
            DamageInfo damageInfo = new DamageInfo(DamageDefOf.SurgicalCut, injurySeverity, 0f, -1f, surgeon, torso);
            donor.TakeDamage(damageInfo);
        }

        Verse.Hediff? bloodLoss = donor.health.hediffSet.GetFirstHediffOfDef(RimWorld.HediffDefOf.BloodLoss);
        if (bloodLoss != null) {
            bloodLoss.Severity += 0.3f;
        } else {
            Verse.Hediff newBloodLoss = HediffMaker.MakeHediff(RimWorld.HediffDefOf.BloodLoss, donor);
            newBloodLoss.Severity = 0.3f;
            donor.health.AddHediff(newBloodLoss);
        }
    }
}
