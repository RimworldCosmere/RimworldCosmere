using System;
using UnityEngine;
using Verse;

namespace Cosmere.Core.UI.Skin;

public sealed class DataSystemSkin : ISystemSkin {
    private readonly string headerLabelKey;
    private readonly Func<Texture2D?>? sigilSource;
    private Texture2D? cachedSigil;

    public DataSystemSkin(
        string systemId,
        string headerLabelKey,
        Color accentColor,
        Color barFillColor,
        Color barBackgroundColor,
        Color headerTextColor,
        Color panelBackgroundColor,
        Color borderTintColor,
        GameFont headerFont = GameFont.Small,
        Func<Texture2D?>? sigil = null
    ) {
        SystemId = systemId;
        this.headerLabelKey = headerLabelKey;
        AccentColor = accentColor;
        BarFillColor = barFillColor;
        BarBackgroundColor = barBackgroundColor;
        HeaderTextColor = headerTextColor;
        PanelBackgroundColor = panelBackgroundColor;
        BorderTintColor = borderTintColor;
        HeaderFont = headerFont;
        sigilSource = sigil;
    }

    public string SystemId { get; }

    public string HeaderLabel => headerLabelKey.Translate();

    public Color AccentColor { get; }

    public Color BarFillColor { get; }

    public Color BarBackgroundColor { get; }

    public Color HeaderTextColor { get; }

    public GameFont HeaderFont { get; }

    public Color PanelBackgroundColor { get; }

    public Color BorderTintColor { get; }

    public Texture2D? Sigil => cachedSigil ??= sigilSource?.Invoke();

    public Color? GetColor(ThemeSlot slot) {
        return slot switch {
            ThemeSlot.SurfaceAccent => AccentColor,
            ThemeSlot.TextOnAccent => HeaderTextColor,
            _ => null,
        };
    }

    public Font? GetFont(FontRole role) {
        return null;
    }
}
