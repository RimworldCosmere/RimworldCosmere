using System;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace Cosmere.Core.Lib.FloatSubMenu;

public class FloatMenuToggleOption : FloatMenuOption {
    public Func<bool> checkDimmed = null!;
    public Func<bool> checkOn = null!;

    public FloatMenuToggleOption(
        string label,
        Action toggle,
        Func<bool> checkOn,
        Func<bool> checkDimmed,
        MenuOptionPriority priority = MenuOptionPriority.Default,
        Action<Rect>? mouseoverGuiAction = null,
        Verse.Thing? revalidateClickTarget = null,
        WorldObject? revalidateWorldClickTarget = null,
        bool playSelectionSound = true,
        int orderInPriority = 0
    )
        : base(
            label,
            toggle,
            priority,
            mouseoverGuiAction,
            revalidateClickTarget,
            20f,
            null,
            revalidateWorldClickTarget,
            playSelectionSound,
            orderInPriority
        ) {
        Setup(checkOn, checkDimmed);
    }

    public FloatMenuToggleOption(
        string label,
        Action toggle,
        Func<bool> checkOn,
        Func<bool> checkDimmed,
        ThingDef shownItemForIcon,
        ThingStyleDef? thingStyle = null,
        bool forceBasicStyle = false,
        MenuOptionPriority priority = MenuOptionPriority.Default,
        Action<Rect>? mouseoverGuiAction = null,
        Verse.Thing? revalidateClickTarget = null,
        WorldObject? revalidateWorldClickTarget = null,
        bool playSelectionSound = true,
        int orderInPriority = 0,
        int?
            graphicIndexOverride = null
    )
        : base(
            label,
            toggle,
            shownItemForIcon,
            thingStyle,
            forceBasicStyle,
            priority,
            mouseoverGuiAction,
            revalidateClickTarget,
            20f,
            null,
            revalidateWorldClickTarget,
            playSelectionSound,
            orderInPriority,
            graphicIndexOverride
        )
        => Setup(checkOn, checkDimmed);

    public FloatMenuToggleOption(
        string label,
        Action toggle,
        Func<bool> checkOn,
        Func<bool> checkDimmed,
        Texture2D itemIcon,
        Color iconColor,
        MenuOptionPriority priority = MenuOptionPriority.Default,
        Action<Rect>? mouseoverGuiAction = null,
        Verse.Thing? revalidateClickTarget = null,
        WorldObject? revalidateWorldClickTarget = null,
        bool playSelectionSound = true,
        int orderInPriority = 0,
        HorizontalJustification iconJustification = HorizontalJustification.Left
    )
        : base(
            label,
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
            orderInPriority,
            iconJustification
        )
        => Setup(checkOn, checkDimmed);

    private void Setup(Func<bool> checkOn, Func<bool> checkDimmed) {
        this.checkOn = checkOn ?? True;
        this.checkDimmed = checkDimmed ?? False;

        extraPartOnGUI = DrawCheck;
        extraPartRightJustified = true;
    }

    private static bool True() {
        return true;
    }

    private static bool False() {
        return false;
    }

    private bool DrawCheck(Rect r) {
        Widgets.CheckboxDraw(r.x, r.y + (r.height - 20f) / 2, checkOn(), checkDimmed(), 20f);
        return false;
    }

    public override bool DoGUI(Rect rect, bool colonistOrdering, FloatMenu floatMenu) {
        base.DoGUI(rect, colonistOrdering, null);
        return false;
    }
}
