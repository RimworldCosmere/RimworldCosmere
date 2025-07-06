using CosmereFramework.Extension;
using CosmereResources.Def;
using UnityEngine;
using Verse;

namespace CosmereScadrial.Def;

public enum AllomancyAxis {
    None,
    Internal,
    External,
}

public enum AllomancyGroup {
    None,
    Physical,
    Mental,
    Enhancement,
    Temporal,
}

public enum AllomancyPolarity {
    None,
    Pushing,
    Pulling,
}

public enum FeruchemyGroup {
    None,
    Physical,
    Cognitive,
    Spiritual,
    Hybrid,
}

public class MetallicArtsMetalDef : MetalDef {
    public MetalAllomancyDef? allomancy;
    public MetalFeruchemyDef? feruchemy;
    public Texture2D? invertedIcon;
    public Texture2D? uiIcon;

    public static MetallicArtsMetalDef FromMetalDef(MetalDef def) {
        if (def is MetallicArtsMetalDef metallicArtsMetalDef) return metallicArtsMetalDef;

        return DefDatabase<MetallicArtsMetalDef>.GetNamed(def.defName);
    }

    public override void PostLoad() {
        LongEventHandler.ExecuteWhenFinished(() => {
                if (allomancy != null) {
                    allomancy.icon = ContentFinder<Texture2D>.Get(
                        $"UI/Icons/Genes/Investiture/Allomancy/{defName}",
                        false
                    );
                    if (allomancy.icon != null) {
                        allomancy.invertedIcon = allomancy.icon.CloneTexture().InvertColors();
                    }
                }

                if (feruchemy != null) {
                    feruchemy.icon = ContentFinder<Texture2D>.Get(
                        $"UI/Icons/Genes/Investiture/Feruchemy/{defName}",
                        false
                    );
                    if (feruchemy.icon != null) {
                        feruchemy.invertedIcon = feruchemy.icon.CloneTexture().InvertColors();
                    }
                }

                uiIcon = allomancy?.icon ?? feruchemy?.icon;
                invertedIcon = allomancy?.invertedIcon ?? feruchemy?.invertedIcon;
            }
        );
    }
}

public class MetalAllomancyDef {
    public AllomancyAxis? axis;
    public string description = "";
    public AllomancyGroup? group;
    public Texture2D icon = null!;
    public Texture2D invertedIcon = null!;
    public AllomancyPolarity? polarity;
    public string? userName;
}

public class MetalFeruchemyDef {
    public string description = "";
    public FeruchemyGroup? group;
    public Texture2D icon = null!;
    public Texture2D invertedIcon = null!;
    public string? userName;
}