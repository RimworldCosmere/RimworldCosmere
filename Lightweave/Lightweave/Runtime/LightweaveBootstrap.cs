using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using UnityEngine;
using Verse;

namespace Cosmere.Lightweave.Runtime;

[StaticConstructorOnStartup]
public static class LightweaveBootstrap {
    static LightweaveBootstrap() {
        RegisterPrimitiveDefaults();
    }

    public static void Ensure() { }

    private static void RegisterPrimitiveDefaults() {
        ThemeClassRegistry.Register("text", new Style {
            FontFamily = FontRole.Body,
            FontSize = new Rem(1f),
            FontWeight = FontStyle.Normal,
            TextColor = ThemeSlot.TextPrimary,
            TextAlign = TextAlign.Start,
        });
        ThemeClassRegistry.Register("caption", new Style {
            FontFamily = FontRole.Caption,
            FontSize = new Rem(0.75f),
            TextColor = ThemeSlot.TextMuted,
        });
        ThemeClassRegistry.Register("label", new Style {
            FontFamily = FontRole.Label,
            FontSize = new Rem(0.875f),
            TextColor = ThemeSlot.TextSecondary,
        });
        ThemeClassRegistry.Register("heading", new Style {
            FontFamily = FontRole.Heading,
            FontWeight = FontStyle.Bold,
        });
        ThemeClassRegistry.Register("h1", new Style { FontSize = new Rem(2f) });
        ThemeClassRegistry.Register("h2", new Style { FontSize = new Rem(1.5f) });
        ThemeClassRegistry.Register("h3", new Style { FontSize = new Rem(1.25f) });
        ThemeClassRegistry.Register("h4", new Style { FontSize = new Rem(1.125f) });
        ThemeClassRegistry.Register("eyebrow", new Style {
            FontFamily = FontRole.Body,
            FontSize = new Rem(1f),
            FontWeight = FontStyle.Normal,
            TextColor = ThemeSlot.TextMuted,
        });
        ThemeClassRegistry.Register("display", new Style {
            FontFamily = FontRole.Display,
            FontWeight = FontStyle.Bold,
            TextColor = ThemeSlot.TextPrimary,
        });
        ThemeClassRegistry.Register("display-1", new Style { FontSize = new Rem(4.0f) });
        ThemeClassRegistry.Register("display-2", new Style { FontSize = new Rem(3.0f) });
        ThemeClassRegistry.Register("display-3", new Style { FontSize = new Rem(2.25f) });
        ThemeClassRegistry.Register("display-4", new Style { FontSize = new Rem(1.75f) });
        ThemeClassRegistry.Register("code-inline", new Style {
            FontFamily = FontRole.Mono,
            FontSize = new Rem(0.875f),
            TextColor = ThemeSlot.TextPrimary,
        });
        ThemeClassRegistry.Register("hotkey-badge", new Style {
            FontFamily = FontRole.Mono,
            FontSize = new Rem(0.6875f),
            TextColor = ThemeSlot.TextSecondary,
        });
        ThemeClassRegistry.Register("rich-text", new Style {
            FontFamily = FontRole.Body,
            FontSize = new Rem(1f),
            TextColor = ThemeSlot.TextPrimary,
        });
        ThemeClassRegistry.Register("icon", new Style {
            TextColor = Color.white,
        });
        ThemeClassRegistry.Register("spacer", new Style());
        ThemeClassRegistry.Register("divider", new Style {
            Background = BackgroundSpec.Of(ThemeSlot.BorderSubtle),
        });
    }
}
