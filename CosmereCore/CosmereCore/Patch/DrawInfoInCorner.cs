using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace CosmereCore.Patch;

[HarmonyPatch(typeof(VersionControl), nameof(VersionControl.DrawInfoInCorner))]
internal static class DrawInfoInCorner {
    private static List<string>? cosmereMods;

    private static TaggedString tooltip {
        get {
            StringBuilder sb = new StringBuilder();
            foreach (string mod in cosmereMods ?? []) {
                sb.AppendLine(mod);
            }

            return sb.ToTaggedString();
        }
    }

    [HarmonyPostfix]
    private static void Postfix() {
        cosmereMods ??= typeof(Mod).AllSubclassesNonAbstract()
            .Where(m => m.Name.Contains("Cosmere"))
            .Select(m =>
                m.Name.Colorize(ColoredText.ExpectationsColor) +
                ": " +
                ("v" + m.Assembly.GetName().Version).Colorize(ColoredText.DateTimeColor)
            )
            .ToList();

        Version? version = typeof(CosmereCore).Assembly.GetName().Version;
        string text = $"RimworldCosmere v{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";

        Rect rect;
        using (new TextBlock(GameFont.Small, Color.white.ToTransparent(0.5f))) {
            Vector2 vector = Text.CalcSize(text);
            rect = new Rect(10f, 75f, vector.x, vector.y);
            Widgets.Label(rect, text);
        }

        if (Mouse.IsOver(rect)) {
            TooltipHandler.TipRegion(rect, tooltip);
            Widgets.DrawHighlight(rect);
        }
    }
}