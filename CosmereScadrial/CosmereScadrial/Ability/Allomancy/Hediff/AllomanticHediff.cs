using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CosmereCore.Extension;
using CosmereFramework.Util;
using CosmereScadrial.Ability.Allomancy.Hediff.Comp;
using CosmereScadrial.Comp.Thing;
using CosmereScadrial.Def;
using CosmereScadrial.Util;
using Verse;

namespace CosmereScadrial.Ability.Allomancy.Hediff;

public class AllomanticHediff : HediffWithComps {
    public float extraSeverity = 0f;
    public MetallicArtsMetalDef metal;
    public List<AbstractAbility> sourceAbilities = [];

    public AllomanticHediff(HediffDef hediffDef, Pawn pawn, AbstractAbility ability) {
        def = hediffDef;
        this.pawn = pawn;
        metal = ability.metal;
    }

    public override string LabelBase =>
        base.LabelBase + (sourceAbilities.Count > 1 ? $" ({sourceAbilities.Count} sources)" : "");

    public SeverityCalculator severityCalculator => GetComp<SeverityCalculator>();

    public event Action<AllomanticHediff, AbstractAbility> OnSourceAdded;
    public event Action<AllomanticHediff, AbstractAbility> OnSourceRemoved;

    public override void PostRemoved() {
        base.PostRemoved();
        Log.Warning($"{pawn.NameFullColored} has lost {def.defName} hediff");
    }

    public void AddSource(AbstractAbility sourceAbility) {
        if (sourceAbilities.Contains(sourceAbility)) {
            return;
        }

        Log.Warning(
            $"{sourceAbility.pawn.NameFullColored} applying {def.defName} hediff to {pawn.NameFullColored} with Status={sourceAbility.status}");

        sourceAbilities.Add(sourceAbility);
        OnSourceAdded?.Invoke(this, sourceAbility);
    }

    public void RemoveSource(AbstractAbility sourceAbility) {
        if (!sourceAbilities.Contains(sourceAbility)) {
            return;
        }

        Log.Warning(
            $"{sourceAbility.pawn.NameFullColored} is no longer casting {sourceAbility.def.defName} on {pawn.NameFullColored}, Removing source for {def.defName} hediff");

        sourceAbilities.Remove(sourceAbility);
        OnSourceRemoved?.Invoke(this, sourceAbility);
    }

    public override void Tick() {
        SurgeChargeHediff? surge = AllomancyUtility.GetSurgeBurn(pawn);
        if (def.defName != surge?.def.defName) {
            surge?.Burn(
                severityCalculator.RecalculateSeverity,
                (int)TickUtility.TicksPerSecond,
                () => {
                    pawn.GetComp<MetalBurning>().TryBurnMetals();
                    severityCalculator.RecalculateSeverity();
                });
        }

        if (pawn.IsShieldedAgainstInvestiture() && !def.Equals(HediffDefOf.Cosmere_Hediff_Investiture_Shield)) {
            foreach (AbstractAbility abstractAbility in sourceAbilities.ToList()) {
                abstractAbility.UpdateStatus(BurningStatus.Off);
            }
        }

        base.Tick();
    }

    public override void ExposeData() {
        base.ExposeData();

        List<Pawn> sourcePawns = null!;

        if (Scribe.mode == LoadSaveMode.Saving) {
            sourcePawns = sourceAbilities.Select(a => a.pawn).Distinct().ToList();
        }

        Scribe_Collections.Look(ref sourcePawns, "sourcePawns", LookMode.Reference);

        if (Scribe.mode != LoadSaveMode.LoadingVars) return;
        sourceAbilities = [];

        if (sourcePawns == null) return;
        foreach (Pawn? localPawn in sourcePawns) {
            foreach (RimWorld.Ability? ability in localPawn?.abilities?.abilities ?? []) {
                if (ability is not AbstractAbility aa) continue;
                // If this pawn has any Allomantic ability, we re-attach it
                sourceAbilities.Add(aa);
                // Optional: maybe only add one matching ability type, if you can correlate them
                break;
            }
        }
    }

    public override string DebugString() {
        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.AppendLine(base.DebugString());

        stringBuilder.AppendLine("Severity Sources:");
        foreach (AbstractAbility? ability in sourceAbilities) {
            stringBuilder.AppendLine(
                $"  {ability.pawn.NameShortColored} -> {ability.def.LabelCap}: {ability.GetStrength():0.0000}");
        }

        return stringBuilder.ToString();
    }
}