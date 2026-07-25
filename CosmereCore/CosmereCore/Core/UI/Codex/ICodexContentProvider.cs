using UnityEngine;
using Verse;

namespace Cosmere.Core.UI.Codex;

public interface ICodexContentProvider {
    bool ShowsBondsSubtab { get; }

    /// Whether this art keeps memories at all. Allomancy has nowhere to put them,
    /// so the tab should not be offered rather than opening onto nothing.
    bool ShowsMemoriesSubtab { get; }
    bool HasProgression(Pawn pawn);
    void DrawProgression(Rect rect, Pawn pawn, CodexState state);
    bool HasBonds(Pawn pawn);
    void DrawBonds(Rect rect, Pawn pawn, CodexState state);

    bool HasMemories(Pawn pawn);
    void DrawMemories(Rect rect, Pawn pawn, CodexState state);

    bool OwnsAbility(RimWorld.Ability ability);

    string? HeaderLabelFor(Pawn pawn);
}