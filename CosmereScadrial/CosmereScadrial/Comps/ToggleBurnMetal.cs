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

        protected string Metal => parent.def.GetModExtension<MetalLinked>().metal;

        protected bool AtLeastPassive => Comp.Status > ToggleBurnMetalStatus.Passive;

        protected bool Burning => Comp.Status == ToggleBurnMetalStatus.Burning;

        protected bool Flaring => Comp.Status == ToggleBurnMetalStatus.Flaring;

        public ToggleBurnMetalStatus Status => Comp.Status;

        public new Properties.ToggleBurnMetal Props => (Properties.ToggleBurnMetal)props;

        public override void Initialize(AbilityCompProperties props) {
            base.Initialize(props);

            if (Props.hediff == null) Log.Error("Missing hediff");
            if (Comp == null) Log.Error($"Failed to find BurnMetal Comp on {Props.hediff}");
        }

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest) {
            base.Apply(target, dest);

            Log.Info($"{parent.pawn.NameFullColored} using ToggleBurnMetal against {target.Pawn.NameFullColored}, hediff={Props.hediff} currentStatus={Status.ToString()}");

            Comp.Target = target;
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
        }
    }
}