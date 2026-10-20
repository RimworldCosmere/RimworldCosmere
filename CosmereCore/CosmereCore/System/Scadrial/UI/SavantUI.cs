using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.System.Scadrial.UI;

public static class SavantUI {
    private static readonly Color NoviceColor = new Color(0.55f, 0.55f, 0.55f);
    private static readonly Color TranscendentTint = new Color(1f, 0.9f, 0.4f);

    public static string StageLabel(int stage) {
        return stage switch {
            1 => "CC_Codex_Savant_Stage1".Translate(),
            2 => "CC_Codex_Savant_Stage2".Translate(),
            3 => "CC_Codex_Savant_Stage3".Translate(),
            _ => "CC_Codex_Savant_Stage0".Translate(),
        };
    }

    public static void DrawSavantStage(Rect rect, int stage, Color accent) {
        Color color = stage switch {
            1 => Color.Lerp(accent, Color.white, 0.35f),
            2 => accent,
            3 => Color.Lerp(accent, TranscendentTint, 0.5f),
            _ => NoviceColor,
        };

        using (new TextBlock(GameFont.Small, TextAnchor.MiddleLeft, color))
            Widgets.Label(rect, StageLabel(stage));
    }
}
