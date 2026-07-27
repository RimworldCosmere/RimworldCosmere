using Verse;

namespace Cosmere.System.Roshar.Comp.Game;

public class BrokenBondRecord : IExposable {
    public int breakTick;
    public string orderDefName = string.Empty;

    public void ExposeData() {
        Scribe_Values.Look(ref orderDefName!, "orderDefName");
        Scribe_Values.Look(ref breakTick, "breakTick");
    }
}
