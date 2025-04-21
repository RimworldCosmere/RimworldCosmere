#nullable disable
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace CosmereFramework.Menu {
    public class FloatMenuStateOption : FloatMenuOption {
        private int currentState;
        private Dictionary<int, Texture2D> stateIcons;

        public FloatMenuStateOption(
            string label,
            string tooltip,
            Action toggle,
            int currentState,
            Dictionary<int, Texture2D> stateIcons,
            MenuOptionPriority priority = MenuOptionPriority.Default)
            : base(label, toggle, priority, null, null, 20f) {
            Setup(tooltip, currentState, stateIcons);
        }

        public FloatMenuStateOption(
            string label,
            string tooltip,
            Action toggle,
            int currentState,
            Dictionary<int, Texture2D> stateIcons,
            Texture2D itemIcon,
            Color iconColor,
            MenuOptionPriority priority = MenuOptionPriority.Default)
            : base(label, toggle, itemIcon, iconColor, priority, null, null, 20f) {
            Setup(tooltip, currentState, stateIcons);
        }

        private void Setup(string tooltip, int currentState, Dictionary<int, Texture2D> stateIcons) {
            this.tooltip = tooltip;
            this.currentState = currentState;
            this.stateIcons = stateIcons;

            extraPartOnGUI = DrawState;
            extraPartRightJustified = true;
        }

        private bool DrawState(Rect rect) {
            Log.Verbose("DrawState triggered");
            stateIcons.TryGetValue(currentState, out var icon);

            rect.y += 20f;
            Widgets.DrawTextureFitted(
                rect,
                icon ?? BaseContent.BadTex,
                1f
            );

            return false;
        }
    }
}