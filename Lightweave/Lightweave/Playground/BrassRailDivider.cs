using System.Runtime.CompilerServices;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using UnityEngine;

namespace Cosmere.Lightweave.Playground;

public static class BrassRailDivider {
    private const float BarWidthFraction = 0.4f;
    private const float BarThickness = 2f;
    private const float DiamondSize = 4f;
    private const float AccentAlpha = 0.6f;

    public static LightweaveNode Create(
        Style? style = null,
        string[]? classes = null,
        string? id = null,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        LightweaveNode node = NodeBuilder.New("BrassRailDivider", line, file);
        node.ApplyStyling("brass-rail-divider", style, classes, id);
        node.PreferredHeight = 8f;
        node.Paint = (rect, _) => {
            Theme.Theme theme = RenderContext.Current.Theme;
            Color accent = theme.GetColor(ThemeSlot.SurfaceAccent);
            accent.a *= AccentAlpha;

            float barWidth = rect.width * BarWidthFraction;
            float barX = rect.x + (rect.width - barWidth) / 2f;
            float barY = rect.y + (rect.height - BarThickness) / 2f;

            Rect leftBar = new Rect(barX, barY, (barWidth - DiamondSize) / 2f, BarThickness);
            Rect rightBar = new Rect(
                leftBar.xMax + DiamondSize,
                barY,
                (barWidth - DiamondSize) / 2f,
                BarThickness
            );

            Color saved = GUI.color;
            GUI.color = accent;
            GUI.DrawTexture(leftBar, Texture2D.whiteTexture);
            GUI.DrawTexture(rightBar, Texture2D.whiteTexture);

            Rect diamond = new Rect(
                rect.x + (rect.width - DiamondSize) / 2f,
                rect.y + (rect.height - DiamondSize) / 2f,
                DiamondSize,
                DiamondSize
            );
            GUI.DrawTexture(diamond, Texture2D.whiteTexture);

            GUI.color = saved;
        };
        return node;
    }
}