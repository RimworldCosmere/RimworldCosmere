using Cosmere.Core.Framework;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.System.Roshar.Surgebinding.Ability.Progression;

public class Lifesurge : SurgebindingAbility {
    private const float LimbRegenCostMultiplier = 2f;
    private const float WoundHealCostFraction = 0.1f;
    private const float DiseaseCureCostMultiplier = 1.5f;
    private static ThingDef? _pulseDef;

    private static ThingDef? PulseMoteDef => _pulseDef ??= ThingDefOf.Cosmere_Roshar_Thing_LifesurgePulse;

    private static readonly int[] DurationSeconds = [10, 15, 20, 25, 30];
    private static readonly int[] MaxWoundsToHeal = [1, 3, 5, int.MaxValue, int.MaxValue];

    public Lifesurge(Pawn pawn) : base(pawn) { }

    public Lifesurge(Pawn pawn, AbilityDef def) : base(pawn, def) { }

    public override bool Activate(LocalTargetInfo target, LocalTargetInfo dest) {
        float cost = def.beuPerTick / (1 << Gene.CurrentIdeal);
        if (!Gene.CanLowerReserve(cost)) return false;

        Pawn? targetPawn = target.Pawn;
        if (targetPawn == null || targetPawn.Dead) return false;

        Gene.RemoveFromReserve(cost);
        int ideal = Gene.CurrentIdeal;

        CureBleeding(targetPawn);
        HealWounds(targetPawn, ideal);

        if (ideal >= 1) {
            RegenerateMissingParts(targetPawn, ideal);
        }

        if (ideal >= 2) {
            CureDiseases(targetPawn, ideal);
        }

        ApplyLifesurgeHediff(targetPawn, ideal);

        if (PulseMoteDef != null) {
            MoteMaker.MakeAttachedOverlay(targetPawn, PulseMoteDef, Vector3.zero, 4f);
        }

        return true;
    }

    private static void CureBleeding(Pawn targetPawn) {
        List<Verse.Hediff> hediffs = targetPawn.health.hediffSet.hediffs;
        for (int i = 0; i < hediffs.Count; i++) {
            if (hediffs[i].Bleeding) {
                hediffs[i].Tended(1f, 1f);
            }
        }
    }

    private void HealWounds(Pawn targetPawn, int ideal) {
        List<Hediff_Injury> injuries = [];
        List<Verse.Hediff> hediffs = targetPawn.health.hediffSet.hediffs;
        for (int i = 0; i < hediffs.Count; i++) {
            if (hediffs[i] is Hediff_Injury injury && injury.Severity > 0) {
                HediffComp_GetsPermanent? permComp = injury.TryGetComp<HediffComp_GetsPermanent>();
                if (permComp != null && permComp.IsPermanent && ideal < 4) continue;
                injuries.Add(injury);
            }
        }

        if (injuries.Count == 0) return;

        int maxWounds = ideal < MaxWoundsToHeal.Length ? MaxWoundsToHeal[ideal] : int.MaxValue;
        injuries.Sort((a, b) => a.Severity.CompareTo(b.Severity));

        if (ideal >= 4) {
            for (int i = 0; i < injuries.Count; i++) {
                float healCost = def.beuPerTick * WoundHealCostFraction;
                if (!Gene.CanLowerReserve(healCost)) break;
                Gene.RemoveFromReserve(healCost);
                injuries[i].Heal(injuries[i].Severity);
            }

            return;
        }

        if (ideal == 3) {
            for (int i = 0; i < injuries.Count; i++) {
                float partMaxHp = injuries[i].Part?.def.hitPoints ?? 30f;
                if (injuries[i].Severity > partMaxHp * 0.5f) continue;
                float healCost = def.beuPerTick * WoundHealCostFraction;
                if (!Gene.CanLowerReserve(healCost)) break;
                Gene.RemoveFromReserve(healCost);
                injuries[i].Heal(injuries[i].Severity);
            }

            return;
        }

        int healed = 0;
        for (int i = 0; i < injuries.Count && healed < maxWounds; i++) {
            float healCost = def.beuPerTick * WoundHealCostFraction;
            if (!Gene.CanLowerReserve(healCost)) break;
            Gene.RemoveFromReserve(healCost);
            injuries[i].Heal(injuries[i].Severity);
            healed++;
        }
    }

