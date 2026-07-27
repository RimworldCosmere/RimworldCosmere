using Cosmere.System.Roshar.Comp.Fabrials;
using UnityEngine;
using Verse;

namespace Cosmere.System.Roshar.Dialog;

public class SphereFilter<T> : Window
    where T : ThingComp, IFilterableComp {
    private readonly T thing;
    private Vector2 scrollPosition;

    public SphereFilter(T comp) {
        thing = comp;
        forcePause = false;
    }

    public override Vector2 InitialSize => new Vector2(400f, 500f);

    private void AddCheckboxSpheres(int i, Rect viewRect) {
        ThingDef sphereDef = thing.AllowedSpheres[i];
        Rect checkboxRect = new Rect(0, i * 30, viewRect.width, 30);

        bool currentlyAllowed = thing.FilterList.Contains(sphereDef);
        bool flag = currentlyAllowed;

        Widgets.CheckboxLabeled(checkboxRect, sphereDef.label, ref flag);

        if (flag != currentlyAllowed) {
            if (flag) {
                thing.FilterList.Add(sphereDef);
            } else {
                thing.FilterList.Remove(sphereDef);
            }
        }
    }

    public override void DoWindowContents(Rect inRect) {
        Widgets.Label(new Rect(inRect.x, inRect.y, inRect.width, 30), "Select allowed types:");

        Rect outRect = new Rect(inRect.x, inRect.y + 30, inRect.width, inRect.height + 165);
        Rect viewRect = new Rect(0, 0, inRect.width - 16, 300);
        Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);

        for (int i = 0; i < thing.AllowedSpheres.Count; i++) {
            AddCheckboxSpheres(i, viewRect);
        }

        Widgets.EndScrollView();

        if (Widgets.ButtonText(new Rect(inRect.width - 100, inRect.height - 35, 100, 30), "Close")) {
            Close();
        }
    }
}
