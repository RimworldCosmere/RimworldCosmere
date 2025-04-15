using System;
using System.Collections.Generic;
using CosmereScadrial.Comps.Properties;
using CosmereScadrial.DefModExtensions;
using CosmereScadrial.Hediffs.Comps;
using RimWorld;
using UnityEngine;
using Verse;
using Log = CosmereFramework.Log;

namespace CosmereScadrial.Comps {
    public enum ToggleBurnMetalStatus {
        Off,
        Burning,
        Flaring,
    }

    public class CompToggleBurnMetal : CompAbilityEffect {
        public Hediff hediff { get; private set; }

        public string Metal => parent.def.GetModExtension<MetalLinked>().metal;

        public bool Burning => hediff != null;

        public bool Flaring => hediff?.Severity > 1f;

        public ToggleBurnMetalStatus Status {
            get {
                if (hediff == null) {
                    return ToggleBurnMetalStatus.Off;
                }

                return Mathf.Approximately(hediff.Severity, 1f) ? ToggleBurnMetalStatus.Burning : ToggleBurnMetalStatus.Flaring;
            }
        }

        public new ToggleBurnMetal Props => (ToggleBurnMetal)props;

        public override IEnumerable<Gizmo> CompGetGizmosExtra() {
            var pawn = parent.pawn;

            foreach (var ability in pawn.abilities.abilities) {
                foreach (var command in ability.GetGizmos()) {
                    yield return command is Commands.ToggleBurnMetal ? new Commands.ToggleBurnMetal(ability, pawn) : command;
                }
            }
        }

        public override void PostExposeData() {
            var b = Burning;
            Scribe_Values.Look(ref b, "burning");
            var f = Flaring;
            Scribe_Values.Look(ref f, "flaring");
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

            Log.Warning($"{target.Pawn.NameFullColored} using ToggleBurnMetal, hediff={Props.hediff} currentBurning={Burning} currentFlaring={Flaring}");

            if (Burning) {
                hediff = parent.pawn.health.hediffSet.GetFirstHediffOfDef(Props.hediff);
                if (hediff != null) {
                    var comp = hediff.TryGetComp<BurnMetal>();
                    if (comp == null) {
                        Log.Error("Missing BurnMetal HediffComp");
                        return;
                    }

                    comp.Stop();
                    hediff = null;
                    return;
                }
            }

            hediff = HediffMaker.MakeHediff(Props.hediff, parent.pawn);
            hediff.Severity = Flaring ? 2f : 1f;
            parent.pawn.health.AddHediff(hediff);
        }

        public void ToggleFlaring() {
            ToggleFlaring(!Flaring);
        }

        public void ToggleFlaring(bool nextFlaring) {
            hediff ??= parent.pawn.health.hediffSet.GetFirstHediffOfDef(Props.hediff);

            if (hediff == null) {
                Log.Warning($"Pawn doesnt have {Props.hediff}");
                return;
            }

            var comp = hediff.TryGetComp<BurnMetal>();
            if (comp == null) {
                Log.Error("Missing BurnMetal HediffComp");
                return;
            }

            comp.UpdateSeverity(nextFlaring ? 2f : 1f);
            Log.Verbose($"Toggled flaring on {Metal}: {(nextFlaring ? "Flare" : "Normal")} hediff={hediff.def.defName} comp={comp}");
        }
    }
}