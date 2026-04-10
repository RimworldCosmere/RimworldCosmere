using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace FloatSubMenus {
    public class FloatMenuToggleOption : FloatMenuOption {
        public Func<bool> checkOn;
        public Func<bool> checkDimmed;

        public FloatMenuToggleOption(string label,
                                     Action toggle,
                                     Func<bool> checkOn,
                                     Func<bool> checkDimmed,
                                     MenuOptionPriority priority = MenuOptionPriority.Default,
                                     Action<Rect> mouseoverGuiAction = null,
                                     Thing revalidateClickTarget = null,
                                     WorldObject revalidateWorldClickTarget = null,
                                     bool playSelectionSound = true,
                                     int orderInPriority = 0)
            : base(label,
                   toggle,
                   priority,
                   mouseoverGuiAction,
                   revalidateClickTarget,
                   20f,
                   null,
                   revalidateWorldClickTarget,
                   playSelectionSound,
                   orderInPriority)
            => Setup(checkOn, checkDimmed);

        public FloatMenuToggleOption(string label,
                                     Action toggle,
                                     Func<bool> checkOn,
                                     Func<bool> checkDimmed,
                                     ThingDef shownItemForIcon,
                                     ThingStyleDef thingStyle = null,
                                     bool forceBasicStyle = false,
                                     MenuOptionPriority priority = MenuOptionPriority.Default,
                                     Action<Rect> mouseoverGuiAction = null,
                                     Thing revalidateClickTarget = null,
                                     WorldObject revalidateWorldClickTarget = null,
                                     bool playSelectionSound = true,
                                     int orderInPriority = 0, int? 
                                     graphicIndexOverride = null)
            : base(label,
                   toggle,
                   shownItemForIcon,
#if !VERSION_1_3
                   thingStyle,
                   forceBasicStyle,
#endif
                   priority,
                   mouseoverGuiAction,
                   revalidateClickTarget,
                   20f,
                   null,
                   revalidateWorldClickTarget,
                   playSelectionSound,
                   orderInPriority
#if !VERSION_1_3
                   , graphicIndexOverride
#endif
                  )
            => Setup(checkOn, checkDimmed);

        public FloatMenuToggleOption(string label,
                                     Action toggle,
                                     Func<bool> checkOn,
                                     Func<bool> checkDimmed,
                                     Texture2D itemIcon,
                                     Color iconColor,
                                     MenuOptionPriority priority = MenuOptionPriority.Default,
                                     Action<Rect> mouseoverGuiAction = null,
                                     Thing revalidateClickTarget = null,
                                     WorldObject revalidateWorldClickTarget = null,
                                     bool playSelectionSound = true,
                                     int orderInPriority = 0,
                                     HorizontalJustification iconJustification = HorizontalJustification.Left)
            : base(label,
                   toggle,
                   itemIcon,
                   iconColor,
                   priority,
                   mouseoverGuiAction,
                   revalidateClickTarget,
                   20f,
                   null,
                   revalidateWorldClickTarget,
                   playSelectionSound,
                   orderInPriority
#if !VERSION_1_3
                   , iconJustification
#endif
                   )
            => Setup(checkOn, checkDimmed);

        private void Setup(Func<bool> checkOn, Func<bool> checkDimmed) {
            this.checkOn = checkOn ?? True;
            this.checkDimmed = checkDimmed ?? False;

            extraPartOnGUI = DrawCheck;
#if !VERSION_1_3
            extraPartRightJustified = true;
#endif
        }

        private static bool True() => true;
        private static bool False() => false;

        private bool DrawCheck(Rect r) {
#if VERSION_1_3
            Color color = GUI.color;
            bool disabled = checkDimmed();
            if (disabled) {
                GUI.color = Widgets.InactiveColor;
            }
            GUI.DrawTexture(new Rect(r.x, r.y, 20f, 20f),
                            checkOn() ? Widgets.CheckboxOnTex : Widgets.CheckboxOffTex);
            if (disabled) {
                GUI.color = color;
            }
#else
            Widgets.CheckboxDraw(r.x, r.y + (r.height - 20f) / 2, checkOn(), checkDimmed(), 20f);
#endif
            return false;
        }

        public override bool DoGUI(Rect rect, bool colonistOrdering, FloatMenu floatMenu) {
            base.DoGUI(rect, colonistOrdering, null);
            return false;
        }
    }
}
