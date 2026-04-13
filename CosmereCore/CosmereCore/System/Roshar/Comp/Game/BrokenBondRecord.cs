using Verse;

namespace Cosmere.System.Roshar.Comp.Game;

public class BrokenBondRecord : IExposable {
    public string orderDefName = "";
    public int breakTick;

    public void ExposeData() {
        Scribe_Values.Look(ref orderDefName!, "orderDefName");
        Scribe_Values.Look(ref breakTick, "breakTick");
    }
}
