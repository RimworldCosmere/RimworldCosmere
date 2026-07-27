using System;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace Cosmere.Core.Lib.FloatSubMenu;

public class FloatSubMenu : FloatMenuOption {
    private const float ArrowExtraWidth = 16f;
    private const float ArrowOffset = 4f;
    private const float ArrowAlpha = 0.6f;

    private static readonly Vector2 MenuOffset = new Vector2(-1f, 0f);
    private readonly Func<Rect, bool>? extraPartOnGUIOuter;
    private readonly float extraPartWidthOuter;

    private readonly List<FloatMenuOption> subOptions;
    private Rect extraGUIRect = new Rect(-1f, -1f, 0f, 0f);
    private FloatMenuFilter? filter;
    private Action? parentCloseCallback;
    private bool parentSetUp;

    private FloatSubMenuInner? subMenu;
    internal bool subMenuOptionChosen;
    private bool subOptionsInitialized;

    public FloatSubMenu(
        string label,
        List<FloatMenuOption> subOptions,
        MenuOptionPriority priority = MenuOptionPriority.Default,
        Verse.Thing? revalidateClickTarget = null,
        float extraPartWidth = 0,
        Func<Rect, bool>? extraPartOnGUI = null,
        WorldObject? revalidateWorldClickTarget = null,
        bool playSelectionSound = true,
        int orderInPriority = 0
    )
        : base(
            label,
            NoAction,
            priority,
            null,
            revalidateClickTarget,
            extraPartWidth + ArrowExtraWidth,
            null,
            revalidateWorldClickTarget,
            playSelectionSound,
            orderInPriority
        ) {
        this.subOptions = subOptions;
        extraPartOnGUIOuter = extraPartOnGUI;
        extraPartWidthOuter = extraPartWidth;
        this.extraPartOnGUI = DrawExtra;
    }

    public FloatSubMenu(
        string label,
        List<FloatMenuOption> subOptions,
        ThingDef shownItemForIcon,
        ThingStyleDef? thingStyle = null,
        bool forceBasicStyle = false,
        MenuOptionPriority priority = MenuOptionPriority.Default,
        Verse.Thing? revalidateClickTarget = null,
        float extraPartWidth = 0,
        Func<Rect, bool>? extraPartOnGUI = null,
        WorldObject? revalidateWorldClickTarget = null,
        bool playSelectionSound = true,
        int orderInPriority = 0,
        int? graphicIndexOverride = null
    )
        : base(
            label,
            NoAction,
            shownItemForIcon,
            thingStyle,
            forceBasicStyle,
            priority,
            null,
            revalidateClickTarget,
            extraPartWidth + ArrowExtraWidth,
            null,
            revalidateWorldClickTarget,
            playSelectionSound,
            orderInPriority,
            graphicIndexOverride
        ) {
        this.subOptions = subOptions;
        extraPartOnGUIOuter = extraPartOnGUI;
        extraPartWidthOuter = extraPartWidth;
        this.extraPartOnGUI = DrawExtra;
    }

    public FloatSubMenu(
        string label,
        List<FloatMenuOption> subOptions,
        Texture2D itemIcon,
        Color iconColor,
        MenuOptionPriority priority = MenuOptionPriority.Default,
        Verse.Thing? revalidateClickTarget = null,
        float extraPartWidth = 0,
        Func<Rect, bool>? extraPartOnGUI = null,
        WorldObject? revalidateWorldClickTarget = null,
        bool playSelectionSound = true,
        int orderInPriority = 0,
        HorizontalJustification iconJustification = HorizontalJustification.Left,
        bool extraPartRightJustified = false
    )
        : base(
            label,
            NoAction,
            itemIcon,
            iconColor,
            priority,
            null,
            revalidateClickTarget,
            extraPartWidth + ArrowExtraWidth,
            null,
            revalidateWorldClickTarget,
            playSelectionSound,
            orderInPriority,
            iconJustification,
            extraPartRightJustified
        ) {
        this.subOptions = subOptions;
        extraPartOnGUIOuter = extraPartOnGUI;
        extraPartWidthOuter = extraPartWidth;
        this.extraPartOnGUI = DrawExtra;
    }

    public bool Open => subMenu != null && subMenu.IsOpen;

    public List<FloatMenuOption> Options {
        get {
            if (!subOptionsInitialized) {
                FloatMenuSizeMode mode = subOptions.Count > 60 ? FloatMenuSizeMode.Tiny : FloatMenuSizeMode.Normal;
                subOptions.ForEach(o => o.SetSizeMode(mode));
                subOptions.Sort(OptionPriorityCmp);
                subOptionsInitialized = true;
            }

            return subOptions;
        }
    }

    private FloatMenuFilter Filter => filter ?? (filter = new FloatMenuFilter());

    private static void NoAction() { }

    public bool DrawExtra(Rect rect) {
        extraGUIRect = rect.RightPartPixels(ArrowExtraWidth);
        extraPartOnGUIOuter?.Invoke(rect.LeftPartPixels(extraPartWidthOuter));
        return false;
    }

