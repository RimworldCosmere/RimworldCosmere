using System.Reflection;
using System.Reflection.Emit;
using Concord;
using Cosmere.System.Roshar.Comp.Map;
using RimWorld;
using UnityEngine;
using Verse;
using Logger = Cosmere.Core.Logger;

namespace Cosmere.System.Roshar.Patch.Highstorm;

// Kept for the Texture2D field: RimWorld's startup check flags any type holding one, even though
// the icon here is loaded lazily on the main thread rather than in a static constructor.
[StaticConstructorOnStartup]
public static class HighstormGlobalControlsPatch {
    public static bool showHighstormReadout = true;

    private static Texture2D? highstormIcon;

    public static void DrawHighstormReadout(float leftX, ref float curBaseY) {
        if (!showHighstormReadout) return;

        Map map = Find.CurrentMap;
        if (map == null) return;

        string? statusText = HighstormScheduler.GetStatusText(map);
        if (statusText == null) return;

        Rect rect = new Rect(leftX - 100f, curBaseY - 26f, 293f, 26f);
        Text.Anchor = TextAnchor.MiddleRight;
        Widgets.Label(rect, statusText);
        Text.Anchor = TextAnchor.UpperLeft;

        string? tooltip = HighstormScheduler.GetTooltipText(map);
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

[Patch]
public abstract class HighstormReadoutPatch : GlobalControls {
    [Inject(At.Transpiler, nameof(GlobalControlsOnGUI))]
    private static IEnumerable<CodeInstruction> InsertReadout(IEnumerable<CodeInstruction> instructions) {
        MethodInfo doDateMethod = typeof(GlobalControlsUtility).GetMethod(
            nameof(GlobalControlsUtility.DoDate),
            BindingFlags.Public | BindingFlags.Static
        )!;
        MethodInfo drawReadout = typeof(HighstormGlobalControlsPatch).GetMethod(
            nameof(HighstormGlobalControlsPatch.DrawHighstormReadout),
            BindingFlags.Public | BindingFlags.Static
        )!;

        // The "first match only" guard is a local, not a field: Concord recomposes the target
        // from raw IL on every patch and unpatch, so a static flag would suppress the insert
        // on every run after the first.
        bool inserted = false;

        foreach (CodeInstruction instruction in instructions) {
            yield return instruction;

            if (inserted) continue;
            if (!instruction.Is(OpCodes.Call, doDateMethod) && !instruction.Is(OpCodes.Callvirt, doDateMethod)) {
                continue;
            }

            yield return new CodeInstruction(OpCodes.Ldloc_0);
            yield return new CodeInstruction(OpCodes.Ldloca_S, (byte)1);
            yield return new CodeInstruction(OpCodes.Call, drawReadout);
            inserted = true;
        }

        // Reported here rather than from a startup callback: Patcher.Apply runs inside a queued
        // long event, so anything checking a flag from LongEventHandler.ExecuteWhenFinished reads
        // it before this transpiler has run.
        if (!inserted) {
            Logger.Warning("GlobalControls.GlobalControlsOnGUI highstorm readout transpiler found no DoDate call.");
        }
    }
}

[Patch(typeof(PlaySettings))]
public static class HighstormTogglePatch {
    [Inject(At.Transpiler, nameof(PlaySettings.DoPlaySettingsGlobalControls))]
    private static IEnumerable<CodeInstruction> InsertToggle(IEnumerable<CodeInstruction> instructions) {
        MethodInfo doMapControls = typeof(PlaySettings).GetMethod(
            "DoMapControls",
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public
        )!;
        MethodInfo drawToggle = typeof(HighstormGlobalControlsPatch).GetMethod(
            nameof(HighstormGlobalControlsPatch.DrawHighstormToggle),
            BindingFlags.Public | BindingFlags.Static
        )!;

        bool inserted = false;

        foreach (CodeInstruction instruction in instructions) {
            yield return instruction;

            if (inserted) continue;
            if (!instruction.Is(OpCodes.Call, doMapControls) && !instruction.Is(OpCodes.Callvirt, doMapControls)) {
                continue;
            }

            yield return new CodeInstruction(OpCodes.Ldarg_1);
            yield return new CodeInstruction(OpCodes.Call, drawToggle);
            inserted = true;
        }

        if (!inserted) {
            Logger.Warning(
                "PlaySettings.DoPlaySettingsGlobalControls highstorm toggle transpiler found no DoMapControls call."
            );
        }
    }
}
