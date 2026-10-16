using Cosmere.Core.Def;
using Verse;

namespace Cosmere.Core.Entity;

public class Shard : Verse.Entity, IExposable, ILoadReferenceable {
    public ShardDef def;

    public Shard(ShardDef def) {
        this.def = def;
    }

    public override string LabelCap => def.LabelCap;

    public override string Label => def.label;

    public void ExposeData() { }

    public string GetUniqueLoadID() {
        return "Shard_" + def.defName;
    }

    public override void SpawnSetup(Map map, bool respawningAfterLoad) { }

    public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish) { }
}
