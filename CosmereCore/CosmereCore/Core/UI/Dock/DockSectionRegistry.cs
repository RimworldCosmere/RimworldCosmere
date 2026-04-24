namespace Cosmere.Core.UI.Dock;

public static class DockSectionRegistry {
    private static readonly List<IDockSection> sections = [];

    public static IReadOnlyList<IDockSection> All => sections;

    public static void Register(IDockSection section) {
        for (int i = 0; i < sections.Count; i++) {
            if (sections[i].SystemId == section.SystemId) return;
        }

        sections.Add(section);
    }

    public static IDockSection? For(string systemId) {
        for (int i = 0; i < sections.Count; i++) {
            if (sections[i].SystemId == systemId) return sections[i];
        }

        return null;
    }
}