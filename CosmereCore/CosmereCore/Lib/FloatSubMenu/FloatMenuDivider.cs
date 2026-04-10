using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace FloatSubMenus {
    public class FloatMenuDivider : FloatMenuOption {
        private readonly string label;

        private readonly Traverse<float> widthField;
        private readonly Traverse<float> heightField;
        private readonly float labelWidth;
        private readonly Vector2 size;
        private const float HorizMargin   =   3f;
        private const float VertMargin    =   1f;
        private const float MinLineLength =  10f;
        private const float MinWidth      = 100f;
        private const float MaxTextWidth  = 300f - 2 * HorizMargin;

#if VERSION_1_3
        private static readonly Color ColorBGActive = new ColorInt(21, 25, 29).ToColor;
        private static readonly Color ColorTextDisabled = new Color(0.9f, 0.9f, 0.9f);
#endif

        public FloatMenuDivider(string label = null) : base(" ", NoAction) {
            this.label = label;

            var traverse = Traverse.Create(this);
            widthField  = traverse.Field<float>("cachedRequiredWidth");
            heightField = traverse.Field<float>("cachedRequiredHeight");

            GameFont font = Text.Font;
            Text.Font = GameFont.Tiny;
            labelWidth = Text.CalcSize(label).x;
            float height = Text.CalcHeight(label, MaxTextWidth);
            size.x = Mathf.Min(labelWidth + 2 * HorizMargin, MinWidth);
            size.y = 2 * VertMargin + Text.CalcHeight(Label, MaxTextWidth);
            Text.Font = font;
            SetupSize();
        }

        private static void NoAction() {}

        private void SetupSize() {
            widthField.Value = size.x;
            heightField.Value = size.y;
        }

        public override bool DoGUI(Rect rect, bool colonistOrdering, FloatMenu floatMenu) {
            SetupSize();

            Text.Font = GameFont.Tiny;
            if (tooltip.HasValue) {
                TooltipHandler.TipRegion(rect, tooltip.Value);
            }

            Color color = GUI.color;
            GUI.color = ColorBGActive * color;
            GUI.DrawTexture(rect, BaseContent.WhiteTex);
            GUI.color = ColorTextDisabled * color;
            Widgets.DrawAtlas(rect, TexUI.FloatMenuOptionBG);
            GUI.color = ColorTextDisabled * color * 0.75f;

            Rect inner = rect.ContractedBy(HorizMargin, VertMargin);
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(inner, label);
            Text.Anchor = TextAnchor.UpperLeft;

            float lineLength = inner.width - labelWidth - HorizMargin;
            if (lineLength > MinLineLength) {
                Widgets.DrawLineHorizontal(inner.x + labelWidth + HorizMargin,
                                           Mathf.Round(inner.y + 0.65f * inner.height),
                                           lineLength);
            }

            GUI.color = color;
            return false;
        }
    }
}
