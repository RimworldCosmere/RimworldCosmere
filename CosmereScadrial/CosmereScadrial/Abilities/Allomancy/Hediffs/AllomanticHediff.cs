using System.Collections.Generic;
using System.Linq;
using System.Text;
using CosmereFramework.Utils;
using CosmereScadrial.Defs;
using CosmereScadrial.Utils;
using RimWorld;
using Verse;

namespace CosmereScadrial.Abilities.Allomancy.Hediffs;

public class AllomanticHediff : HediffWithComps {
    public float extraSeverity = 0f;
    public MetallicArtsMetalDef metal;
    public List<AbstractAbility> sourceAbilities = [];

    public AllomanticHediff(HediffDef hediffDef, Pawn pawn, AbstractAbility ability) {
        def = hediffDef;
        this.pawn = pawn;
        AddSource(ability);
        metal = ability.metal;
    }

    public override string LabelBase =>
        base.LabelBase + (sourceAbilities.Count > 1 ? $" ({sourceAbilities.Count} sources)" : "");

    public void AddSource(AbstractAbility sourceAbility) {
        if (sourceAbilities.Contains(sourceAbility)) {
            RecalculateSeverity();
            return;
        }

        Log.Warning(
            $"{sourceAbility.pawn.NameFullColored} applying {def.defName} hediff to {pawn.NameFullColored} with Status={sourceAbility.status}");
        sourceAbility.OnStatusChanged += (_, _, _) => RecalculateSeverity();
        sourceAbilities.Add(sourceAbility);
        RecalculateSeverity();
    }

    public void RemoveSource(AbstractAbility sourceAbility, bool calculateSeverity = false) {
        if (!sourceAbilities.Contains(sourceAbility)) {
            if (calculateSeverity) RecalculateSeverity();
            return;
        }

        Log.Warning(
            $"{sourceAbility.pawn.NameFullColored} removing {def.defName} hediff from {pawn.NameFullColored}");
        sourceAbilities.Remove(sourceAbility);
        if (calculateSeverity) RecalculateSeverity();
    }

    public override void Tick() {
        AbstractAbility[] toRemove = sourceAbilities
            .Where(x => x.status == BurningStatus.Off || x.pawn.DestroyedOrNull())
            .ToArray();
        foreach (AbstractAbility ability in toRemove) {
            RemoveSource(ability);
        }

        SurgeChargeHediff? surge = AllomancyUtility.GetSurgeBurn(pawn);
        if (surge != null && def.defName != surge.def.defName) {
            surge?.Burn(
                RecalculateSeverity,
                (int)TickUtility.TicksPerSecond,
                RecalculateSeverity
            );
        }

        base.Tick();
    }

    protected virtual void RecalculateSeverity() {
        Severity = CalculateSeverity();
    }

    public float CalculateSeverity() {
        return sourceAbilities.Sum(x => x.GetStrength()) + extraSeverity;
    }

    public override void ExposeData() {
        base.ExposeData();

        List<Pawn> sourcePawns = null;

        if (Scribe.mode == LoadSaveMode.Saving) {
            sourcePawns = sourceAbilities.Select(a => a.pawn).Distinct().ToList();
        }

        Scribe_Collections.Look(ref sourcePawns, "sourcePawns", LookMode.Reference);

        if (Scribe.mode != LoadSaveMode.LoadingVars) return;
        sourceAbilities = [];

        if (sourcePawns == null) return;
        foreach (Pawn? localPawn in sourcePawns) {
            foreach (Ability? ability in localPawn?.abilities?.abilities ?? []) {
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