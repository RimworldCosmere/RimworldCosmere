using System;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace CosmereFramework.Patch;

[HarmonyPatch]
public static class PatchInTPS {
    private const double TicksPerSecondMultiplier = 1.0 / TimeSpan.TicksPerSecond;
    private static long PreviousTime;
    private static long PreviousTick = -1;
    private static float PreviousTickRateMultiplier = -1;
    private static string ToDisplay = "TPS: 0";

    [HarmonyPatch(typeof(GlobalControlsUtility), nameof(GlobalControlsUtility.DrawFpsCounter))]
    [HarmonyPostfix]
    public static void Postfix(float leftX, float width, ref float curBaseY) {
        if (!CosmereFramework.cosmereSettings.debugMode) return;

        float trm = Current.Game.tickManager.TickRateMultiplier;
        long td = DateTime.Now.Ticks - PreviousTime;
        if (!Mathf.Approximately(trm, PreviousTickRateMultiplier) || td >= TimeSpan.TicksPerSecond) {
            long tick = Current.Game.tickManager.TicksGame;
            if (PreviousTick < -1) {
                PreviousTick = tick;
            }

            int target = (int)Math.Round(trm == 0f ? 0f : 60f * trm);
            ToDisplay = $"TPS: {(tick - PreviousTick) / (td * TicksPerSecondMultiplier):0} ({target})";
            PreviousTime = DateTime.Now.Ticks;
            PreviousTick = tick;
            PreviousTickRateMultiplier = trm;
        }

        Rect rect = new Rect(leftX - 20f, curBaseY - 26f, width + 20f - 7f, 26f);
        Text.Anchor = TextAnchor.MiddleRight;
        Widgets.Label(rect, ToDisplay);
        Text.Anchor = TextAnchor.UpperLeft;
        curBaseY -= 26f;
    }
}