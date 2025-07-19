using System.Collections.Generic;
using Verse;

namespace CosmereRoshar.Comp.Thing;

public class RawGemstoneProperties : CompProperties {
    public int spawnChance;

    public RawGemstoneProperties() {
        compClass = typeof(RawGemstone);
    }
}

public class RawGemstone : ThingComp {
    private Stormlight? cachedStormlight;
    public int gemstoneSize;
    private Stormlight? stormlight => cachedStormlight ??= parent.TryGetComp<Stormlight>();

    public override void Initialize(CompProperties props) {
        base.Initialize(props);
        // @todo Remove sizes
        List<int> sizeList = new List<int> { 1, 5, 20 };
        // make better roller later with lower prob for bigger size
        gemstoneSize = StormlightUtilities.RollForRandomIntFromList(sizeList);

        if (stormlight != null) {
            stormlight.stormlightContainerSize = gemstoneSize;
        }
    }

    public override bool AllowStackWith(Verse.Thing other) {
        if (!other.TryGetComp(out RawGemstone comp)) return true;

        return comp.gemstoneSize == gemstoneSize;
    }

    public override string TransformLabel(string label) {
        return gemstoneSize switch {
            1 => "small " + label,
            5 => "average " + label,
            20 => "large " + label,
            _ => label,
        };
    }
}