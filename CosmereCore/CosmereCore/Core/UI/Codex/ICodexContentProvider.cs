using UnityEngine;
using Verse;

namespace Cosmere.Core.UI.Codex;

public interface ICodexContentProvider {
    bool HasProgression(Pawn pawn);
    void DrawProgression(Pawn pawn, Rect rect);

    bool HasBonded(Pawn pawn);
    void DrawBonded(Pawn pawn, Rect rect);

    bool HasMemories(Pawn pawn);
    void DrawMemories(Pawn pawn, Rect rect);
}
