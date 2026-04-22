using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.Core.UI.Codex;

public interface ICodexContentProvider {
    bool HasProgression(Pawn pawn);
    void DrawProgression(Pawn pawn, Rect rect, CodexState state);

    bool ShowsBondsSubtab { get; }
    bool HasBonds(Pawn pawn);
    void DrawBonds(Pawn pawn, Rect rect, CodexState state);

    bool HasMemories(Pawn pawn);
    void DrawMemories(Pawn pawn, Rect rect, CodexState state);

    bool OwnsAbility(RimWorld.Ability ability);

    string? HeaderLabelFor(Pawn pawn);
}
