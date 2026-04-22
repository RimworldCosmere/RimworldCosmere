using Cosmere.Core.Settings;
using UnityEngine;
using Verse;

namespace Cosmere.Core.UI.Skin;

public static class SystemSkinRegistry {
    private static readonly List<ISystemSkin> skins = [];

    private sealed class FallbackSkin : ISystemSkin {
        private readonly string systemId;
        public FallbackSkin(string systemId) { this.systemId = systemId; }
        public string SystemId => systemId;
        public string HeaderLabel => systemId;
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
    }

    public static void Register(ISystemSkin skin) {
        for (int i = 0; i < skins.Count; i++) {
            if (skins[i].SystemId == skin.SystemId) return;
        }
        skins.Add(skin);
    }

    public static ISystemSkin For(string systemId) {
        ISystemSkin resolved = ResolveRaw(systemId);
        CoreModSettings settings = Cosmere.Core.Mod.GetModSettings<CoreModSettings>();
        if (settings.highContrast) {
            return new HighContrastSkinDecorator(resolved);
        }
        return resolved;
    }

    public static ISystemSkin Raw(string systemId) => ResolveRaw(systemId);

    private static ISystemSkin ResolveRaw(string systemId) {
        for (int i = 0; i < skins.Count; i++) {
            if (skins[i].SystemId == systemId) return skins[i];
        }
        return new FallbackSkin(systemId);
    }
}
