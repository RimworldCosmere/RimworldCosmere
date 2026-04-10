using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace FloatSubMenus {
    public static class ExtensionMethods {
        public static FloatMenuOption MenuOption(this Pawn pawn, Action action, bool shortName = true) 
            => new FloatMenuOption(label: shortName ? pawn.LabelShortCap : pawn.LabelCap,
                                  action: action,
                                iconTex: null,
                               iconColor: Color.white,
                          extraPartWidth: 30f,
                          extraPartOnGUI: r => DrawPawn(r, pawn)
#if !VERSION_1_3
               , extraPartRightJustified: true
#endif
                );

        public static FloatMenuOption MenuOption(this Pawn pawn, Action<Pawn> action, bool shortName = true) 
            => pawn.MenuOption(() => action(pawn), shortName);

        private static bool DrawPawn(Rect r, Pawn p) {
            Widgets.ThingIcon(r.ExpandedBy(6f), p);
            return false;
        }

        public static void OpenMenu(this IEnumerable<FloatMenuOption> menu)
            => Find.WindowStack.Add(new FloatMenu(menu.ToList()));

        public static void OpenMenu(this List<FloatMenuOption> menu)
            => Find.WindowStack.Add(new FloatMenu(menu));

        public static void OpenMenu(this FloatMenu menu)
            => Find.WindowStack.Add(menu);
    }
}
