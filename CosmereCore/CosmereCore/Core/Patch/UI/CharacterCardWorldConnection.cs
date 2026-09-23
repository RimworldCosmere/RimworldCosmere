using System.Text;
using Cosmere.Core.Def;
using Cosmere.Core.ShardConnection;
using Cosmere.Core.UI.Codex;
using Cosmere.Core.UI.Skin;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.Core.Patch;

[StaticConstructorOnStartup]
public static class CharacterCardWorldConnection {
    private const float IconSize = 20f;
    private const float IconGap = 5f;

    public static void AddHomeworldElement(List<GenUI.AnonymousStackElement> elements, Pawn pawn) {
        CosmereWorldDef? home = WorldConnectionUtility.HomeworldFor(pawn);
        if (home == null) return;

        float width = IconSize + IconGap + Text.CalcSize(home.LabelCap).x + 14f;
        elements.Add(new GenUI.AnonymousStackElement {
            drawer = rect => DrawHomeworldButton(rect, pawn, WorldConnectionUtility.For(pawn, home)),
            width = width,
        });
    }

    private static void DrawHomeworldButton(Rect rect, Pawn pawn, WorldConnection entry) {
        Color previousColor = GUI.color;
        GUI.color = CharacterCardUtility.StackElementBackground;
        GUI.DrawTexture(rect, BaseContent.WhiteTex);
        GUI.color = previousColor;
        Widgets.DrawHighlightIfMouseover(rect);

        Texture2D? icon = SystemSkinRegistry.ForOrFallback(entry.World.SkinId).Sigil ?? ConnectionTextures.Sigil;
        float textX = rect.x + 5f;
        if (icon != null) {
            Rect iconRect = new Rect(rect.x + 1f, rect.y + 1f, IconSize, IconSize);
            GUI.DrawTexture(iconRect, icon);
            textX = iconRect.xMax + IconGap;
        }

        TextAnchor previousAnchor = Text.Anchor;
        Text.Anchor = TextAnchor.MiddleLeft;
        Rect labelRect = new Rect(textX, rect.y, Mathf.Max(0f, rect.xMax - 5f - textX), rect.height);
        Widgets.Label(labelRect, entry.World.LabelCap.Truncate(labelRect.width));
        Text.Anchor = previousAnchor;

        if (Mouse.IsOver(rect)) TooltipHandler.TipRegion(rect, AllPlanetConnectionTip(pawn, entry.World));

        if (Widgets.ButtonInvisible(rect)) Find.WindowStack.Add(new Dialog_InfoCard(entry.World));
    }

    private static string AllPlanetConnectionTip(Pawn pawn, CosmereWorldDef home) {
        StringBuilder text = new StringBuilder();
        text.AppendLine(
            "CC_Homeworld_TipTitle".Translate(home.LabelCap.Named("WORLD")).Colorize(ColoredText.TipSectionTitleColor)
        );
        text.AppendLine();

        List<WorldConnection> entries = WorldConnectionUtility.ActiveFor(pawn);
        for (int i = 0; i < entries.Count; i++) {
            WorldConnection entry = entries[i];
            text.AppendLine(
                "CC_PlanetConnection_TipRow".Translate(
                    entry.World.LabelCap.Named("WORLD"),
                    entry.Strength.Named("LEVEL"),
                    ConnectionMath.Max.Named("MAX"),
                    entry.Ancestry.Named("ANCESTRY"),
                    entry.Residence.Named("RESIDENCE"),
                    entry.Investiture.Named("INVESTITURE"),
                    entry.Earned.Named("EARNED")
                )
            );
        }

        text.AppendLine();
        text.Append(
            "CC_Homeworld_ClickToView".Translate(home.LabelCap.Named("WORLD")).Colorize(ColoredText.SubtleGrayColor)
        );
        return text.ToString();
    }
}
