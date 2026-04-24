using Cosmere.Core.UI.Codex;
using Cosmere.Core.UI.Model;
using Cosmere.Core.UI.Radial;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.System.Nalthis.UI;

public sealed class AwakeningInvestitureProvider : IInvestitureProvider, ICodexContentProvider {
    private static readonly AwakeningCodexContent codex = new AwakeningCodexContent();

    public bool HasProgression(Pawn pawn) {
        return codex.HasProgression(pawn);
    }

    public void DrawProgression(Pawn pawn, Rect rect, CodexState state) {
        codex.DrawProgression(pawn, rect, state);
    }

    public bool ShowsBondsSubtab => codex.ShowsBondsSubtab;

    public bool HasBonds(Pawn pawn) {
        return codex.HasBonds(pawn);
    }

    public void DrawBonds(Pawn pawn, Rect rect, CodexState state) {
        codex.DrawBonds(pawn, rect, state);
    }

    public bool HasMemories(Pawn pawn) {
        return codex.HasMemories(pawn);
    }

    public void DrawMemories(Pawn pawn, Rect rect, CodexState state) {
        codex.DrawMemories(pawn, rect, state);
    }

    public bool OwnsAbility(Ability ability) {
        return codex.OwnsAbility(ability);
    }

    public string? HeaderLabelFor(Pawn pawn) {
        return codex.HeaderLabelFor(pawn);
    }

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
}