using Cosmere.Core.UI.Codex;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.System.Nalthis.UI;

public sealed class AwakeningCodexContent : ICodexContentProvider {
    public bool HasProgression(Pawn pawn) {
        return true;
    }

    public bool ShowsMemoriesSubtab => false;

    public bool ShowsBondsSubtab => false;

    public bool HasBonds(Pawn pawn) {
        return false;
    }

    public bool HasMemories(Pawn pawn) {
        return false;
    }

    public bool OwnsAbility(Ability ability) {
        return false;
    }

    public string? HeaderLabelFor(Pawn pawn) {
        return null;
    }

    public void DrawProgression(Rect rect, Pawn pawn, CodexState state) {
        using (new TextBlock(GameFont.Medium, TextAnchor.MiddleLeft, Color.white))
            Widgets.Label(
                new Rect(rect.x, rect.y, rect.width, 30f),
                "CC_Codex_Awakening_Progression_Header".Translate()
            );

        using (new TextBlock(GameFont.Small, TextAnchor.MiddleLeft, new Color(0.75f, 0.75f, 0.75f)))
            Widgets.Label(
                new Rect(rect.x, rect.y + 34f, rect.width, 24f),
                "CC_Codex_Awakening_Progression_Placeholder".Translate()
            );
    }

    public void DrawBonds(Rect rect, Pawn pawn, CodexState state) { }
    public void DrawMemories(Rect rect, Pawn pawn, CodexState state) { }
}