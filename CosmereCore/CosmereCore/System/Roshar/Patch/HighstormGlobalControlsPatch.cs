using System.Reflection;
using System.Reflection.Emit;
using Cosmere.System.Roshar.Comp.Map;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.System.Roshar.Patch;

[StaticConstructorOnStartup]
[HarmonyPatch]
public static class HighstormGlobalControlsPatch {
    public static bool showHighstormReadout = true;

    private static bool patchedReadout;
    private static bool patchedToggle;
    private static Texture2D? highstormIcon;

    [HarmonyPrepare]
    public static bool Prepare() {
        LongEventHandler.ExecuteWhenFinished(() => {
            if (!patchedReadout) Log.Warning("[Cosmere] GlobalControls.GlobalControlsOnGUI highstorm readout transpiler could not be applied.");
            if (!patchedToggle) Log.Warning("[Cosmere] PlaySettings.DoPlaySettingsGlobalControls highstorm toggle transpiler could not be applied.");
        });
        return true;
    }

    [HarmonyPatch(typeof(GlobalControls), nameof(GlobalControls.GlobalControlsOnGUI))]
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> ReadoutTranspiler(IEnumerable<CodeInstruction> instructions) {
        MethodInfo doDateMethod = AccessTools.Method(typeof(GlobalControlsUtility), nameof(GlobalControlsUtility.DoDate));
        MethodInfo drawReadout = AccessTools.Method(typeof(HighstormGlobalControlsPatch), nameof(DrawHighstormReadout));

        foreach (CodeInstruction instruction in instructions) {
            yield return instruction;

            if (!patchedReadout && instruction.Calls(doDateMethod)) {
                yield return new CodeInstruction(OpCodes.Ldloc_0);
                yield return new CodeInstruction(OpCodes.Ldloca_S, (byte)1);
                yield return new CodeInstruction(OpCodes.Call, drawReadout);
                patchedReadout = true;
            }
        }
    }

    [HarmonyPatch(typeof(PlaySettings), nameof(PlaySettings.DoPlaySettingsGlobalControls))]
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> ToggleTranspiler(IEnumerable<CodeInstruction> instructions) {
        MethodInfo doMapControls = AccessTools.Method(typeof(PlaySettings), "DoMapControls");
        MethodInfo drawToggle = AccessTools.Method(typeof(HighstormGlobalControlsPatch), nameof(DrawHighstormToggle));

        foreach (CodeInstruction instruction in instructions) {
            yield return instruction;

            if (!patchedToggle && instruction.Calls(doMapControls)) {
                yield return new CodeInstruction(OpCodes.Ldarg_1);
                yield return new CodeInstruction(OpCodes.Call, drawToggle);
                patchedToggle = true;
            }
        }
    }

    public static void DrawHighstormReadout(float leftX, ref float curBaseY) {
        if (!showHighstormReadout) return;

        Map map = Find.CurrentMap;
        if (map == null) return;

        string statusText = HighstormScheduler.GetStatusText(map);
        if (statusText == null) return;

        Rect rect = new Rect(leftX - 100f, curBaseY - 26f, 293f, 26f);
        Text.Anchor = TextAnchor.MiddleRight;
        Widgets.Label(rect, statusText);
        Text.Anchor = TextAnchor.UpperLeft;

        string tooltip = HighstormScheduler.GetTooltipText(map);
        if (tooltip != null) {
            TooltipHandler.TipRegion(rect, tooltip);
        }

        curBaseY -= 26f;
    }

    public static void DrawHighstormToggle(WidgetRow row) {
        highstormIcon ??= ContentFinder<Texture2D>.Get("UI/Icons/Highstorm", false);
        if (highstormIcon == null) return;

        Color prevColor = GUI.color;
        GUI.color = new Color(2f, 2f, 2f, 1f);
        row.ToggleableIcon(
            ref showHighstormReadout,
            highstormIcon,
            "Toggle highstorm readout",
            SoundDefOf.Mouseover_ButtonToggle
        );
        GUI.color = prevColor;
    }
}
