using System.Collections.Generic;
using CosmereRoshar.Comp;
using CosmereRoshar.Comp.Thing;
using RimWorld;
using Verse;

namespace CosmereRoshar.Comps.Gems;

public enum GemSize {
    None,
    Chip,
    Mark,
    Broam,
}

public class CompGemSphere : ThingComp {
    private int gemstoneQuality;
    private int gemstoneSize;
    public bool inheritGemstone = false;
    public int inheritGemstoneQuality = 1;
    public int inheritGemstoneSize = 1;
    private Stormlight stormlight;
    public CompPropertiesGemSphere gemstoneProps => (CompPropertiesGemSphere)props;
    public StormlightProperties stormlightProps => (StormlightProperties)props;
    public string getFullLabel => TransformLabel(parent.Label);

    public override void Initialize(CompProperties props) {
        base.Initialize(props);

        stormlight = parent.GetComp<Stormlight>();
        if (inheritGemstone == false) {
            List<int> sizeList = new List<int> { 1, 5, 20 };
            List<int> qualityList = new List<int> { 1, 2, 3, 4, 5 };

            gemstoneQuality =
                StormlightUtilities
                    .RollForRandomIntFromList(
                        qualityList
                    ); //make better roller later with lower prob for bigger size
            gemstoneSize =
                StormlightUtilities
                    .RollForRandomIntFromList(sizeList); //make better roller later with lower prob for bigger size
        } else {
            gemstoneQuality = inheritGemstoneQuality;
            gemstoneSize = inheritGemstoneSize;
        }

        //parent.def.BaseMarketValue = (parent.def.BaseMarketValue * gemstoneSize) + gemstoneQuality * 5;

        //Log.Message($"Base Value {parent.def.BaseMarketValue}");
        if (stormlight != null) {
            // stormlightComp.StormlightContainerQuality = gemstoneQuality;
            stormlight.stormlightContainerSize = gemstoneSize;
            stormlight.CalculateMaximumGlowRadius(gemstoneQuality, gemstoneSize);
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
            if (comp.gemstoneQuality != gemstoneQuality || comp.gemstoneSize != gemstoneSize) {
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
        if (CosmereRoshar.devOptionAutofillSpheres && stormlight != null) {
            yield return new Command_Action {
                defaultLabel = "Fill sphere with 10 stormlight",
                defaultDesc = "Debug/Dev feature.",
                //icon = ContentFinder<Texture2D>.Get("UI/Icons/SomeIcon"), 
                icon = TexCommand.DesirePower,
                action = () => { stormlight.InfuseStormlight(10f); },
            };

            yield return new Command_Action {
                defaultLabel = "Fill sphere with 100 stormlight",
                defaultDesc = "Debug/Dev feature.",
                //icon = ContentFinder<Texture2D>.Get("UI/Icons/SomeIcon"), 
                icon = TexCommand.DesirePower,
                action = () => { stormlight.InfuseStormlight(100f); },
            };

            yield return new Command_Action {
                defaultLabel = "Fill sphere with maximum stormlight",
                defaultDesc = "Debug/Dev feature.",
                //icon = ContentFinder<Texture2D>.Get("UI/Icons/SomeIcon"), 
                icon = TexCommand.DesirePower,
                action = () => { stormlight.InfuseStormlight(stormlight.currentMaxStormlight); },
            };
        }

        yield break;
    }
}

public class CompPropertiesGemSphere : CompProperties {
    public CompPropertiesGemSphere() {
        compClass = typeof(CompGemSphere);
    }
}