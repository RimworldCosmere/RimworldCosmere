using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace FloatSubMenus;

public class FloatMenuSearch : FloatMenuOption {
    private const float Margin = 2f;
    private const float Width = 240f;
    private const float Height = QuickSearchWidget.WidgetHeight + 2 * Margin;

    private readonly FloatMenuFilter filter = new FloatMenuFilter();
    private readonly Traverse<float> heightField;
    private readonly QuickSearchWidget search = new QuickSearchWidget();
    private readonly bool subMenus;
    private readonly Traverse<float> widthField;

    public FloatMenuSearch(bool subMenus = false) : base(" ", () => { }) {
        extraPartOnGUI = ExtraPart;
        extraPartWidth = Width;
#if !VERSION_1_3
        extraPartRightJustified = true;
#endif
        action = OnClicked;

        this.subMenus = subMenus;

        Traverse traverse = Traverse.Create(this);
        widthField = traverse.Field<float>("cachedRequiredWidth");
        heightField = traverse.Field<float>("cachedRequiredHeight");
    }

    private void OnClicked() {
#if !VERSION_1_3
        search.Focus();
#endif
    }

    private void Filter() {
        filter.Filter(
            x => x == this || search.filter.Matches(x.Label),
            !search.filter.Active,
            subMenus
        );
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