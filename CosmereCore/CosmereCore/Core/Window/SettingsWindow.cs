using Cosmere.Core.Settings;
using Cosmere.Core.Settings.Model;
using Cosmere.Core.Settings.Search;
using Cosmere.Core.UI;
using Cosmere.Core.UI.Skin;
using UnityEngine;
using Verse;

namespace Cosmere.Core.Window;

public sealed class SettingsWindow {
    private readonly SettingsContentRenderer contentRenderer = new SettingsContentRenderer();
    private readonly SettingsFooterRenderer footerRenderer = new SettingsFooterRenderer();
    private readonly SettingsNavigationState navigation = new SettingsNavigationState();
    private readonly SettingsSearchResultsRenderer searchResultsRenderer = new SettingsSearchResultsRenderer();
    private readonly Dictionary<string, IReadOnlyList<SettingSection>> sectionsBySystem = [];
    private readonly List<CosmereModSettings> systems;

    private readonly Dictionary<string, string> selectedSectionBySystem = [];
    private readonly List<SettingSection> visibleSections = [];

    private readonly SettingsSearchIndex searchIndex;

    private CosmereModSettings selectedSystem;
    private string searchText = string.Empty;

    public SettingsWindow(List<CosmereModSettings> allModSettings) {
        systems = new List<CosmereModSettings>(allModSettings);
        systems.Sort(CompareSystems);

        for (int i = 0; i < systems.Count; i++) {
            CosmereModSettings system = systems[i];
            sectionsBySystem.Add(system.Name, system.BuildSections());
        }

        searchIndex = new SettingsSearchIndex(BuildSearchDocuments());

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

        if (SettingsHeaderRenderer.DrawCrest(layout.Crest, skin, selectedSystem.DisplayLabel)) RequestClose();

        if (!searchText.NullOrEmpty()) {
            DrawSearchResults(layout.ContentViewport, skin);
            footerRenderer.Draw(layout.Footer, selectedSystem.Name, selectedSystem.Name, sections, skin, RequestClose);
            return;
        }

        string activeSectionKey = ActiveSectionKey(sections);
        string? requestedSection = SettingsHeaderRenderer.DrawSectionTabs(
            layout.SectionRail,
            skin,
            sections,
            activeSectionKey
        );
        if (requestedSection != null && requestedSection != activeSectionKey) {
            selectedSectionBySystem[selectedSystem.Name] = requestedSection;
            activeSectionKey = requestedSection;
            navigation.SetScroll(selectedSystem.Name, Vector2.zero);
        }

        DrawContentShell(layout.ContentViewport, layout.Content, skin, SectionsFor(sections, activeSectionKey));
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

    /// <summary>
    ///     Tabs paginate, so the pane draws exactly one section. Falls back to the first visible
    ///     section when the remembered one is gone, e.g. a dev-only section whose visibility turned false.
    /// </summary>
    private string ActiveSectionKey(IReadOnlyList<SettingSection> sections) {
        string? remembered = selectedSectionBySystem.TryGetValue(selectedSystem.Name, out string? key) ? key : null;
        string? firstVisible = null;
        for (int i = 0; i < sections.Count; i++) {
            SettingSection section = sections[i];
            if (!section.IsVisible) continue;

            firstVisible ??= section.Key;
            if (section.Key == remembered) return remembered;
        }

        return firstVisible ?? string.Empty;
    }

    private IReadOnlyList<SettingSection> SectionsFor(IReadOnlyList<SettingSection> sections, string sectionKey) {
        visibleSections.Clear();
        for (int i = 0; i < sections.Count; i++) {
            SettingSection section = sections[i];
            if (section.Key == sectionKey && section.IsVisible) visibleSections.Add(section);
        }

        return visibleSections;
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

    private void DrawContentShell(
        Rect viewportRect,
        Rect contentRect,
        ISystemSkin skin,
        IReadOnlyList<SettingSection> sections
    ) {
        contentRenderer.Measure(selectedSystem.Name, sections, contentRect.width);
        Vector2 scroll = navigation.GetScroll(selectedSystem.Name);
        if (navigation.TryConsumeScrollTargetSection(out string? sectionKey) &&
            sectionKey != null &&
            contentRenderer.TryGetSectionOffset(sectionKey, out float sectionOffset)) {
            float maxScroll = Mathf.Max(0f, contentRenderer.ContentHeight - viewportRect.height);
            scroll.y = Mathf.Clamp(sectionOffset - SettingsWindowLayout.ContentPadding, 0f, maxScroll);
        }

        Rect viewRect = new Rect(0f, 0f, contentRect.width, Mathf.Max(viewportRect.height, contentRenderer.ContentHeight));
        Widgets.BeginScrollView(viewportRect, ref scroll, viewRect);
        try {
            contentRenderer.Draw(
                viewRect,
                new Rect(0f, scroll.y, contentRect.width, viewportRect.height),
                selectedSystem.Name,
                skin,
                navigation,
                IsReducedMotionEnabled()
            );
        } finally {
            Widgets.EndScrollView();
            navigation.SetScroll(selectedSystem.Name, scroll);
        }
    }

    private static bool IsReducedMotionEnabled() {
        return Mod.GetModSettings<CoreModSettings>().reduceMotion;
    }

    /// <summary>
    ///     Labels resolve once here, so a language switch needs the window reopened.
    /// </summary>
    private List<SettingsSearchDocument> BuildSearchDocuments() {
        List<SettingsSearchDocument> documents = [];

        for (int i = 0; i < systems.Count; i++) {
            CosmereModSettings system = systems[i];
            IReadOnlyList<SettingSection> sections = sectionsBySystem[system.Name];

            for (int s = 0; s < sections.Count; s++) {
                SettingSection section = sections[s];
                string sectionName = section.TitleKey.Translate();

                for (int d = 0; d < section.Settings.Count; d++) {
                    SettingDescriptor descriptor = section.Settings[d];
                    documents.Add(
                        new SettingsSearchDocument(
                            system.Name,
                            system.Name,
                            section.Key,
                            sectionName,
                            descriptor.Key,
                            descriptor.LabelKey.Translate(),
                            descriptor.DescriptionKey?.Translate(),
                            section,
                            descriptor
                        )
                    );
                }
            }
        }

        return documents;
    }

    private void DrawSearchResults(Rect viewportRect, ISystemSkin skin) {
        IReadOnlyList<SettingsSearchResult> results = searchIndex.Search(searchText.Trim());
        SettingsSearchDocument? chosen = searchResultsRenderer.Draw(viewportRect, skin, results, searchText.Trim());
        if (chosen == null) return;

        CosmereModSettings? target = systems.Find(system => system.Name == chosen.SystemKey);
        if (target == null) return;

        searchText = string.Empty;
        searchResultsRenderer.Reset();
        SelectSystem(target);
        selectedSectionBySystem[target.Name] = chosen.SectionKey;
        navigation.FlashSetting(chosen.SystemKey, chosen.SectionKey, chosen.SettingKey);
    }

    private void RequestClose() {
        CloseRequested = true;
        footerRenderer.CancelConfirmation();
    }
}
