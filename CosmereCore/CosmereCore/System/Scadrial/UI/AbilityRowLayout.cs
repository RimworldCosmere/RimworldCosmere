namespace Cosmere.System.Scadrial.UI;

public readonly record struct AbilityRow(string DefName, string Label, bool IsTargeted, bool CanFlare);

// Deliberately free of RimWorld and Unity types so the test host can load it.
public static class AbilityRowLayout {
    public const float GroupHeaderHeight = 20f;
    public const float RowGap = 4f;
    public const float RowHeight = 26f;

    public static List<AbilityRow> Ordered(IReadOnlyList<AbilityRow> rows) {
        List<AbilityRow> ordered = new List<AbilityRow>(rows.Count);
        for (int i = 0; i < rows.Count; i++) {
            if (!rows[i].IsTargeted) ordered.Add(rows[i]);
        }

        for (int i = 0; i < rows.Count; i++) {
            if (rows[i].IsTargeted) ordered.Add(rows[i]);
        }

        return ordered;
    }

    // A lone header over a single row is chrome, and twelve of sixteen metals grant one ability.
    public static bool ShowGroupHeaders(IReadOnlyList<AbilityRow> rows) {
        bool sustained = false;
        bool targeted = false;
        for (int i = 0; i < rows.Count; i++) {
            if (rows[i].IsTargeted) targeted = true;
            else sustained = true;
        }

        return sustained && targeted;
    }

    public static float HeightFor(IReadOnlyList<AbilityRow> rows) {
        if (rows.Count == 0) return 0f;

        float height = rows.Count * RowHeight + (rows.Count - 1) * RowGap;
        if (ShowGroupHeaders(rows)) height += GroupHeaderHeight * 2f;

        return height;
    }
}
