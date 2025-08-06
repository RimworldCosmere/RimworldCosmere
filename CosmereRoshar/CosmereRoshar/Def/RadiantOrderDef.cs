using System;
using Cosmere.Resources.Def;
using Cosmere.Roshar.Surgebinding.IdealChecker;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.Roshar.Def;

public class Ideal {
    public List<AbilityDef> abilities;
    public string label;
    public List<string> quotes;
}

public class RadiantOrderDef : Verse.Def {
    public List<AbilityDef> abilities;
    public Texture2D bannerIcon;
    public Color color;
    public GemDef gemstone;
    public Texture2D icon;

    public AbstractIdealChecker idealChecker;
    public List<Ideal> ideals;
    public Texture2D invertedIcon;
    public List<SurgeDef> surges;

    private Type idealCheckerClass =>
        typeof(RadiantOrderDef).Assembly.GetType("Cosmere.Roshar.Surgebinding.IdealChecker." + defName);

    public GeneDef GetSurgebindingGene() {
        return DefDatabase<GeneDef>.GetNamed("Cosmere_Roshar_Gene_Radiant" + defName);
    }

    public override void PostLoad() {
        base.PostLoad();

        idealChecker = (AbstractIdealChecker)Activator.CreateInstance(idealCheckerClass, this);
        LongEventHandler.ExecuteWhenFinished(() => {
                icon = ContentFinder<Texture2D>.Get($"UI/Surgebinding/Order/{defName}");
                bannerIcon = ContentFinder<Texture2D>.Get($"UI/Surgebinding/Order/Banner{defName}");
                if (icon != null) {
                    invertedIcon = icon.CloneTexture().InvertColors();
                }
            }
        );
    }
}