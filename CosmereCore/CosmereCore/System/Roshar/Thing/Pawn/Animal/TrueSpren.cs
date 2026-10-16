using Cosmere.Core.Comp.Game;
using Cosmere.System.Roshar.Comp.Map;
using Cosmere.System.Roshar.Util;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Thing.Pawn.Animal;

public class TrueSpren : Spren {
    private Verse.Pawn? Bond => playerSettings?.Master;

    private Connection? ConnectionWithBond => Bond == null ? null : this.GetConnection(Bond);

    public override void Notify_SignalReceived(Signal signal) {
        base.Notify_SignalReceived(signal);
        if (signal.tag is not SpiritWeb.ChangedSignal) return;
        TryConsummateBond();
    }

    private void TryConsummateBond() {
        if (Bond == null || ConnectionWithBond == null) return;
        if (ConnectionWithBond.Value < 1.0) return;

        if (RadiantOrderUtility.BondWithSpren(Bond)) return;

        Map.GetComponent<TrueSprenSpawner>().DespawnSpren(Bond, this);
        Destroy();
    }

    protected override void TickInterval(int delta) {
        base.TickInterval(delta);
        if (!this.IsHashIntervalTick(GenTicks.TickLongInterval, delta)) return;
        TryConsummateBond();
    }
}
