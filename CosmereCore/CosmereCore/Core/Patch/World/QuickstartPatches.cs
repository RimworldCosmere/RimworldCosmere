using System;
using System.Reflection;
using System.Reflection.Emit;
using Concord;
using Cosmere.Core.Quickstart;
using Verse;

namespace Cosmere.Core.Patch;

[Patch]
public abstract class QuickstartRootOnGUIPatch : Root {
    [Inject(At.Return, nameof(OnGUI))]
    private void AfterOnGUI() {
        Quickstarter.instance?.OnGUI();
    }
}

[Patch]
public abstract class QuickstartDebugButtonsPatch : DebugWindowsOpener {
    [Inject(At.Transpiler, "DrawButtons")]
    private static IEnumerable<CodeInstruction> DrawAdditionalButtons(IEnumerable<CodeInstruction> instructions) {
        FieldInfo? widgetRowField = typeof(DebugWindowsOpener).GetField(
            "widgetRow",
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public
        );

        // Local, not a field: Concord reruns transpilers on every recompose of the target,
        // so a static guard would suppress the insert on every run after the first.
        bool inserted = false;

        foreach (CodeInstruction inst in instructions) {
            // before "if (Current.ProgramState == ProgramState.Playing)"
            if (!inserted && widgetRowField != null && inst.opcode == OpCodes.Bne_Un_S) {
                yield return new CodeInstruction(OpCodes.Ldarg_0);
                yield return new CodeInstruction(OpCodes.Ldfld, widgetRowField);
                yield return new CodeInstruction(
                    OpCodes.Call,
                    ((Action<WidgetRow>)Quickstarter.DrawDebugToolbarButton).Method
                );
                inserted = true;
            }

            yield return inst;
        }

        // Reported here rather than from a startup callback: Patcher.Apply runs inside a queued
        // long event, so anything checking a flag from LongEventHandler.ExecuteWhenFinished reads
        // it before this transpiler has run.
        if (!inserted) {
            Logger.Warning("DebugWindowsOpener.DrawButtons transpiler found no `bne.un.s` to insert before.");
        }
    }
}
