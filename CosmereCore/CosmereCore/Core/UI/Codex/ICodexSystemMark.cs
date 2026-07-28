using UnityEngine;
using Verse;

namespace Cosmere.Core.UI.Codex;

/// <summary>
///     Implemented by a codex content provider whose mark in the system rail depends on the pawn
///     rather than being fixed for the whole art - a Radiant's order, for instance, where the glyph
///     and colour belong to the order and not to Surgebinding as a whole.
/// </summary>
/// <remarks>
///     Opt-in on purpose: an art whose mark never varies has nothing to say here and should keep
///     using its <see cref="Skin.ISystemSkin" />. Returning <see langword="null" /> from either
///     member falls back to the skin for that piece.
/// </remarks>
public interface ICodexSystemMark {
    /// <summary>
    ///     The glyph to draw for this pawn. Unlike a system sigil, this carries its own colours, so
    ///     the rail draws it untinted - which means the implementer, not the rail, has to supply the
    ///     right artwork for each state. A tint cannot do it: GUI.color multiplies, so a glyph that is
    ///     already coloured can only be made darker, never white.
    /// </summary>
    Texture2D? SigilFor(Pawn pawn, bool selected);

    /// <summary>The colour that stands for this pawn's branch of the art.</summary>
    Color? AccentFor(Pawn pawn);
}
