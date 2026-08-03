using System;
using Concord;
using Cosmere.Core.Quickstart;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.Core.Patch;

/// <summary>
///     Swaps the main menu's dev quicktest entry for the picker, and opens the picker outright
///     when the -quicktest arg was claimed at startup. Done by rewriting the option's action
///     rather than transpiling MainMenuDrawer, so a vanilla layout change to the menu cannot
///     silently drop the hook.
/// </summary>
[Patch(typeof(OptionListingUtility))]
public static class QuicktestMainMenuOptionPatch {
    private static readonly Action OpenPicker = () => Find.WindowStack.Add(new Dialog_QuicktestPicker());

    [Inject(At.Head, nameof(OptionListingUtility.DrawOptionListing))]
    private static void BeforeDrawOptionListing(Rect rect, List<ListableOption> optList) {
        if (!Prefs.DevMode || Current.ProgramState != ProgramState.Entry) return;

        string label = "DevQuickTest".Translate();
        for (int i = 0; i < optList.Count; i++) {
            if (optList[i].label != label || optList[i].action == OpenPicker) continue;

            optList[i].action = OpenPicker;
        }
    }
}
