using Verse;

namespace Cosmere.Roshar.Thing.Pawn.Animal;

public class LesserSpren : Spren {
    public override void SpawnSetup(Map map, bool respawningAfterLoad) {
        base.SpawnSetup(map, respawningAfterLoad);
        Destroy();
    }
}