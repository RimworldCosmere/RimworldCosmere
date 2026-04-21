using Cosmere.Core.UI.Codex;
using Cosmere.System.Roshar.Def;
using Cosmere.System.Roshar.Dialog;
using Cosmere.System.Roshar.Gene;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.System.Roshar.UI;

public sealed class SurgebindingCodexContent : ICodexContentProvider {
    public bool HasProgression(Pawn pawn) => GetSurgebinder(pawn) != null;
    public bool HasBonded(Pawn pawn) => GetSurgebinder(pawn)?.bondedSpren != null;
    public bool HasMemories(Pawn pawn) => false;

    public void DrawProgression(Pawn pawn, Rect rect) {
        Surgebinder? s = GetSurgebinder(pawn);
        if (s == null) return;

        RadiantOrderDef order = s.radiantOrderDef;
        float y = rect.y;

        using (new TextBlock(GameFont.Medium, TextAnchor.MiddleLeft, Color.white))
            Widgets.Label(new Rect(rect.x, y, rect.width - 110f, 30f), $"Order: {order.LabelCap}");

        Rect infoButton = new Rect(rect.xMax - 100f, y + 3f, 100f, 24f);
        if (Widgets.ButtonText(infoButton, "Order info")) {
            Find.WindowStack.Add(new RadiantOrderInfoDialog(pawn, s, RadiantOrderInfoMode.View));
        }
        y += 34f;

        using (new TextBlock(GameFont.Small, TextAnchor.MiddleLeft, new Color(0.85f, 0.85f, 0.85f)))
            Widgets.Label(new Rect(rect.x, y, rect.width, 24f), $"Ideals sworn: {s.currentIdeal} / 5");
        y += 28f;

        for (int i = 0; i < 5; i++) {
            int ideal = i + 1;
            bool achieved = ideal <= s.currentIdeal;
            Color barColor = achieved ? new Color(0.95f, 0.85f, 0.35f) : new Color(0.35f, 0.35f, 0.4f);
            Color textColor = achieved ? Color.white : new Color(0.55f, 0.55f, 0.6f);

            Rect row = new Rect(rect.x, y, rect.width, 26f);
            Rect dot = new Rect(row.x + 4f, row.y + 6f, 14f, 14f);
            Widgets.DrawBoxSolid(dot, barColor);

            using (new TextBlock(GameFont.Small, TextAnchor.MiddleLeft, textColor))
                Widgets.Label(new Rect(row.x + 24f, row.y, row.width - 24f, row.height), IdealLabel(order, ideal, achieved));
            y += 28f;
        }
    }

    public void DrawBonded(Pawn pawn, Rect rect) {
        Surgebinder? s = GetSurgebinder(pawn);
        Pawn? spren = s?.bondedSpren;
        if (spren == null) return;

        using (new TextBlock(GameFont.Medium, TextAnchor.MiddleLeft, Color.white))
            Widgets.Label(new Rect(rect.x, rect.y, rect.width, 30f), "Bonded spren");

        Rect sprenRow = new Rect(rect.x, rect.y + 34f, rect.width, 28f);
        using (new TextBlock(GameFont.Small, TextAnchor.MiddleLeft, new Color(0.9f, 0.9f, 0.9f)))
            Widgets.Label(sprenRow, spren.NameFullColored);

        if (Widgets.ButtonInvisible(sprenRow)) {
            CameraJumper.TryJumpAndSelect(spren);
        }
        if (Mouse.IsOver(sprenRow)) {
            Widgets.DrawHighlight(sprenRow);
        }
    }

    public void DrawMemories(Pawn pawn, Rect rect) { }

    private static Surgebinder? GetSurgebinder(Pawn pawn) {
        return pawn.genes?.GetFirstGeneOfType<Surgebinder>();
    }

    private static string IdealLabel(RadiantOrderDef order, int ideal, bool achieved) {
        if (achieved && order.ideals != null && ideal - 1 < order.ideals.Count) {
            return order.ideals[ideal - 1].label.CapitalizeFirst();
        }
        return $"Ideal {ideal} (unsworn)";
    }
}
