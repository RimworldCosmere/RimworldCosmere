using System.Text;
using UnityEngine;
using Verse;

namespace Cosmere.Foundation.Quickstart;

public class StatusBox(Quickstarter quickstarter) {
    private static readonly Vector2 RectSize = new Vector2(240f, 75f);
    private static readonly Vector2 RectPadding = new Vector2(26f, 18f);

    public void OnGUI() {
        string statusText = GetStatusBoxText();
        Rect boxRect = GetStatusBoxRect(statusText);
        DrawStatusBox(boxRect, statusText);
    }

    private string GetStatusBoxText() {
        StringBuilder sb = new StringBuilder("Quickstarter is launching the following quickstart \n");
        sb.AppendLine();
        sb.AppendLine();
        sb.AppendLine(quickstarter.Quickstart!.GetType().Name.Colorize(ColoredText.GeneColor));
        sb.AppendLine();
        sb.AppendLine(quickstarter.Quickstart!.description.Colorize(ColorLibrary.Grey));
        return sb.ToString();
    }

    private static Rect GetStatusBoxRect(string statusText) {
        Vector2 statusTextSize = Text.CalcSize(statusText);
        float boxWidth = Mathf.Max(RectSize.x, statusTextSize.x + RectPadding.x * 2f);
        float boxHeight = Mathf.Max(RectSize.y, statusTextSize.y + RectPadding.y * 2f);
        Rect boxRect = new Rect((Verse.UI.screenWidth - boxWidth) / 2f, (Verse.UI.screenHeight / 2f - boxHeight) / 2f, boxWidth, boxHeight);
        boxRect = boxRect.Rounded();
        return boxRect;
    }

    private static void DrawStatusBox(Rect rect, string statusText) {
        Widgets.DrawShadowAround(rect);
        Widgets.DrawWindowBackground(rect);
        TextAnchor prevAnchor = Text.Anchor;
        Text.Anchor = TextAnchor.MiddleCenter;
        Widgets.Label(rect, statusText);
        Text.Anchor = prevAnchor;
    }
}