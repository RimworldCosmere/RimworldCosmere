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
    ///     The Shards this metal is made of. God metals only; ordinary metals belong to nobody
    ///     and are not gated on Connection.
    /// </summary>
    /// <remarks>
    ///     A list because god metals alloy with each other. Leratium is lerasium and atium
    ///     together, so it is a piece of Preservation and of Ruin both, and burning it ties the
    ///     drinker to each.
    /// </remarks>
    public List<ShardDef> shards = [];

    /// <summary>
    ///     How much Connection burning this metal grants to each of its Shards, on the 0-100
    ///     scale. Zero for a metal that only spends a Connection you already had.
    /// </summary>
    /// <remarks>
    ///     Lerasium and everything alloyed with it are the way in: they take an unconnected
    ///     person and make them Invested. A metal that grants is therefore never gated on
    ///     Connection - that would deny it to exactly the people it exists for.
    /// </remarks>
    public int connectionGrant;

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
