using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace FloatSubMenus {
    public class FloatMenuSearch : FloatMenuOption {
        private const float Margin =   2f;
        private const float Width  = 240f;
        private const float Height = QuickSearchWidget.WidgetHeight + 2 * Margin;

        private readonly FloatMenuFilter filter = new FloatMenuFilter();
        private readonly QuickSearchWidget search = new QuickSearchWidget();
        private readonly Traverse<float> widthField;
        private readonly Traverse<float> heightField;
        private readonly bool subMenus;

        public FloatMenuSearch(bool subMenus = false) : base(" ", () => {}) {
            extraPartOnGUI = ExtraPart;
            extraPartWidth = Width;
#if !VERSION_1_3
            extraPartRightJustified = true;
#endif
            action = OnClicked;

            this.subMenus = subMenus;

            var traverse = Traverse.Create(this);
            widthField  = traverse.Field<float>("cachedRequiredWidth");
            heightField = traverse.Field<float>("cachedRequiredHeight");
        }

        private void OnClicked() {
#if !VERSION_1_3
            search.Focus();
#endif
        }

        private void Filter() {
            filter.Filter(predicate: x => x == this || search.filter.Matches(x.Label),
                          reset:     !search.filter.Active,
                          recursive: subMenus);
            search.noResultsMatched = filter.Count <= 1;
        }

        private bool ExtraPart(Rect rect) {
            rect.height = Height;
            search.OnGUI(rect.ContractedBy(Margin), Filter);
            return false;
        }

        public override bool DoGUI(Rect rect, bool colonistOrdering, FloatMenu floatMenu) {
            filter.Update(floatMenu, Filter, AfterSizeMode);
            extraPartWidth = rect.width;
            base.DoGUI(rect, colonistOrdering, floatMenu);
            return false;
        }

        private void AfterSizeMode() {
            widthField.Value = Width;
            heightField.Value = Height;
        }
    }
}
