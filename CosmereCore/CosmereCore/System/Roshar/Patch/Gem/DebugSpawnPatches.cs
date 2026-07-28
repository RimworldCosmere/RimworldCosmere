using Concord;
using RimWorld;

namespace Cosmere.System.Roshar.Patch.Gem;

[Patch]
public abstract class DebugSpawnPatch : Verse.Thing {
    [Inject(At.Return, nameof(Notify_DebugSpawned))]
    private void AfterNotify_DebugSpawned() {
        Verse.Thing self = this;
        if (self.def.IsOneOf(
                ThingDefOf.Cosmere_Roshar_Thing_Broam,
                ThingDefOf.Cosmere_Roshar_Thing_Mark,
                ThingDefOf.Cosmere_Roshar_Thing_Chip
            ) &&
            self.Stuff == Core.ThingDefOf.CutGem) {
            self.SetStuffDirect(GenStuff.RandomStuffFor(Core.ThingDefOf.CutGem));
        }
    }
}
