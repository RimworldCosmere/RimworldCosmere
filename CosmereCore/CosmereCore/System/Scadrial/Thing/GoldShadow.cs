using Verse;

namespace Cosmere.System.Scadrial.Thing;

public class GoldShadow : Pawn {
    public Pawn? owner;

    public override void ExposeData() {
        base.ExposeData();
        Scribe_References.Look(ref owner, "owner");
    }

    protected override void Tick() {
        base.Tick();

        if (Destroyed) return;

        // Nothing else keeps the shadow alive, so an orphaned one has to clear itself out.
        if (owner is not { Destroyed: false, Dead: false }) {
            Destroy();
        }
    }
}
