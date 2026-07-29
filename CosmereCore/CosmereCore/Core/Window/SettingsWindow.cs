using Cosmere.Core.Settings;
using Cosmere.Core.Settings.Model;
using Cosmere.Core.UI;
using Cosmere.Core.UI.Skin;
using UnityEngine;
using Verse;

namespace Cosmere.Core.Window;

public sealed class SettingsWindow {
    private readonly SettingsFooterRenderer footerRenderer = new SettingsFooterRenderer();
    private readonly SettingsNavigationState navigation = new SettingsNavigationState();
    private readonly Dictionary<string, IReadOnlyList<SettingSection>> sectionsBySystem = [];
    private readonly List<CosmereModSettings> systems;

    private CosmereModSettings selectedSystem;
    private string searchText = string.Empty;

    public SettingsWindow(List<CosmereModSettings> allModSettings) {
        systems = new List<CosmereModSettings>(allModSettings);
        systems.Sort(CompareSystems);

        for (int i = 0; i < systems.Count; i++) {
            CosmereModSettings system = systems[i];
            sectionsBySystem.Add(system.Name, system.BuildSections());
        }

        selectedSystem = systems[0];
        navigation.SelectSystem(selectedSystem.Name);
    }

    public bool CloseRequested { get; private set; }

    public void Draw(Rect inRect) {
        SettingsWindowLayout layout = SettingsWindowLayout.Create(inRect);
        ISystemSkin skin = SystemSkinRegistry.ForOrFallback(selectedSystem.SkinId);
        IReadOnlyList<SettingSection> sections = sectionsBySystem[selectedSystem.Name];

        CosmereModSettings? requestedSystem = SettingsSidebarRenderer.Draw(layout.Sidebar, systems, selectedSystem, ref searchText);
        if (requestedSystem != null && requestedSystem != selectedSystem) {
            SelectSystem(requestedSystem);
            skin = SystemSkinRegistry.ForOrFallback(selectedSystem.SkinId);
            sections = sectionsBySystem[selectedSystem.Name];
        }

        SettingsHeaderRenderer.DrawCrest(layout.Crest, skin);
        string? selectedSection = SettingsHeaderRenderer.DrawSectionRail(
            layout.SectionRail,
            skin,
            sections,
            navigation.ScrollTargetSectionKey
        );
        if (selectedSection != null) {
            navigation.FlashSection(selectedSystem.Name, selectedSection);
        }

        DrawContentShell(layout.ContentViewport, skin, sections);
        footerRenderer.Draw(
            layout.Footer,
            selectedSystem.Name,
            selectedSystem.Name,
            sections,
            skin,
            RequestClose
        );
    }

    public bool ConsumeCloseRequest() {
        if (!CloseRequested) return false;

        CloseRequested = false;
        return true;
    }

    public void CancelResetConfirmation() {
        footerRenderer.CancelConfirmation();
    }

    private void SelectSystem(CosmereModSettings system) {
        selectedSystem = system;
        navigation.SelectSystem(system.Name);
        footerRenderer.CancelConfirmation();
    }

    private static int CompareSystems(CosmereModSettings left, CosmereModSettings right) {
        if (left.Name == "Core") return right.Name == "Core" ? 0 : -1;
        if (right.Name == "Core") return 1;

        return string.Compare(left.Name, right.Name, global::System.StringComparison.Ordinal);
    }

    private void DrawContentShell(Rect rect, ISystemSkin skin, IReadOnlyList<SettingSection> sections) {
        Vector2 scroll = navigation.GetScroll(selectedSystem.Name);
        Rect viewRect = new Rect(0f, 0f, rect.width, rect.height);
        Widgets.BeginScrollView(rect, ref scroll, viewRect, false);
        try {
            DrawContentShellBody(viewRect, skin, sections);

            if (navigation.TryGetFlash(IsReducedMotionEnabled(), out SettingsNavigationState.FlashState flash, out float alpha) &&
                flash.SystemKey == selectedSystem.Name) {
                Widgets.DrawBoxSolid(viewRect, new Color(skin.AccentColor.r, skin.AccentColor.g, skin.AccentColor.b, alpha * 0.14f));
            }
        } finally {
            Widgets.EndScrollView();
            navigation.SetScroll(selectedSystem.Name, scroll);
        }
    }

    private static void DrawContentShellBody(Rect rect, ISystemSkin skin, IReadOnlyList<SettingSection> sections) {
        Widgets.DrawBoxSolid(
            rect,
            new Color(skin.PanelBackgroundColor.r, skin.PanelBackgroundColor.g, skin.PanelBackgroundColor.b, 0.30f)
        );
        if (HasVisibleSections(sections)) return;

        UIText.EllipsisLabel(
            rect.ContractedBy(SettingsWindowLayout.ContentPadding),
            (string)"CC_Settings_System_Empty".Translate(),
            GameFont.Small,
            TextAnchor.MiddleCenter,
            skin.HeaderTextColor
        );
    }

    private static bool IsReducedMotionEnabled() {
        return Mod.GetModSettings<CoreModSettings>().reduceMotion;
    }

    private static bool HasVisibleSections(IReadOnlyList<SettingSection> sections) {
        for (int i = 0; i < sections.Count; i++) {
            if (sections[i].IsVisible) return true;
        }

        return false;
    }

    private void RequestClose() {
        CloseRequested = true;
        footerRenderer.CancelConfirmation();
    }
}
