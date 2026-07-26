using System;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace Cosmere.Core.Lib.FloatSubMenu;

public static class FloatSubMenuFactory {
    private const string Achtung_ID = "brrainz.achtung";
    private static readonly bool Achtung = LoadedModManager.RunningMods.Any(x => x.PackageId == Achtung_ID);
    private static readonly bool CompatMMM = Achtung;

    private static Action CompatSub(List<FloatMenuOption> subOption) {
        return () => subOption.OpenMenu();
    }

    public static FloatMenuOption CompatMMMCreate(
        string label,
        List<FloatMenuOption> subOptions,
        MenuOptionPriority priority = MenuOptionPriority.Default,
        Verse.Thing? revalidateClickTarget = null,
        float extraPartWidth = 0,
        Func<Rect, bool>? extraPartOnGUI = null,
        WorldObject? revalidateWorldClickTarget = null,
        bool playSelectionSound = true,
        int orderInPriority = 0
    ) {
        if (CompatMMM) {
            return new FloatMenuOption(label, CompatSub(subOptions), priority, null, revalidateClickTarget, extraPartWidth, extraPartOnGUI, revalidateWorldClickTarget, playSelectionSound, orderInPriority);
        }
        return new FloatSubMenu(label, subOptions, priority, revalidateClickTarget, extraPartWidth, extraPartOnGUI, revalidateWorldClickTarget, playSelectionSound, orderInPriority);
    }

    public static FloatMenuOption CompatMMMCreate(
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
    ) {
        if (CompatMMM) {
            return new FloatMenuOption(
                label,
                CompatSub(subOptions),
                shownItemForIcon,
                thingStyle,
                forceBasicStyle,
                priority,
                null,
                revalidateClickTarget,
                extraPartWidth,
                extraPartOnGUI,
                revalidateWorldClickTarget,
                playSelectionSound,
                orderInPriority,
                graphicIndexOverride
            );
        }
        return new FloatSubMenu(label, subOptions, shownItemForIcon, thingStyle, forceBasicStyle, priority, revalidateClickTarget, extraPartWidth, extraPartOnGUI, revalidateWorldClickTarget, playSelectionSound, orderInPriority, graphicIndexOverride);
    }

    public static FloatMenuOption CompatMMMCreate(
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
    ) {
        if (CompatMMM) {
            return new FloatMenuOption(
                label,
                CompatSub(subOptions),
                itemIcon,
                iconColor,
                priority,
                null,
                revalidateClickTarget,
                extraPartWidth,
                extraPartOnGUI,
                revalidateWorldClickTarget,
                playSelectionSound,
                orderInPriority,
                iconJustification,
                extraPartRightJustified
            );
        }
        return new FloatSubMenu(label, subOptions, itemIcon, iconColor, priority, revalidateClickTarget, extraPartWidth, extraPartOnGUI, revalidateWorldClickTarget, playSelectionSound, orderInPriority, iconJustification, extraPartRightJustified);
    }
}
