using Concord;
using Cosmere.System.Roshar.Surgebinding;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Patch.IdealTracking;

[Patch]
public abstract class ArtDestructionViolationPatch : Verse.Thing {
    [Inject(At.Head, nameof(Destroy))]
    private void BeforeDestroy(DestroyMode mode) {
        if (mode != DestroyMode.Deconstruct) return;

        CompArt compArt = this.TryGetComp<CompArt>();
        if (compArt == null) return;
        if (!compArt.Active) return;

        Map map = Map;
        if (map == null) return;

        List<Pawn> colonists = map.mapPawns.FreeColonistsSpawned;
        for (int i = 0; i < colonists.Count; i++) {
            Pawn colonist = colonists[i];
            if (ViolationUtility.IsSurgebinderOfOrder(colonist, RadiantOrderDefOf.Lightweaver)) {
                ViolationUtility.ApplyViolation(colonist, 0.1f, "deconstructing artwork");
            }
        }
    }
}
