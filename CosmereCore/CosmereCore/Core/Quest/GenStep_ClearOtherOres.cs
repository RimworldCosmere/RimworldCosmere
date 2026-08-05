using System.Collections.Generic;
using RimWorld;
using Verse;
using Logger = Cosmere.Core.Logger;

namespace Cosmere.Core.Quest;

/// <summary>
///     Removes every ore except one from a generated map. The base map generator scatters
///     steel, gold, plasteel and the rest into any rock it finds, which buries a quest's own
///     vein in noise - on a site that exists to be one resource, nothing else should be worth
///     swinging a pick at.
///     <para>Plain rock is left alone: it is what the veins are embedded in.</para>
/// </summary>
public class GenStep_ClearOtherOres : GenStep {
    public ThingDef? keep;

    public override int SeedPart => 744192077;

    public override void Generate(Verse.Map map, GenStepParams parms) {
        ThingDef? kept = keep;
        if (kept == null) {
            Logger.Error("GenStep_ClearOtherOres has no keep def.");
            return;
        }

        List<Verse.Thing> doomed = new List<Verse.Thing>();
        List<Verse.Thing> all = map.listerThings.AllThings;
        for (int i = 0; i < all.Count; i++) {
            Verse.Thing thing = all[i];
            if (thing.def == kept || thing is not Mineable) continue;

            // isResourceRock is the split between ore and the rock it sits in.
            if (thing.def.building?.isResourceRock != true) continue;

            doomed.Add(thing);
        }

        for (int i = 0; i < doomed.Count; i++) {
            if (!doomed[i].Destroyed) doomed[i].Destroy(DestroyMode.Vanish);
        }

        Logger.Verbose($"GenStep_ClearOtherOres: removed {doomed.Count} ore cells, keeping {kept.defName}.");
    }
}
