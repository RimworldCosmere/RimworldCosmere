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
    public Texture2D invertedIcon;
    public Texture2D uiIcon;

    public static MetallicArtsMetalDef FromMetalDef(MetalDef def) {
        if (def is MetallicArtsMetalDef metallicArtsMetalDef) return metallicArtsMetalDef;

        return DefDatabase<MetallicArtsMetalDef>.GetNamed(def.defName);
    }

    public override void PostLoad() {
        LongEventHandler.ExecuteWhenFinished(() => {
            uiIcon = ContentFinder<Texture2D>.Get($"UI/Icons/Genes/Investiture/Allomancy/{defName}", false);
            if (uiIcon == null) return;

            invertedIcon = uiIcon.CloneTexture().InvertColors();
        });
    }
}

public class MetalAllomancyDef {
    public AllomancyAxis? axis;
    public string description;
    public AllomancyGroup? group;
    public AllomancyPolarity? polarity;
    public string? userName;
}

public class MetalFeruchemyDef {
    public string description;
    public FeruchemyGroup? group;
    public string? userName;
}