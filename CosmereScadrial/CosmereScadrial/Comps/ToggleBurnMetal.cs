using System;
using CosmereScadrial.Comps.Properties;
using CosmereScadrial.Hediffs.Comps;
using RimWorld;
using Verse;

namespace CosmereScadrial.Comps {
    public class CompToggleBurnMetal : CompAbilityEffect {
        public bool burning;

        public bool flaring;

        private Hediff hediff;

        public new ToggleBurnMetal Props {
            get => (ToggleBurnMetal)props;
        }

        public override void PostExposeData() {
            Scribe_Values.Look(ref burning, "burning");
            Scribe_Values.Look(ref flaring, "flaring");
            base.PostExposeData();
        }

        public override void Initialize(AbilityCompProperties props) {
            base.Initialize(props);

            if (Props.hediff == null) {
                throw new Exception("Missing hediff");
            }
        }

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest) {
            base.Apply(target, dest);

            Log.Warning($"{target.Pawn.NameFullColored} using ToggleBurnMetal, hediff={Props.hediff} burning={burning}");

            if (burning) {
                hediff = parent.pawn.health.hediffSet.GetFirstHediffOfDef(Props.hediff);
                if (hediff != null) {
                    var comp = hediff.TryGetComp<BurnMetal>();
                    if (comp == null) {
                        throw new Exception("Missing BurnMetal HediffComp");
                    }

                    comp.Toggle();
                    burning = false;
                    return;
                }

                burning = false;
            }

            hediff = HediffMaker.MakeHediff(Props.hediff, parent.pawn);
            hediff.Severity = flaring ? 2f : 1f;
            parent.pawn.health.AddHediff(hediff);
            burning = true;
        }

        public void ToggleFlaring() {
            flaring = !flaring;
            hediff ??= parent.pawn.health.hediffSet.GetFirstHediffOfDef(Props.hediff);

            if (hediff == null) return;
            var comp = hediff.TryGetComp<BurnMetal>();
            if (comp == null) {
                throw new Exception("Missing BurnMetal HediffComp");
            }

            comp.UpdateSeverity(flaring ? 2f : 1f);
        }
    }
}