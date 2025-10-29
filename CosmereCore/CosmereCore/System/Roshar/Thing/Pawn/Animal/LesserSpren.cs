using Verse;

namespace Cosmere.System.Roshar.Thing.Pawn.Animal;

public class LesserSpren : Spren {
    public override void SpawnSetup(Map map, bool respawningAfterLoad) {
        base.SpawnSetup(map, respawningAfterLoad);
        Destroy();
    }
}