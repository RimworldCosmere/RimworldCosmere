using Cosmere.Core.Comp.Game;
using Cosmere.System.Roshar.Comp.Map;
using Cosmere.System.Roshar.Utility;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Thing.Pawn.Animal;

public class TrueSpren : Spren {
    private Verse.Pawn? bond => playerSettings?.Master;
    private Connection? connectionWithBond => bond == null ? null : this.GetConnection(bond);

    public override void Notify_SignalReceived(Signal signal) {
        base.Notify_SignalReceived(signal);
        if (signal.tag is not SpiritWeb.CHANGED_SIGNAL) return;
        CheckForBond();
    }

    private void CheckForBond() {
        if (bond == null || connectionWithBond == null) return;
        if (connectionWithBond.value < 1.0) return;

        if (RadiantOrder.BondWithSpren(bond)) return;

        Map.GetComponent<TrueSprenSpawner>().TryDespawnSpren(bond, this);
        Destroy();
    }

    protected override void TickInterval(int delta) {
        base.TickInterval(delta);
        if (!this.IsHashIntervalTick(GenTicks.TickLongInterval, delta)) return;
        CheckForBond();
    }
}