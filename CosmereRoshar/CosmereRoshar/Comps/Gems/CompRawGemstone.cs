using RimWorld;
using Verse;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Security.Cryptography;

namespace CosmereRoshar {
    public class CompRawGemstone : ThingComp {
        public CompProperties_RawGemstone GemstoneProps => (CompProperties_RawGemstone)props;
        public StormlightProperties StormlightProps => (StormlightProperties)props;
        private CompStormlight stormlightComp;
        public int gemstoneSize;

        public override void Initialize(CompProperties props) {
            base.Initialize(props);
            stormlightComp = parent.GetComp<CompStormlight>();
            List<int> sizeList = new List<int>() { 1, 5, 20 };
            gemstoneSize = StormlightUtilities.RollForRandomIntFromList(sizeList);   //make better roller later with lower prob for bigger size

            if (stormlightComp != null) {
                stormlightComp.StormlightContainerSize = gemstoneSize;
            }
        }

        // Called after loading or on spawn
        public override void PostExposeData() {
            base.PostExposeData();
        }

        public override bool AllowStackWith(Thing other) {
            CompRawGemstone comp = other.TryGetComp<CompRawGemstone>();
            if (comp != null) {
                if (comp.gemstoneSize != this.gemstoneSize) {
                    return false;
                }
            }
            return true;
        }

        public override string TransformLabel(string label) {
            switch (gemstoneSize) {
                case 1:
                    return "small " + label;
                case 5:
                    return "average " + label;
                case 20:
                    return "large " + label;
                default:
                    return label;
            }
        }

        public override void CompTick() {
            base.CompTick();
        }
    }
}

namespace CosmereRoshar {
    public class CompProperties_RawGemstone : CompProperties {
        public int spawnChance;
        public CompProperties_RawGemstone() {
            this.compClass = typeof(CompRawGemstone);
        }
    }
}
