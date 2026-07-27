using System;
using UnityEngine;
using Verse;

namespace Cosmere.Core.Lib.FloatSubMenu;

public static class ExtensionMethods {
    public static FloatMenuOption MenuOption(this Pawn pawn, Action action, bool shortName = true)
        => new FloatMenuOption(
            shortName ? pawn.LabelShortCap : pawn.LabelCap,
            action,
            null,
            Color.white,
            extraPartWidth: 30f,
            extraPartOnGUI: r => DrawPawn(r, pawn),
            extraPartRightJustified: true
        );

    public static FloatMenuOption MenuOption(this Pawn pawn, Action<Pawn> action, bool shortName = true) {
        return pawn.MenuOption(() => action(pawn), shortName);
    }

    private static bool DrawPawn(Rect r, Pawn p) {
        Widgets.ThingIcon(r.ExpandedBy(6f), p);
        return false;
    }

    public static void OpenMenu(this IEnumerable<FloatMenuOption> menu) {
        Find.WindowStack.Add(new FloatMenu(menu.ToList()));
    }

    public static void OpenMenu(this List<FloatMenuOption> menu) {
        Find.WindowStack.Add(new FloatMenu(menu));
    }

    public static void OpenMenu(this FloatMenu menu) {
        Find.WindowStack.Add(menu);
    }
}