    private static void DrawArrow(Rect rect) {
        rect.width -= ArrowOffset;

        GameFont font = Text.Font;
        TextAnchor anchor = Text.Anchor;
        Color color = GUI.color;

        Text.Font = GameFont.Tiny;
        Text.Anchor = TextAnchor.MiddleRight;
        GUI.color = new Color(color.r, color.g, color.b, color.a * ArrowAlpha);
        Widgets.Label(rect, ">");

        Text.Font = font;
        Text.Anchor = anchor;
        GUI.color = color;
    }

    public override bool DoGUI(Rect rect, bool colonistOrdering, FloatMenu floatMenu) {
        if (floatMenu == null) {
            return base.DoGUI(rect, colonistOrdering, floatMenu);
        }

        SetupParent(floatMenu);

        MouseArea mouseArea = FindMouseArea(rect, floatMenu);
        bool inExtraSpace = Mouse.IsOver(extraGUIRect);
        if (mouseArea == (Open ? MouseArea.Menu : MouseArea.Option)) {
            MouseAction(rect, !Open, floatMenu);
        }

        // When the sub menu is open, let super implementation know only about
        // mouse movement inside parent menu. Also do not let it know if the
        // mouse is in our extraPartOnGUI space, since it does not highlight
        // option then.
        Vector2 mouse = Event.current.mousePosition;
        if (Open && mouseArea == MouseArea.Outside || inExtraSpace) {
            Event.current.mousePosition = new Vector2(rect.x + 2f, rect.y + 2f);
        }

        base.DoGUI(rect, colonistOrdering, floatMenu);
        DrawArrow(rect);

        // Reset mouse position
        Event.current.mousePosition = mouse;

        if (subMenuOptionChosen) {
            floatMenu.PreOptionChosen(this);
        }

        return subMenuOptionChosen;
    }

    internal bool AnyMatches(Func<FloatMenuOption, bool> predicate, bool recursive) {
        return subOptions.Any(x => predicate(x) || (recursive && SubAnyMatches(x, predicate)));
    }

    private bool SubAnyMatches(FloatMenuOption opt, Func<FloatMenuOption, bool> predicate) {
        return opt is FloatSubMenu sub && sub.AnyMatches(predicate, true);
    }

    internal void FilterSubMenu(Func<FloatMenuOption, bool> predicate, bool reset, bool recursive) {
        Filter.Filter(predicate, reset, recursive);
    }

    internal void UpdateFilter(FloatMenu floatMenu) {
        filter?.Update(floatMenu);
    }

    private static int OptionPriorityCmp(FloatMenuOption a, FloatMenuOption b) {
        int res = (int)b.Priority - (int)a.Priority;
        return res != 0 ? res : b.orderInPriority - a.orderInPriority;
    }

    internal static bool ShouldReplaceDistanceFor(FloatMenu menu, ref float distance) {
        OpenMenuSet? set = OpenMenuSet.For(menu);
        if (set != null) {
            distance = set.MinDistance;
            return true;
        }

        return false;
    }

    private void SetupParent(FloatMenu parent) {
        if (!(parentSetUp || parent is FloatSubMenuInner)) {
            parentSetUp = true;
            parentCloseCallback = parent.onCloseCallback;
            parent.onCloseCallback = OnParentClose;
        }
    }

    private void OnParentClose() {
        CloseSubMenu();
        parentCloseCallback?.Invoke();
    }

    private MouseArea FindMouseArea(Rect option, FloatMenu menu) {
        option.height--;
        if (Mouse.IsOver(option)) {
            return MouseArea.Option;
        }

        return Mouse.IsOver(menu.windowRect.AtZero()) ? MouseArea.Menu : MouseArea.Outside;
    }

    private void MouseAction(Rect rect, bool enter, FloatMenu parentMenu) {
        if (enter) {
            Vector2 localPos = new Vector2(rect.xMax, rect.yMin) + MenuOffset;
            OpenSubMenu(parentMenu, localPos);
        } else {
            CloseSubMenu();
        }
    }

    private void OpenSubMenu(FloatMenu parentMenu, Vector2 localPos) {
        if (!Open) {
            Vector2 mouse = Event.current.mousePosition;
            Vector2 offset = localPos - mouse;
            SoundDef sound = RimWorld.SoundDefOf.FloatMenu_Open;
            RimWorld.SoundDefOf.FloatMenu_Open = null;
            subMenu = new FloatSubMenuInner(
                this,
                subOptions,
                offset,
                parentMenu.vanishIfMouseDistant
            );
            RimWorld.SoundDefOf.FloatMenu_Open = sound;
            subOptionsInitialized = true;
            Find.WindowStack.Add(subMenu);
            OpenMenuSet.Open(parentMenu, subMenu);
        }
    }

    internal void CloseSubMenu() {
        if (Open) {
            Find.WindowStack.TryRemove(subMenu, false);
            subMenu = null;
        }
    }

    private enum MouseArea {
        Option,
        Menu,
        Outside,
    }
}
