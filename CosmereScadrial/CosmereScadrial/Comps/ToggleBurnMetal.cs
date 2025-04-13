using System;
using CosmereScadrial.Comps.Properties;
using CosmereScadrial.Hediffs.Comps;
using RimWorld;
using Verse;

namespace CosmereScadrial.Comps {
    public class CompToggleBurnMetal : CompAbilityEffect {
        public bool burning;

        public new ToggleBurnMetal Props {
            get => (ToggleBurnMetal)props;
        }

        public override void PostExposeData() {
            Scribe_Values.Look(ref burning, "burning");
            base.PostExposeData();
        }

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest) {
            base.Apply(target, dest);

            var pawn = parent.pawn;

            Log.Warning($"{target.Pawn.NameFullColored} using ToggleBurnMetal, hediff={Props.hediff} burning={burning}");
            if (Props.hediff == null) {
                throw new Exception("Missing hediff");
            }

            Hediff hediff;
            if (burning) {
                hediff = pawn.health.hediffSet.GetFirstHediffOfDef(Props.hediff);
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

            hediff = HediffMaker.MakeHediff(Props.hediff, pawn);
            hediff.Severity = 1f;
            pawn.health.AddHediff(hediff);
            Log.Error($"Added {Props.hediff} hediff to pawn {pawn}");
            burning = true;
        }
    }
}