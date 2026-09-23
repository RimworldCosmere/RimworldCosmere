using System.Reflection;
using System.Reflection.Emit;
using Concord;
using RimWorld;
using Verse;

namespace Cosmere.Core.Patch;

[Patch(typeof(CharacterCardUtility))]
public static class CharacterCardWorldConnectionPatch {
    private static readonly FieldInfo StackElements = typeof(CharacterCardUtility).GetField(
        "tmpStackElements",
        BindingFlags.Static | BindingFlags.NonPublic
    )!;

    private static readonly MethodInfo AddHomeworld = typeof(CharacterCardWorldConnection).GetMethod(
        nameof(CharacterCardWorldConnection.AddHomeworldElement),
        BindingFlags.Static | BindingFlags.Public
    )!;

    [Inject(At.Transpiler, "DoTopStack")]
    private static IEnumerable<CodeInstruction> AddHomeworldToTopStack(IEnumerable<CodeInstruction> instructions) {
        bool foundXenotypeDrawer = false;
        bool inserted = false;

        foreach (CodeInstruction instruction in instructions) {
            yield return instruction;

            if (instruction.opcode == OpCodes.Ldftn && instruction.operand is MethodInfo drawer &&
                drawer.Name == "<DoTopStack>b__8") {
                foundXenotypeDrawer = true;
                continue;
            }

            if (!foundXenotypeDrawer || inserted || instruction.opcode != OpCodes.Callvirt ||
                instruction.operand is not MethodInfo called || called.Name != nameof(List<object>.Add)) {
                continue;
            }

            yield return new CodeInstruction(OpCodes.Ldsfld, StackElements);
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Call, AddHomeworld);
            inserted = true;
        }

        if (!inserted) Log.Warn("CharacterCardUtility.DoTopStack world Connection transpiler found no xenotype element.");
    }
}
