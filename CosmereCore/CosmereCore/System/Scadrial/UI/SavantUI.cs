
using RimWorld;
using UnityEngine;
using Verse;
namespace Cosmere.System.Scadrial.UI;

public static class SavantUI {
    public static void DrawSavantStage(Rect rect, int stage, Color accent) {
        string label;
        Color color;
        switch (stage) {
            case 1:
                label = (string)"CC_Codex_Savant_Stage1".Translate();
                color = Color.Lerp(accent, Color.white, 0.35f);
                break;
            case 2:
                label = (string)"CC_Codex_Savant_Stage2".Translate();
                color = accent;
                break;
            case 3:
                label = (string)"CC_Codex_Savant_Stage3".Translate();
                color = Color.Lerp(accent, new Color(1f, 0.9f, 0.4f), 0.5f);
                break;
            default:
                label = (string)"CC_Codex_Savant_Stage0".Translate();
                color = new Color(0.55f, 0.55f, 0.55f);
                break;
        }

        using (new TextBlock(GameFont.Small, TextAnchor.MiddleLeft, color))
            Widgets.Label(rect, label);
    }
}
