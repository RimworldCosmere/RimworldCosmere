using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Concord;
using Cosmere.System.Roshar.LesserSpren.SprenController;
using UnityEngine;
using Verse;

namespace Cosmere.System.Roshar.Patch.Spren;

[Patch]
public abstract class MouseoverSprenInfoPatch : MouseoverReadout {
    private static readonly StringBuilder SprenInfoBuilder = new StringBuilder();

    private static readonly FieldInfo? BotLeftField =
        typeof(MouseoverReadout).GetField("BotLeft", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);

    private static readonly MethodInfo ShowSprenInfoMethod =
        typeof(MouseoverSprenInfoPatch).GetMethod(
            nameof(ShowSprenInfo),
            BindingFlags.Static | BindingFlags.Public
        )!;

    [Inject(At.Transpiler, nameof(MouseoverReadoutOnGUI))]
    private static IEnumerable<CodeInstruction> AppendSprenReadout(IEnumerable<CodeInstruction> instructions) {
        return new CodeMatcher(instructions)
            .End() // Go to the end
            .Advance(-1) // Go back one instruction to the last ret
            .Insert(
                new CodeInstruction(OpCodes.Ldloc_1), // Load num variable
                new CodeInstruction(OpCodes.Call, ShowSprenInfoMethod) // Call our method
            )
            .InstructionEnumeration();
    }

    public static void ShowSprenInfo(float yOffset) {
        IntVec3 cell = Verse.UI.MouseCell();
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

        Vector2 botLeft = (Vector2)BotLeftField!.GetValue(null);

        // Add spren info to the mouseover readout using the same Y offset pattern as vanilla
        Widgets.Label(new Rect(botLeft.x, Verse.UI.screenHeight - botLeft.y - yOffset, 999f, 999f), sprenInfo);
    }
}
