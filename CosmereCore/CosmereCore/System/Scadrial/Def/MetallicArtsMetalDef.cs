using Cosmere.Core.Def;
using UnityEngine;
using Verse;

namespace Cosmere.System.Scadrial.Def;

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

    /// <summary>
    ///     Whether swallowing this metal does anything at all.
    /// </summary>
    /// <remarks>
    ///     A god metal with neither Allomancy nor Feruchemy has no effect to give. Harmonium and
    ///     trellium are both like that - harmonium in particular reacts with the water in a body
    ///     and is not something anyone survives eating, which is why it carries no power to
    ///     grant. Offering it as food promised a transformation that never came.
    /// </remarks>
    public bool CanBeIngested => allomancy != null || feruchemy != null;

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
    public string description = string.Empty;
    public AllomancyGroup? group;
    public Texture2D icon = null!;
    public Texture2D invertedIcon = null!;
    public AllomancyPolarity? polarity;
    public string? userName;
}

public class MetalFeruchemyDef {
    public string description = string.Empty;
    public FeruchemyGroup? group;
    public Texture2D icon = null!;
    public Texture2D invertedIcon = null!;

    // Scales charge moved per second. One figure, not one per direction: a metalmind
    // hands back exactly what went in, so storing and tapping have to run at the same
    // rate. Bendalloy sits far below 1 so a single band covers most of a week.
    public float rateMultiplier = 1f;
    public string? userName;
}
