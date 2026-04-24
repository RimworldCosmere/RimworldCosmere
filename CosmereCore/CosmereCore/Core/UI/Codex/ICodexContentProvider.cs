using UnityEngine;
using Verse;

namespace Cosmere.Core.UI.Codex;

public interface ICodexContentProvider {
    bool ShowsBondsSubtab { get; }
    bool HasProgression(Pawn pawn);
    void DrawProgression(Pawn pawn, Rect rect, CodexState state);
    bool HasBonds(Pawn pawn);
    void DrawBonds(Pawn pawn, Rect rect, CodexState state);

    bool HasMemories(Pawn pawn);
    void DrawMemories(Pawn pawn, Rect rect, CodexState state);

    bool OwnsAbility(RimWorld.Ability ability);

    string? HeaderLabelFor(Pawn pawn);
}