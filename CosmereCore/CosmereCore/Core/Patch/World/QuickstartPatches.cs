using System;
using System.Reflection;
using System.Reflection.Emit;
using Cosmere.Core.Quickstart;
using HarmonyLib;
using Verse;

namespace Cosmere.Core.Patch;

[HarmonyPatch]
public static class QuickstartPatch {
    private static bool patchedDrawButtons;

    [HarmonyPrepare]
    public static bool Prepare() {
        LongEventHandler.ExecuteWhenFinished(() => {
            if (!patchedDrawButtons) Logger.Warning("DebugWindowOpener.DrawButtons could not be applied.");
        }
        );
        return true;
    }

    [HarmonyPatch(typeof(Root), nameof(Root.OnGUI))]
    [HarmonyPostfix]
    public static void PostfixRootOnGUI() {
        Quickstarter.instance?.OnGUI();
    }

    [HarmonyPatch(typeof(DebugWindowsOpener), "DrawButtons")]
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> DrawAdditionalButtons(IEnumerable<CodeInstruction> instructions) {
        patchedDrawButtons = false;
        CodeInstruction[] instructionsArr = instructions.ToArray();
        FieldInfo? widgetRowField = AccessTools.Field(typeof(DebugWindowsOpener), "widgetRow");
        foreach (CodeInstruction inst in instructionsArr) {
            // before "if (Current.ProgramState == ProgramState.Playing)"
            if (!patchedDrawButtons && widgetRowField != null && inst.opcode == OpCodes.Bne_Un_S) {
                yield return new CodeInstruction(OpCodes.Ldarg_0);
                yield return new CodeInstruction(OpCodes.Ldfld, widgetRowField);
                yield return new CodeInstruction(
                    OpCodes.Call,
                    ((Action<WidgetRow>)Quickstarter.DrawDebugToolbarButton).Method
                );
                patchedDrawButtons = true;
            }

            yield return inst;
        }
    }
}