using System.Collections.Generic;
using Cosmere.Lightweave.Fonts;
using Cosmere.Lightweave.Settings;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.Lightweave.Patch;

[HarmonyPatch(typeof(Dialog_Options), "DoUIOptions")]
public static class Dialog_OptionsFontSizePatch {
    private static readonly int[] Presets = { 85, 100, 115, 125 };

    public static void Prefix(Listing_Standard listing) {
        LightweaveSettings? settings = LightweaveMod.Settings;
        if (settings != null && settings.RedesignMainMenu) {
            return;
        }
        DrawFontSizeRow(listing);
    }

    private static void DrawFontSizeRow(Listing_Standard listing) {
        if (listing == null) {
            return;
        }

        LightweaveSettings settings = LightweaveMod.Settings;
        if (settings == null) {
            return;
        }

        string label = "CL_FontSize".Translate();
        string current = settings.FontScalePercent + "%";
        if (!listing.ButtonTextLabeledPct(label, current, 0.6f, TextAnchor.MiddleLeft, null, null, null)) {
            return;
        }

        List<FloatMenuOption> options = new List<FloatMenuOption>(Presets.Length);
        for (int i = 0; i < Presets.Length; i++) {
            int captured = Presets[i];
            options.Add(new FloatMenuOption(captured + "%", () => {
                settings.FontScalePercent = captured;
                LightweaveMod.Save();
                GameFontOverride.Apply();
            }));
        }
        Find.WindowStack.Add(new FloatMenu(options));
    }
}



