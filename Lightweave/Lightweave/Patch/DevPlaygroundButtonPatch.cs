using System;
using System.Collections.Generic;
using System.Reflection;
using Cosmere.Lightweave.Input;
using Cosmere.Lightweave.Playground;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Settings;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.Lightweave.Patch;

[HarmonyPatch]
public static class DevPlaygroundButtonPatch {
    private const float ButtonHeight = 28f;
    private const float ButtonWidth = 220f;
    private const float Margin = 8f;

    private static readonly Guid RootId = Guid.NewGuid();

    public static IEnumerable<MethodBase> TargetMethods() {
        yield return AccessTools.Method(typeof(UIRoot_Entry), nameof(UIRoot.UIRootOnGUI));
        yield return AccessTools.Method(typeof(UIRoot_Play), nameof(UIRoot.UIRootOnGUI));
    }

    public static void Postfix() {
        if (!Prefs.DevMode) {
            return;
        }

        if (Find.WindowStack == null) {
            return;
        }

        if (Current.ProgramState == ProgramState.Entry
            && LightweaveMod.Settings != null
            && LightweaveMod.Settings.RedesignMainMenu) {
            return;
        }

        Rect rect = new Rect(
            UI.screenWidth - ButtonWidth - Margin,
            UI.screenHeight - ButtonHeight - Margin,
            ButtonWidth,
            ButtonHeight
        );

        LightweaveRoot.Render(rect, RootId, BuildButton);
    }

    

    private static LightweaveNode BuildButton() {
        return Button.Create(
            label: "CL_DevButton_Playground".Translate(),
            onClick: OpenPlayground,
            variant: ButtonVariant.Primary,
            fullWidth: true
        );
    }

    public static void OpenPlayground() {
        if (Find.WindowStack.IsOpen<LightweavePlayground>()) {
            return;
        }

        Find.WindowStack.Add(new LightweavePlayground());
    }
}
