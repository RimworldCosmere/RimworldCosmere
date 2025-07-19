using Verse;

namespace CosmereRoshar.Comp.Fabrials;

public class CompFabrialCage : ThingComp {
    public CompPropertiesFabrialCage props => (CompPropertiesFabrialCage)base.props;


    public override void PostSpawnSetup(bool respawningAfterLoad) {
        base.PostSpawnSetup(respawningAfterLoad);
    }

    public override void PostExposeData() {
        base.PostExposeData();
    }
}

public class CompPropertiesFabrialCage : CompProperties {
    public CompPropertiesFabrialCage() {
        compClass = typeof(CompFabrialCage);
    }
}