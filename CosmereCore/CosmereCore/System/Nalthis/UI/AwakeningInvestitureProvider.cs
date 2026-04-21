using Cosmere.Core.UI.Codex;
using Cosmere.Core.UI.Model;
using Cosmere.Core.UI.Radial;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.System.Nalthis.UI;

public sealed class AwakeningInvestitureProvider : IInvestitureProvider, ICodexContentProvider {
    private static readonly AwakeningCodexContent codex = new();
    public string SystemId => "Awakening";

    public bool IsInvested(Pawn pawn) {
        return false;
    }

    public InvestitureSnapshot? Snapshot(Pawn pawn) {
        return null;
    }

    public RadialSystem? SnapshotRadial(Pawn pawn) {
        return null;
    }

    public bool HasProgression(Pawn pawn) => codex.HasProgression(pawn);
    public void DrawProgression(Pawn pawn, Rect rect) => codex.DrawProgression(pawn, rect);
    public bool HasBonded(Pawn pawn) => codex.HasBonded(pawn);
    public void DrawBonded(Pawn pawn, Rect rect) => codex.DrawBonded(pawn, rect);
    public bool HasMemories(Pawn pawn) => codex.HasMemories(pawn);
    public void DrawMemories(Pawn pawn, Rect rect) => codex.DrawMemories(pawn, rect);
    public string? HeaderLabelFor(Pawn pawn) => codex.HeaderLabelFor(pawn);
}
