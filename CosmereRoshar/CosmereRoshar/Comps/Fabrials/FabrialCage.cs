using Verse;

namespace CosmereRoshar {
    public class CompFabrialCage : ThingComp {
        public CompProperties_FabrialCage Props => (CompProperties_FabrialCage)props;


        public override void PostSpawnSetup(bool respawningAfterLoad) {
            base.PostSpawnSetup(respawningAfterLoad);
        }

        public override void PostExposeData() {
            base.PostExposeData();
        }
    }
}

namespace CosmereRoshar {
    public class CompProperties_FabrialCage : CompProperties {
        public CompProperties_FabrialCage() {
            compClass = typeof(CompFabrialCage);
        }
    }
}