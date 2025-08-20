using System.Reflection.Emit;
using System.Text;
using Cosmere.Roshar.LesserSpren.SprenController;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace Cosmere.Roshar.Patch;

[HarmonyPatch(typeof(MouseoverReadout), nameof(MouseoverReadout.MouseoverReadoutOnGUI))]
public static class MouseoverSprenInfoPatch {
    private static readonly StringBuilder SprenInfoBuilder = new StringBuilder();

    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
        return new CodeMatcher(instructions)
            .End() // Go to the end
            .Advance(-1) // Go back one instruction to the last ret
            .Insert(
                new CodeInstruction(OpCodes.Ldloc_1), // Load num variable
                new CodeInstruction(
                    OpCodes.Call,
                    AccessTools.Method(typeof(MouseoverSprenInfoPatch), nameof(ShowSprenInfo))
                ) // Call our method
            )
            .InstructionEnumeration();
    }

    public static void ShowSprenInfo(float yOffset) {
        IntVec3 cell = UI.MouseCell();
        Map map = Find.CurrentMap;

        if (map == null || !cell.InBounds(map)) return;

        SprenInfoBuilder.Clear();
        List<string> sprenNames = [];

        // Check each enabled spren controller directly for active spren at this position
        foreach (BaseSprenController controller in SprenControllerRegistry.enabledControllers) {
            if (controller.activeSpawnInfo.Any(s => s.position.Equals(cell))) {
                sprenNames.Add(controller.GetLocalizedName());
            }
        }

        if (sprenNames.Count == 0) return;

        string sprenInfo = string.Join(", ", sprenNames);

        // Get BotLeft field using reflection
        Vector2 botLeft = (Vector2)AccessTools.Field(typeof(MouseoverReadout), "BotLeft").GetValue(null);

        // Add spren info to the mouseover readout using the same Y offset pattern as vanilla
        Widgets.Label(new Rect(botLeft.x, UI.screenHeight - botLeft.y - yOffset, 999f, 999f), sprenInfo);
    }
}