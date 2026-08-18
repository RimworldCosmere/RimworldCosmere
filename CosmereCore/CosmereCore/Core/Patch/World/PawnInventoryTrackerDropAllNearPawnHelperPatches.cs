using System;
using System.Reflection;
using System.Reflection.Emit;
using Concord;
using Verse;
using ThingUtility = Cosmere.Core.Util.ThingUtility;

namespace Cosmere.Core.Patch;

[Patch]
public abstract class PawnInventoryTrackerDropAllNearPawnHelperPatch : Pawn_InventoryTracker {
    protected PawnInventoryTrackerDropAllNearPawnHelperPatch(Pawn pawn) : base(pawn) { }

    [Inject(
        At.Transpiler,
        "DropAllNearPawnHelper",
        parameterTypes: [typeof(IntVec3), typeof(bool), typeof(bool), typeof(bool)]
    )]
    private static IEnumerable<CodeInstruction> FilterDroppedThings(IEnumerable<CodeInstruction> instructions) {
        MethodInfo? addRange = typeof(List<Verse.Thing>).GetMethod(
            nameof(List<Verse.Thing>.AddRange),
            [typeof(IEnumerable<Verse.Thing>)]
        );
        MethodInfo? shouldDrop = typeof(ThingUtility).GetMethod(
            nameof(ThingUtility.ShouldDrop),
            BindingFlags.Public | BindingFlags.Static
        );
        ConstructorInfo? funcCtor = typeof(Func<Verse.Thing, bool>).GetConstructor([typeof(object), typeof(IntPtr)]);

        MethodInfo where = typeof(Enumerable)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .First(m =>
                m.Name == "Where" &&
                m.GetParameters().Length == 2 &&
                m.GetParameters()[1].ParameterType.GetGenericTypeDefinition() == typeof(Func<,>)
            )
            .MakeGenericMethod(typeof(Verse.Thing));

        foreach (CodeInstruction? instruction in instructions) {
            // Find: list.AddRange(arg)
            if (instruction.Is(OpCodes.Call, addRange) || instruction.Is(OpCodes.Callvirt, addRange)) {
                // Transform arg into arg.Where(ShouldDrop) before the AddRange call.
                yield return new CodeInstruction(OpCodes.Ldnull);
                yield return new CodeInstruction(OpCodes.Ldftn, shouldDrop);
                yield return new CodeInstruction(OpCodes.Newobj, funcCtor);
                yield return new CodeInstruction(OpCodes.Call, where);

                // Now call AddRange on the filtered list
                yield return instruction;
            } else {
                yield return instruction;
            }
        }
    }
}