    private void RegenerateMissingParts(Pawn targetPawn, int ideal) {
        List<Hediff_MissingPart> missingParts = [];
        List<Verse.Hediff> hediffs = targetPawn.health.hediffSet.hediffs;
        for (int i = 0; i < hediffs.Count; i++) {
            if (hediffs[i] is Hediff_MissingPart missingPart) {
                missingParts.Add(missingPart);
            }
        }

        missingParts.Sort((a, b) => GetRegenTier(a.Part).CompareTo(GetRegenTier(b.Part)));

        for (int i = 0; i < missingParts.Count; i++) {
            int tier = GetRegenTier(missingParts[i].Part);
            if (ideal < tier) continue;

            float regenCost = def.beuPerTick * LimbRegenCostMultiplier;
            if (!Gene.CanLowerReserve(regenCost)) break;

            Gene.RemoveFromReserve(regenCost);
            targetPawn.health.RestorePart(missingParts[i].Part);
            FleckMaker.Static(targetPawn.DrawPos, targetPawn.Map, FleckDefOf.PsycastAreaEffect);
        }
    }

    private void CureDiseases(Pawn targetPawn, int ideal) {
        List<Verse.Hediff> toRemove = [];
        List<Verse.Hediff> hediffs = targetPawn.health.hediffSet.hediffs;

        for (int i = 0; i < hediffs.Count; i++) {
            Verse.Hediff hediff = hediffs[i];
            if (!hediff.def.isBad) continue;
            if (hediff is Hediff_MissingPart) continue;
            if (hediff is Hediff_Injury) continue;
            if (InvestitureHealExclusionRegistry.IsExcluded(hediff)) continue;

            if (ideal >= 2 &&
                RimWorld.HediffDefOf.WoundInfection != null &&
                hediff.def == RimWorld.HediffDefOf.WoundInfection) {
                toRemove.Add(hediff);
                continue;
            }

            if (ideal >= 3 && hediff.def.makesSickThought) {
                toRemove.Add(hediff);
                continue;
            }

            if (ideal >= 4) {
                toRemove.Add(hediff);
            }
        }

        for (int i = 0; i < toRemove.Count; i++) {
            float cureCost = def.beuPerTick * DiseaseCureCostMultiplier * Mathf.Max(toRemove[i].Severity, 0.1f);
            if (!Gene.CanLowerReserve(cureCost)) break;
            Gene.RemoveFromReserve(cureCost);
            targetPawn.health.RemoveHediff(toRemove[i]);
        }
    }

    private void ApplyLifesurgeHediff(Pawn targetPawn, int ideal) {
        HediffDef? hediffDef = def.hediff;
        if (hediffDef == null) return;

        HediffWithComps hediff = (HediffWithComps)HediffMaker.MakeHediff(hediffDef, targetPawn);
        hediff.Severity = ideal + 1f;

        HediffComp_Disappears? disappears = hediff.TryGetComp<HediffComp_Disappears>();
        if (disappears != null) {
            int durationIdx = Mathf.Clamp(ideal, 0, DurationSeconds.Length - 1);
            disappears.ticksToDisappear = GenTicks.TicksPerRealSecond * DurationSeconds[durationIdx];
        }

        targetPawn.health.AddHediff(hediff);
    }

    private static int GetRegenTier(BodyPartRecord part) {
        List<BodyPartTagDef> tags = part.def.tags;
        for (int i = 0; i < tags.Count; i++) {
            string tagName = tags[i].defName;
            if (tagName is "MovingLimbCore" or "ManipulationLimbCore") return 3;
            if (tagName is "MovingLimbSegment" or "ManipulationLimbSegment" or "SightSource") return 2;
            if (tagName is "MovingLimbDigit" or "ManipulationLimbDigit" or "HearingSource" or "BreathingSource")
                return 1;
        }

        if (part.depth == BodyPartDepth.Inside) return 4;
        return 1;
    }
}
