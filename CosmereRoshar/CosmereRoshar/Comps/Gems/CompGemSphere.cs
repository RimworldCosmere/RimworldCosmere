using RimWorld;
using Verse;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Reflection.Emit;
using System.Security.Cryptography;
using Verse.AI;

namespace CosmereRoshar {
    public enum GemSize { None, Chip, Mark, Broam };
    public class CompGemSphere : ThingComp {
        public CompProperties_GemSphere GemstoneProps => (CompProperties_GemSphere)props;
        public StormlightProperties StormlightProps => (StormlightProperties)props;
        private CompStormlight stormlightComp;
        private int gemstoneQuality;
        private int gemstoneSize;
        public bool inheritGemstone = false;
        public int inheritGemstoneQuality = 1;
        public int inheritGemstoneSize = 1;
        public string GetFullLabel => TransformLabel(parent.Label);
        public override void Initialize(CompProperties props) {
            base.Initialize(props);

            stormlightComp = parent.GetComp<CompStormlight>();
            if (inheritGemstone == false) {
                List<int> sizeList = new List<int>() { 1, 5, 20 };
                List<int> qualityList = new List<int>() { 1, 2, 3, 4, 5 };

                gemstoneQuality = StormlightUtilities.RollForRandomIntFromList(qualityList);   //make better roller later with lower prob for bigger size
                gemstoneSize = StormlightUtilities.RollForRandomIntFromList(sizeList);         //make better roller later with lower prob for bigger size
            }
            else {
                gemstoneQuality = inheritGemstoneQuality;
                gemstoneSize = inheritGemstoneSize;
            }

            //parent.def.BaseMarketValue = (parent.def.BaseMarketValue * gemstoneSize) + gemstoneQuality * 5;

            //Log.Message($"Base Value {parent.def.BaseMarketValue}");
            if (stormlightComp != null) {
                // stormlightComp.StormlightContainerQuality = gemstoneQuality;
                stormlightComp.StormlightContainerSize = gemstoneSize;
                stormlightComp.calculateMaximumGlowRadius(gemstoneQuality, gemstoneSize);
            }
        }

        public GemSize GetGemSize() {
            switch (gemstoneSize) {
                case 1:
                    return GemSize.Chip;
                case 5:
                    return GemSize.Mark;
                case 20:
                    return GemSize.Broam;
                default:
                    return GemSize.Chip;
            }
        }

        public override bool AllowStackWith(Thing other) {
            CompGemSphere comp = other.TryGetComp<CompGemSphere>();
            if (comp != null) {
                if (comp.gemstoneQuality != this.gemstoneQuality || comp.gemstoneSize != this.gemstoneSize) {
                    return false;
                }
            }
            return true;
        }

        public override string TransformLabel(string label) {
            string sizeLabel = "";
            string qualityLabel = "";
            switch (gemstoneSize) {
                case 1:
                    sizeLabel = " chip";
                    break;
                case 5:
                    sizeLabel = " mark";
                    break;
                case 20:
                    sizeLabel = " broam";
                    break;
                default:
                    break;
            }
            switch (gemstoneQuality) {
                case 1:
                    qualityLabel = "flawed ";
                    break;
                case 2:
                    qualityLabel = "imperfect ";
                    break;
                case 3:
                    qualityLabel = "standard ";
                    break;
                case 4:
                    qualityLabel = "flawless ";
                    break;
                case 5:
                    qualityLabel = "perfect ";
                    break;

                default:
                    break;
            }
            return qualityLabel + label + sizeLabel;
        }

        // Called after loading or on spawn
        public override void PostExposeData() {
            base.PostExposeData();
            Scribe_Values.Look(ref gemstoneQuality, "gemstoneQuality", 1);
            Scribe_Values.Look(ref gemstoneSize, "gemstoneSize", 1);
        }

        public override void CompTick() {
            base.CompTick();
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra() {
            if (CosmereRoshar.DevOptionAutofillSpheres && stormlightComp != null) {

                yield return new Command_Action {
                    defaultLabel = "Fill sphere with 10 stormlight",
                    defaultDesc = "Debug/Dev feature.",
                    //icon = ContentFinder<Texture2D>.Get("UI/Icons/SomeIcon"), 
                    icon = TexCommand.DesirePower,
                    action = () => {
                        stormlightComp.infuseStormlight(10f);
                    }
                };

                yield return new Command_Action {
                    defaultLabel = "Fill sphere with 100 stormlight",
                    defaultDesc = "Debug/Dev feature.",
                    //icon = ContentFinder<Texture2D>.Get("UI/Icons/SomeIcon"), 
                    icon = TexCommand.DesirePower,
                    action = () => {
                        stormlightComp.infuseStormlight(100f);
                    }
                };

                yield return new Command_Action {
                    defaultLabel = "Fill sphere with maximum stormlight",
                    defaultDesc = "Debug/Dev feature.",
                    //icon = ContentFinder<Texture2D>.Get("UI/Icons/SomeIcon"), 
                    icon = TexCommand.DesirePower,
                    action = () => {
                        stormlightComp.infuseStormlight(stormlightComp.CurrentMaxStormlight);
                    }
                };
            }
            yield break;
        }
    }
}

namespace CosmereRoshar {
    public class CompProperties_GemSphere : CompProperties {
        public CompProperties_GemSphere() {
            this.compClass = typeof(CompGemSphere);
        }
    }
}
