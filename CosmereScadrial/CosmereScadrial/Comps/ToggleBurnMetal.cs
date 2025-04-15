using System.Collections.Generic;
using CosmereScadrial.DefModExtensions;
using CosmereScadrial.Hediffs;
using CosmereScadrial.Hediffs.Comps;
using RimWorld;
using Verse;
using Log = CosmereFramework.Log;

namespace CosmereScadrial.Comps {
    public enum ToggleBurnMetalStatus {
        Off,
        Passive,
        Burning,
        Flaring,
    }

    public class ToggleBurnMetal : CompAbilityEffect {
        public BurningMetal Hediff => (BurningMetal)parent.pawn.health.GetOrAddHediff(Props.hediff);

        private BurnMetal Comp => Hediff.TryGetComp<BurnMetal>();

        public string Metal => parent.def.GetModExtension<MetalLinked>().metal;

        public bool AtLeastPassive => Status > ToggleBurnMetalStatus.Passive;

        public bool Burning => Status == ToggleBurnMetalStatus.Burning;

        public bool Flaring => Status == ToggleBurnMetalStatus.Flaring;

        public ToggleBurnMetalStatus Status => Hediff.Severity switch {
            <= 0f => ToggleBurnMetalStatus.Off,
            <= 0.5f => ToggleBurnMetalStatus.Passive,
            <= 1f => ToggleBurnMetalStatus.Burning,
            _ => ToggleBurnMetalStatus.Flaring,
        };

        public new Properties.ToggleBurnMetal Props => (Properties.ToggleBurnMetal)props;

        public override IEnumerable<Gizmo> CompGetGizmosExtra() {
            foreach (var command in parent.GetGizmos()) {
                // yield return Activator.CreateInstance(command.GetType(), parent, parent.pawn) as Gizmo;
            }

            yield break;
        }

        public override void Initialize(AbilityCompProperties props) {
            base.Initialize(props);

            if (Props.hediff == null) Log.Error("Missing hediff");
            if (Comp == null) Log.Error($"Failed to find BurnMetal Comp on {Props.hediff}");
        }

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest) {
            base.Apply(target, dest);

            Log.Info($"{target.Pawn.NameFullColored} using ToggleBurnMetal, hediff={Props.hediff} currentStatus={Status.ToString()}");

            if (AtLeastPassive) {
                Comp.ToggleBurn(false);
                return;
            }

            Comp.ToggleBurn(true);
        }

        public void ToggleFlaring() {
            ToggleFlaring(!Flaring);
        }

        public void ToggleFlaring(bool nextFlaring) {
            Comp.UpdateSeverity(nextFlaring ? 2f : 1f);
            Log.Verbose($"Toggled flaring on {Metal}: {(nextFlaring ? "Flare" : "Normal")} hediff={Hediff.def.defName} comp={Comp}");
        }
    }
}