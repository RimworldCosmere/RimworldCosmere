using Cosmere.Core.UI.Codex;
using UnityEngine;
using Verse;

namespace Cosmere.System.Nalthis.UI;

public sealed class AwakeningCodexContent : ICodexContentProvider {
    public bool HasProgression(Pawn pawn) => true;
    public bool ShowsBondsSubtab => false;
    public bool HasBonds(Pawn pawn) => false;
    public bool HasMemories(Pawn pawn) => false;
    public bool OwnsAbility(RimWorld.Ability ability) => false;
    public string? HeaderLabelFor(Pawn pawn) => null;

    public void DrawProgression(Pawn pawn, Rect rect) {
        using (new TextBlock(GameFont.Medium, TextAnchor.MiddleLeft, Color.white))
            Widgets.Label(new Rect(rect.x, rect.y, rect.width, 30f), "CC_Codex_Awakening_Progression_Header".Translate());

        using (new TextBlock(GameFont.Small, TextAnchor.MiddleLeft, new Color(0.75f, 0.75f, 0.75f)))
            Widgets.Label(new Rect(rect.x, rect.y + 34f, rect.width, 24f), "CC_Codex_Awakening_Progression_Placeholder".Translate());
    }

    public void DrawBonds(Pawn pawn, Rect rect, CodexState state) { }
    public void DrawMemories(Pawn pawn, Rect rect) { }
}
