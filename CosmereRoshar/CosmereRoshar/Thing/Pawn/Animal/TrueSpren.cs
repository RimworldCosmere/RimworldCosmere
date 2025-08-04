using Cosmere.Core.Comp.Game;
using Cosmere.Roshar.Comp.Map;
using Cosmere.Roshar.Def;
using RimWorld;
using Verse;

namespace Cosmere.Roshar.Thing.Pawn.Animal;

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

        if (bond.IsSurgebinder()) return;

        // Random for now, but this will eventually pause the game and give the player a chance to PICK their order
        IEnumerable<RadiantOrderDef> orders =
            DefDatabase<RadiantOrderDef>.AllDefsListForReading.Where(x => x.defName != "Bondsmith");
        RadiantOrderDef order = orders.RandomElement();
        bond.genes.TryAddRadiantOrder(order.GetSurgebindingGene());
        Messages.Message(
            $"{bond.NameFullColored} has become a member of the {order.LabelCap} Radiant Order!",
            bond,
            MessageTypeDefOf.PositiveEvent
        );
        Map.GetComponent<TrueSprenSpawner>().TryDespawnSpren(bond, this);
        Destroy();
    }

    protected override void TickInterval(int delta) {
        base.TickInterval(delta);
        if (!this.IsHashIntervalTick(GenTicks.TickLongInterval, delta)) return;
        CheckForBond();
    }
}