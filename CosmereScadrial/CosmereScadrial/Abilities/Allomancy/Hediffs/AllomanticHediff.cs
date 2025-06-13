using System.Collections.Generic;
using System.Linq;
using System.Text;
using CosmereFramework.Utils;
using CosmereScadrial.Utils;
using Verse;

namespace CosmereScadrial.Abilities.Allomancy.Hediffs {
    public class AllomanticHediff : HediffWithComps {
        public List<AbstractAbility> sourceAbilities = [];

        public override string LabelBase =>
            base.LabelBase + (sourceAbilities.Count > 1 ? $" ({sourceAbilities.Count} sources)" : "");

        public void AddSource(AbstractAbility sourceAbility) {
            if (sourceAbilities.Contains(sourceAbility)) return;

            Log.Warning(
                $"{sourceAbility.pawn.NameFullColored} applying {def.defName} hediff to {pawn.NameFullColored} with Status={sourceAbility.status}");
            sourceAbilities.Add(sourceAbility);
            Severity = CalculateSeverity();
        }

        public void RemoveSource(AbstractAbility sourceAbility, bool calculateSeverity = false) {
            if (!sourceAbilities.Contains(sourceAbility)) return;

            Log.Warning(
                $"{sourceAbility.pawn.NameFullColored} removing {def.defName} hediff from {pawn.NameFullColored}");
            sourceAbilities.Remove(sourceAbility);
            if (calculateSeverity) Severity = CalculateSeverity();
        }

        public override void Tick() {
            var toRemove = sourceAbilities.Where(x => x.status == BurningStatus.Off || x.pawn.DestroyedOrNull())
                .ToArray();
            foreach (var ability in toRemove) {
                RemoveSource(ability);
            }

            var surge = AllomancyUtility.GetSurgeBurn(pawn);
            if (surge != null && def.defName != surge.def.defName) {
                surge?.Burn(
                    () => { Severity = CalculateSeverity(); },
                    (int)TickUtility.TICKS_PER_SECOND,
                    () => { Severity = CalculateSeverity(); }
                );
            }

            base.Tick();
        }

        public float CalculateSeverity() {
            return sourceAbilities.Sum(x => x.GetStrength());
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
            foreach (var localPawn in sourcePawns) {
                foreach (var ability in localPawn?.abilities?.abilities ?? []) {
                    if (ability is not AbstractAbility aa) continue;
                    // If this pawn has any Allomantic ability, we re-attach it
                    sourceAbilities.Add(aa);
                    // Optional: maybe only add one matching ability type, if you can correlate them
                    break;
                }
            }
        }

        public override string DebugString() {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(base.DebugString());

            stringBuilder.AppendLine("Severity Sources:");
            foreach (var ability in sourceAbilities) {
                stringBuilder.AppendLine(
                    $"  {ability.pawn.NameShortColored} -> {ability.def.LabelCap}: {ability.GetStrength():0.0000}");
            }

            return stringBuilder.ToString();
        }
    }
}