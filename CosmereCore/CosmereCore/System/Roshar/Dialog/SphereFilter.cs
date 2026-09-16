using Cosmere.System.Roshar.Comp.Fabrials;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Cosmere.System.Roshar.Dialog;

public class SphereFilter<T> : Window
    where T : ThingComp, IFilterableComp {
    private static readonly Color HeaderColor = new Color(0.86f, 0.90f, 0.93f);
    private static readonly Color EmptyColor = new Color(0.62f, 0.66f, 0.69f);

    private readonly T thing;
    private Vector2 scrollPosition;

    public SphereFilter(T comp) {
        thing = comp;
        forcePause = false;
        doCloseX = true;
    }

    public override Vector2 InitialSize => new Vector2(400f, 500f);

    public static Rect ScrollOuterRect(Rect inRect) {
        return new Rect(
            inRect.x,
            SphereFilterLayout.ScrollY(inRect.y),
            inRect.width,
            SphereFilterLayout.ScrollHeight(inRect.height)
        );
    }

    public static Rect ScrollViewRect(Rect inRect, int sphereCount) {
        return new Rect(
            0f,
            0f,
            SphereFilterLayout.ViewWidth(inRect.width),
            SphereFilterLayout.ViewHeight(sphereCount)
        );
    }

    public static Rect CloseButtonRect(Rect inRect) {
        return new Rect(
            SphereFilterLayout.CloseX(inRect.x, inRect.width),
            SphereFilterLayout.CloseY(inRect.y, inRect.height),
            SphereFilterLayout.CloseWidth,
            SphereFilterLayout.CloseHeight
        );
    }

    public override void DoWindowContents(Rect inRect) {
        Rect headerRect = new Rect(inRect.x, inRect.y, inRect.width, SphereFilterLayout.HeaderHeight);
        using (new TextBlock(GameFont.Medium, TextAnchor.MiddleLeft, HeaderColor)) {
            Widgets.Label(headerRect, "CRO_SphereFilter_Title".Translate());
        }

        Rect outRect = ScrollOuterRect(inRect);
        int count = thing.AllowedSpheres.Count;

        if (count == 0) {
            using (new TextBlock(GameFont.Small, TextAnchor.MiddleCenter, EmptyColor)) {
                Widgets.Label(outRect, "CRO_SphereFilter_Empty".Translate());
            }
        } else {
            Rect viewRect = ScrollViewRect(inRect, count);
            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);

            for (int i = 0; i < count; i++) {
                DrawSphereRow(i, viewRect);
            }

            Widgets.EndScrollView();
        }

        Rect closeRect = CloseButtonRect(inRect);
        TooltipHandler.TipRegion(closeRect, "CRO_SphereFilter_Close_Tip".Translate());
        MouseoverSounds.DoRegion(closeRect);

        if (Widgets.ButtonText(closeRect, "CRO_SphereFilter_Close".Translate())) {
            Close();
        }
    }

    private void DrawSphereRow(int i, Rect viewRect) {
        ThingDef sphereDef = thing.AllowedSpheres[i];
        Rect rowRect = new Rect(0f, i * SphereFilterLayout.RowHeight, viewRect.width, SphereFilterLayout.RowHeight);

        Widgets.DrawHighlightIfMouseover(rowRect);
        TooltipHandler.TipRegion(rowRect, "CRO_SphereFilter_Row_Tip".Translate(sphereDef.label.Named("SPHERE")));
        MouseoverSounds.DoRegion(rowRect);

        bool allowed = thing.FilterList.Contains(sphereDef);
        bool toggled = allowed;

        Widgets.CheckboxLabeled(rowRect, sphereDef.LabelCap, ref toggled);

        if (toggled == allowed) {
            return;
        }

        if (toggled) {
            thing.FilterList.Add(sphereDef);
        } else {
            thing.FilterList.Remove(sphereDef);
        }
    }
}
