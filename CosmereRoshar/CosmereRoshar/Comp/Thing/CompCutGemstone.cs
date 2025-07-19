using System.Collections.Generic;
using RimWorld;
using Verse;

namespace CosmereRoshar.Comp.Thing;

public class SphereRuby : ThingDef { }

public enum Spren {
    None,
    Flame,
    Cold,
    Pain,
    Anger,
    Wind,
    Motion,
    Life,
    Cultivation,
    Rain,
    Glory,
    Light,
    Exhaustion,
    Logic,
}

public class CompCutGemstone : ThingComp {
    public Spren capturedSpren = Spren.None;
    public int gemstoneQuality;
    public int gemstoneSize;
    public int maximumGemstoneSize = 20;
    private Stormlight stormlight;
    public CompPropertiesCutGemstone gemstoneProps => (CompPropertiesCutGemstone)props;
    public StormlightProperties stormlightProps => (StormlightProperties)props;
    public bool hasSprenInside => capturedSpren != Spren.None;
    public string getFullLabel => parent.Label;

    public GemSize GetGemSize() {
        if (gemstoneSize == 1) return GemSize.Chip;
        if (gemstoneSize == 5) return GemSize.Mark;
        if (gemstoneSize == 20) return GemSize.Broam;
        return GemSize.None;
    }

    public override void Initialize(CompProperties props) {
        base.Initialize(props);
        stormlight = parent.GetComp<Stormlight>();
        List<int> sizeList = new List<int> { 1, 5, 20 };
        sizeList.RemoveAll(n => n > maximumGemstoneSize);
        gemstoneQuality = StormlightUtilities.RollTheDice(1, 5);
        gemstoneSize =
            StormlightUtilities
                .RollForRandomIntFromList(sizeList); //make better roller later with lower prob for bigger size

        //parent.def.BaseMarketValue = (parent.def.BaseMarketValue * gemstoneSize) + gemstoneQuality * 5;

        if (stormlight != null) {
            stormlight.stormlightContainerSize = gemstoneSize;
            stormlight.CalculateMaximumGlowRadius(gemstoneQuality, gemstoneSize);
        }
    }

    public override bool AllowStackWith(Verse.Thing other) {
        CompCutGemstone comp = other.TryGetComp<CompCutGemstone>();
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
        Scribe_Values.Look(ref capturedSpren, "capturedSpren");
    }

    public override void CompTick() {
        base.CompTick();
    }


    public override string CompInspectStringExtra() {
        if (capturedSpren != Spren.None) {
            return $"holding: {capturedSpren.ToString()}spren";
        }

        return "";
    }

    public override IEnumerable<Gizmo> CompGetGizmosExtra() {
        if (CosmereRoshar.devOptionAutofillSpheres && stormlight != null) {
            yield return new Command_Action {
                defaultLabel = "Fill gem with 10 stormlight",
                defaultDesc = "Debug/Dev feature.",
                icon = TexCommand.DesirePower,
                action = () => { stormlight.InfuseStormlight(10f); },
            };

            yield return new Command_Action {
                defaultLabel = "Fill gem with 100 stormlight",
                defaultDesc = "Debug/Dev feature.",
                icon = TexCommand.DesirePower,
                action = () => { stormlight.InfuseStormlight(100f); },
            };

            yield return new Command_Action {
                defaultLabel = "Fill gem with maximum stormlight",
                defaultDesc = "Debug/Dev feature.",
                icon = TexCommand.DesirePower,
                action = () => { stormlight.InfuseStormlight(stormlight.currentMaxStormlight); },
            };

            yield return new Command_Action {
                defaultLabel = "remvoe all light",
                defaultDesc = "Debug/Dev feature.",
                icon = TexCommand.CannotShoot,
                action = () => { stormlight.RemoveAllStormlight(); },
            };

            yield return new Command_Action {
                defaultLabel = "Set Quality",
                defaultDesc = "Debug/Dev feature.",
                icon = TexCommand.DesirePower,
                action = () => {
                    gemstoneQuality = gemstoneQuality % 5 + 1;
                    // stormlightComp.StormlightContainerQuality = gemstoneQuality;
                    stormlight.stormlightContainerSize = gemstoneSize;
                    stormlight.CalculateMaximumGlowRadius(gemstoneQuality, gemstoneSize);
                },
            };
            yield return new Command_Action {
                defaultLabel = "Set size",
                defaultDesc = "Debug/Dev feature.",
                icon = TexCommand.DesirePower,
                action = () => {
                    if (gemstoneSize == 1) {
                        gemstoneSize = 5;
                    } else if (gemstoneSize == 5) {
                        gemstoneSize = 20;
                    } else if (gemstoneSize == 20) {
                        gemstoneSize = 1;
                    }

                    // stormlightComp.StormlightContainerQuality = gemstoneQuality;
                    stormlight.stormlightContainerSize = gemstoneSize;
                    stormlight.CalculateMaximumGlowRadius(gemstoneQuality, gemstoneSize);
                },
            };

            yield return new Command_Action {
                defaultLabel = "Set captured spren",
                defaultDesc = "Debug/Dev feature.",
                icon = TexCommand.SelectShelf,
                action = () => {
                    if (capturedSpren == Spren.Logic) {
                        capturedSpren = Spren.None;
                    } else {
                        capturedSpren += 1;
                    }
                },
            };
        }

        yield break;
    }
}

public class CompPropertiesCutGemstone : CompProperties {
    public CompPropertiesCutGemstone() {
        compClass = typeof(CompCutGemstone);
    }
}