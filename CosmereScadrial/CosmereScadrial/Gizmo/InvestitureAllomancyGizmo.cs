using System.Collections.Generic;
using CosmereScadrial.Comps.Things;
using CosmereScadrial.Defs;
using CosmereScadrial.Genes;
using UnityEngine;
using Verse;

namespace CosmereScadrial.Gizmo {
    public class InvestitureAllomancyGizmo(Metalborn gene, MetallicArtsMetalDef metal) : Gizmo_Slider {
        protected readonly Metalborn gene = gene;
        protected readonly MetallicArtsMetalDef metal = metal;
        protected Pawn pawn => gene.pawn;
        protected MetalReserves reserves => pawn.GetComp<MetalReserves>();
        protected MetalBurning burning => pawn.GetComp<MetalBurning>();
        protected override bool IsDraggable => gene.pawn.IsColonistPlayerControlled || gene.pawn.IsPrisonerOfColony;
        protected override Color BarColor => metal.color;
        protected override string BarLabel => $"{ValuePercent:F}%";

        protected override float Target {
            get => reserves.GetReserve(metal) / MetalReserves.MAX_AMOUNT;
            set => reserves.SetReserve(metal, value);
        }

        protected override float ValuePercent => reserves.GetReserve(metal) / MetalReserves.MAX_AMOUNT;
        protected override string Title => metal.LabelCap;

        protected override string GetTooltip() {
            var value = reserves.GetReserve(metal);
            var rate = burning.GetTotalBurnRate(metal) * GenTicks.TicksPerRealSecond;
            var tooltip =
                $"{metal.label.CapitalizeFirst()}\n\n{metal.allomancy.description}\n\n{value:N0} / {MetalReserves.MAX_AMOUNT:N0}";
            if (burning.IsBurning(metal)) {
                tooltip += $" ({rate:0.000}/sec)";
            }

            //GeneGizmo_Resource;
            //GeneGizmo_ResourceHemogen;

            return tooltip;
        }

        protected override IEnumerable<float> GetBarThresholds() {
            for (var i = 0; i < 100; i += 10) {
                yield return i;
            }
        }
    }
}