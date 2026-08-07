using Cosmere.Core.ShardConnection;
using UnityEngine;
using Verse;

namespace Cosmere.Core.UI.Codex;

/// <summary>
///     Colours for the Connection subtab.
/// </summary>
/// <remarks>
///     Shards belong to the cosmere rather than to any one shardworld, so this surface takes no
///     world accent - a Scadrial save must not paint Honor steel-blue. The ground stays neutral
///     parchment and only the tier carries colour, which makes the eight bars comparable at a
///     glance instead of a spectrum you have to decode.
/// </remarks>
public static class ConnectionPalette {
    /// <summary>The rail orb when Connection is the chosen section.</summary>
    public static readonly Color Selected = new Color(0.85f, 0.78f, 0.58f);

    public static readonly Color Ground = new Color(0.16f, 0.15f, 0.14f, 0.55f);
    public static readonly Color Track = new Color(0.10f, 0.10f, 0.10f, 0.70f);
    public static readonly Color Divider = new Color(0.55f, 0.50f, 0.42f, 0.35f);
    public static readonly Color GroupLabel = new Color(0.72f, 0.66f, 0.55f);

    /// <summary>A Shard the scenario switched off. Present, legible, obviously not in play.</summary>
    public static readonly Color Disabled = new Color(0.42f, 0.40f, 0.38f);

    private static readonly Color none = new Color(0.38f, 0.36f, 0.34f);
    private static readonly Color touched = new Color(0.55f, 0.52f, 0.42f);
    private static readonly Color bonded = new Color(0.72f, 0.63f, 0.38f);
    private static readonly Color invested = new Color(0.85f, 0.74f, 0.42f);
    private static readonly Color ascendant = new Color(0.96f, 0.90f, 0.68f);

    /// <summary>
    ///     Tier colour. Deliberately one hue getting brighter rather than five different hues:
    ///     the bands are ordered, so the colour should read as ordered too.
    /// </summary>
    public static Color ForTier(ConnectionTier tier) {
        return tier switch {
            ConnectionTier.Ascendant => ascendant,
            ConnectionTier.Invested => invested,
            ConnectionTier.Bonded => bonded,
            ConnectionTier.Touched => touched,
            _ => none,
        };
    }

    public static string LabelKeyFor(ConnectionTier tier) {
        return tier switch {
            ConnectionTier.Ascendant => "CC_Connection_Tier_Ascendant",
            ConnectionTier.Invested => "CC_Connection_Tier_Invested",
            ConnectionTier.Bonded => "CC_Connection_Tier_Bonded",
            ConnectionTier.Touched => "CC_Connection_Tier_Touched",
            _ => "CC_Connection_Tier_None",
        };
    }
}

/// <summary>
///     The Connection sigil, looked up once at startup.
/// </summary>
/// <remarks>
///     ContentFinder must never be called from a draw path - it is a dictionary lookup plus a
///     load on miss, and the rail redraws every frame the tab is open.
///     <para>
///         Flat white on transparent, because SystemSwitcherStrip tints it with GUI.color to
///         show selection. Any colour baked into the texture would be multiplied by the accent
///         and come out muddy.
///     </para>
/// </remarks>
[StaticConstructorOnStartup]
public static class ConnectionTextures {
    public static readonly Texture2D? Sigil =
        ContentFinder<Texture2D>.Get("UI/Icons/Connection", false);
}
