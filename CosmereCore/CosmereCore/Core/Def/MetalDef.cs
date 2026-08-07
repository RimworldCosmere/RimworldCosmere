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
    ///     The Shards this metal is made of, and what burning it grants to each.
    /// </summary>
    /// <remarks>
    ///     A list because god metals alloy with each other, and per-Shard grants because a metal
    ///     does not tie you equally to everything in it. Lerasium makes a Mistborn, which is
    ///     Preservation's gift, but a Mistborn burns atium too - so it grants deeply to
    ///     Preservation and just enough to Ruin to reach for it.
    /// </remarks>
    public List<ShardGrant> shards = [];

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

/// <summary>One Shard a metal is made of, and how strongly burning it ties you there.</summary>
public class ShardGrant {
    public ShardDef shard = null!;

    /// <summary>0-100. Zero means the metal spends a Connection rather than giving one.</summary>
    public int grant;
}
