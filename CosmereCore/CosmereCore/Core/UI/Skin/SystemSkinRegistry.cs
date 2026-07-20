using Cosmere.Core.Settings;
using UnityEngine;
using Verse;

namespace Cosmere.Core.UI.Skin;

public static class SystemSkinRegistry {
    private static readonly List<ISystemSkin> skins = [];

    public static void Register(ISystemSkin skin) {
        for (int i = 0; i < skins.Count; i++) {
            if (skins[i].SystemId == skin.SystemId) return;
        }

        skins.Add(skin);
    }

    public static ISystemSkin ForOrFallback(string systemId) {
        ISystemSkin resolved = Resolve(systemId);
        CoreModSettings settings = Mod.GetModSettings<CoreModSettings>();
        if (settings.highContrast) {
            return new HighContrastSkinDecorator(resolved);
        }

        return resolved;
    }

    private static ISystemSkin Resolve(string systemId) {
        for (int i = 0; i < skins.Count; i++) {
            if (skins[i].SystemId == systemId) return skins[i];
        }

        return new FallbackSkin(systemId);
    }

    private sealed class FallbackSkin : ISystemSkin {
        public FallbackSkin(string systemId) {
            SystemId = systemId;
        }

        public string SystemId { get; }

        public string HeaderLabel => SystemId;
        public Color AccentColor => new Color(0.7f, 0.7f, 0.7f);
        public Color BarFillColor => new Color(0.55f, 0.55f, 0.55f);
        public Color BarBackgroundColor => new Color(0.12f, 0.12f, 0.12f);
        public Color HeaderTextColor => Color.white;
        public GameFont HeaderFont => GameFont.Small;
        public SkinTypography Typography => SkinTypography.Empty;
        public Color PanelBackgroundColor => new Color(0.05f, 0.05f, 0.08f, 0.75f);
        public Color BorderTintColor => new Color(0.35f, 0.35f, 0.4f);
        public Texture2D? Sigil => null;
        public Texture2D? BorderFrame => null;

        public Color? GetColor(ThemeSlot slot) {
            return null;
        }

        public Font? GetFont(FontRole role) {
            return null;
        }

        public Font? DisplayFont => null;
    }
}