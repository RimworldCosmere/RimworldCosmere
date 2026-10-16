using Cosmere.Core.UI.Dock;
using Cosmere.System.Roshar.Def;
using Cosmere.System.Roshar.Gene;
using UnityEngine;
using Verse;

namespace Cosmere.System.Roshar.UI;

/// <summary>
///     One dot per ideal, connected left to right. Sworn ideals read grey, the one the pawn
///     is living reads in the gauge's own colour, and what is left ahead sits nearly dark.
/// </summary>
public static class OathLadder {
    private static readonly Color SwornDot = new Color(0.427f, 0.416f, 0.388f);
    private static readonly Color UnswornDot = new Color(0.169f, 0.169f, 0.169f);
    private static readonly Color ConnectorDone = new Color(0.290f, 0.278f, 0.251f);
    private static readonly Color ConnectorAhead = new Color(0.149f, 0.149f, 0.165f);

    public static void Draw(Rect rect, RadiantOrderDef order, Surgebinder gene, Color lit) {
        List<Ideal>? ideals = order.ideals;
        int count = OathLadderLayout.DotCount(ideals?.Count ?? 0);
        if (count == 0) return;

        float dotY = rect.y + (rect.height - OathLadderLayout.DotSize) / 2f;
        float lineY = rect.y + (rect.height - OathLadderLayout.Connector) / 2f;
        int current = gene.CurrentIdeal;

        Color previous = GUI.color;

        for (int i = 0; i < count - 1; i++) {
            float from = OathLadderLayout.DotX(i, count, rect.x, rect.width) + OathLadderLayout.DotSize;
            float to = OathLadderLayout.DotX(i + 1, count, rect.x, rect.width);
            Rect line = new Rect(from, lineY, Mathf.Max(0f, to - from), OathLadderLayout.Connector);

            Widgets.DrawBoxSolid(line, i < current ? ConnectorDone : ConnectorAhead);
        }

        for (int i = 0; i < count; i++) {
            Rect dot = new Rect(
                OathLadderLayout.DotX(i, count, rect.x, rect.width),
                dotY,
                OathLadderLayout.DotSize,
                OathLadderLayout.DotSize
            );

            GUI.color = OathLadderLayout.StateFor(i, current) switch {
                OathDotState.Sworn => SwornDot,
                OathDotState.Current => lit,
                _ => UnswornDot,
            };
            Widgets.DrawAtlas(dot, DockTex.RoundFill);
            GUI.color = previous;

            Ideal ideal = ideals![i];
            TooltipHandler.TipRegion(
                dot,
                "CC_Dock_Oath_Ladder_Tip".Translate(
                    ideal.label.Named("IDEAL"),
                    ideal.description.Named("WORDS")
                )
            );
        }
    }
}
