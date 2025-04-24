using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace CosmereScadrial.Abilities.Allomancy.Hediffs {
    public class AllomanticHediff : HediffWithComps {
        public List<AbstractAbility> sourceAbilities = [];

        public override bool ShouldRemove => sourceAbilities.Count == 0 || base.ShouldRemove;

        public override string LabelBase => base.LabelBase + (sourceAbilities.Count > 1 ? $" ({sourceAbilities.Count} sources)" : "");

        public void AddSource(AbstractAbility sourceAbility) {
            if (sourceAbilities.Contains(sourceAbility)) return;

            Log.Warning($"{sourceAbility.pawn.NameFullColored} applying {def.defName} hediff to {pawn.NameFullColored} with Status={sourceAbility.status}");
            sourceAbilities.Add(sourceAbility);
            Severity = CalculateSeverity();
        }

        public void RemoveSource(AbstractAbility sourceAbility, bool recalculate = true) {
            if (!sourceAbilities.Contains(sourceAbility)) return;

            Log.Warning($"{sourceAbility.pawn.NameFullColored} removing {def.defName} hediff from {pawn.NameFullColored}");
            sourceAbilities.Remove(sourceAbility);
            if (recalculate) Severity = CalculateSeverity();
        }

        public override void Tick() {
            var toRemove = sourceAbilities.Where(x => x.status == BurningStatus.Off || x.pawn.DestroyedOrNull()).ToArray();
            foreach (var ability in toRemove) {
                RemoveSource(ability, false);
            }

            Severity = CalculateSeverity();
            base.Tick();
        }

        protected virtual float CalculateSeverity() {
            return sourceAbilities.Sum(GetContribution);
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
                stringBuilder.AppendLine($"  {ability.pawn.NameShortColored} -> {ability.def.LabelCap}: {GetContribution(ability):0.0000}");
            }

            return stringBuilder.ToString();
        }

        private float GetContribution(AbstractAbility ability) {
            const int multiplier = 12;
            var rawPower = ability.pawn.GetStatValue(StatDefOf.Cosmere_Allomantic_Power);
            var statusValue = (ability.status ?? BurningStatus.Burning) switch {
                BurningStatus.Off => 0f,
                BurningStatus.Passive => 0.5f,
                BurningStatus.Burning => 1f,
                BurningStatus.Flaring => 2f,
                _ => 0f,
            };

            return multiplier * ability.def.hediffSeverityFactor * rawPower * statusValue;
        }
    }
}