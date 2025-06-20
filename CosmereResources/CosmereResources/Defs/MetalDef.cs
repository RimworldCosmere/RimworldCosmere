using UnityEngine;
using Verse;

namespace CosmereResources.Defs;

public class MetalDef : Def {
    private Material? cachedSolidLineColor;
    private Material? cachedSolidLineColorTwo;

    private Material? cachedTransparentLineColor;
    private Material? cachedTransparentLineColorTwo;
    public Color color;
    public Color? colorTwo;
    public bool godMetal = false;

    public Material TransparentLineColor => cachedTransparentLineColor ??=
        MaterialPool.MatFrom(GenDraw.LineTexPath, ShaderDatabase.Transparent, color);

    public Material? TransparentLineColorTwo => cachedTransparentLineColorTwo ??=
        colorTwo is null
            ? null
            : MaterialPool.MatFrom(GenDraw.LineTexPath, ShaderDatabase.Transparent, colorTwo.Value);

    public Material SolidLineColor => cachedSolidLineColor ??=
        MaterialPool.MatFrom(GenDraw.LineTexPath, ShaderDatabase.SolidColorBehind, color);

    public Material? SolidLineColorTwo => cachedSolidLineColorTwo ??=
        colorTwo is null
            ? null
            : MaterialPool.MatFrom(GenDraw.LineTexPath, ShaderDatabase.SolidColorBehind, colorTwo.Value);
}