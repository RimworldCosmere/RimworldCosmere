using UnityEngine;
using Verse;

namespace CosmereResources.Def;

public class MetalDef : Verse.Def {
    private Material? cachedSolidLineColor;
    private Material? cachedSolidLineColorTwo;

    private Material? cachedTransparentLineColor;
    private Material? cachedTransparentLineColorTwo;
    public Color color;
    public Color? colorTwo;
    public bool godMetal = false;

    public Material transparentLineColor => cachedTransparentLineColor ??=
        MaterialPool.MatFrom(GenDraw.LineTexPath, ShaderDatabase.Transparent, color);

    public Material? transparentLineColorTwo => cachedTransparentLineColorTwo ??=
        colorTwo is null
            ? null
            : MaterialPool.MatFrom(GenDraw.LineTexPath, ShaderDatabase.Transparent, colorTwo.Value);

    public Material solidLineColor => cachedSolidLineColor ??=
        MaterialPool.MatFrom(GenDraw.LineTexPath, ShaderDatabase.SolidColorBehind, color);

    public Material? solidLineColorTwo => cachedSolidLineColorTwo ??=
        colorTwo is null
            ? null
            : MaterialPool.MatFrom(GenDraw.LineTexPath, ShaderDatabase.SolidColorBehind, colorTwo.Value);

    public string coloredLabel => label.Colorize(ColoredText.DateTimeColor);

    public ThingDef Item => DefDatabase<ThingDef>.GetNamed(defName);
}