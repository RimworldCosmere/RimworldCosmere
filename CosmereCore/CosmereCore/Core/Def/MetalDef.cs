using UnityEngine;
using Verse;

namespace Cosmere.Core.Def;

public class MetalDef : Verse.Def {
    private Material? cachedSolidLineColor;
    private Material? cachedSolidLineColorTwo;

    private Material? cachedTransparentLineColor;
    private Material? cachedTransparentLineColorTwo;
    public Color color;
    public Color? colorTwo;
    public bool godMetal = false;

    /// <summary>
    ///     The Shard this metal is a piece of. Set on god metals only; ordinary metals belong to
    ///     nobody and are not gated on Connection.
    /// </summary>
    public ShardDef? shard;

    public Material transparentLineColor => cachedTransparentLineColor ??=
        MaterialPool.MatFrom(GenDraw.LineTexPath, Verse.ShaderDatabase.Transparent, color);

    public Material? transparentLineColorTwo => cachedTransparentLineColorTwo ??=
        colorTwo is null
            ? null
            : MaterialPool.MatFrom(GenDraw.LineTexPath, Verse.ShaderDatabase.Transparent, colorTwo.Value);

    public Material solidLineColor => cachedSolidLineColor ??=
        MaterialPool.MatFrom(GenDraw.LineTexPath, Verse.ShaderDatabase.SolidColorBehind, color);

    public Material? solidLineColorTwo => cachedSolidLineColorTwo ??=
        colorTwo is null
            ? null
            : MaterialPool.MatFrom(GenDraw.LineTexPath, Verse.ShaderDatabase.SolidColorBehind, colorTwo.Value);

    public string coloredLabel => label.Colorize(ColoredText.DateTimeColor);

    public ThingDef Item => DefDatabase<ThingDef>.GetNamed(defName);
}
