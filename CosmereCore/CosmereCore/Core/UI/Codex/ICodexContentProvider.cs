using UnityEngine;
using Verse;

namespace Cosmere.Core.UI.Codex;

public interface ICodexContentProvider {
    bool ShowsBondsSubtab { get; }
    bool HasProgression(Pawn pawn);
    void DrawProgression(Rect rect, Pawn pawn, CodexState state);
    bool HasBonds(Pawn pawn);
    void DrawBonds(Rect rect, Pawn pawn, CodexState state);

    bool HasMemories(Pawn pawn);
    void DrawMemories(Rect rect, Pawn pawn, CodexState state);

    bool OwnsAbility(RimWorld.Ability ability);

    string? HeaderLabelFor(Pawn pawn);
}