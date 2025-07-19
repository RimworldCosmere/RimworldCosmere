using System.Collections.Generic;
using CosmereRoshar.Comp.Thing;
using UnityEngine;
using Verse;

namespace CosmereRoshar.Dialog;

public interface IFilterableComp {
    List<ThingDef> allowedSpheres { get; }
    List<ThingDef> filterList { get; }
    List<GemSize> sizeFilterList { get; }
}

public class SphereFilter<T> : Window where T : ThingComp, IFilterableComp {
    private readonly T thing;
    private Vector2 scrollPosition;

    public SphereFilter(T comp) {
        thing = comp;
        forcePause = false;
        //absorbInputAroundWindow = true;
    }

    // You can adjust the size of the window as needed.
    public override Vector2 InitialSize => new Vector2(400f, 500f);

    private void AddCheckboxSpheres(int i, Rect viewRect) {
        ThingDef sphereDef = thing.allowedSpheres[i];
        Rect checkboxRect = new Rect(0, i * 30, viewRect.width, 30);

        // Determine if this sphere is currently enabled in the filter.
        bool currentlyAllowed = thing.filterList.Contains(sphereDef);

        // Copy the state into a local variable.
        bool flag = currentlyAllowed;

        // Pass the local variable by reference.
        Widgets.CheckboxLabeled(checkboxRect, sphereDef.label, ref flag);

        // If the checkbox value has changed, update the filter list accordingly.
        if (flag != currentlyAllowed) {
            if (flag) {
                thing.filterList.Add(sphereDef);
            } else {
                thing.filterList.Remove(sphereDef);
            }
        }
    }

    private void AddCheckboxGemSize(int i, Rect viewRect, GemSize size) {
        Rect checkboxRect = new Rect(0, i * 30, viewRect.width, 30);
        // Determine if this sphere is currently enabled in the filter.
        bool currentlyAllowed = thing.sizeFilterList.Contains(size);

        // Copy the state into a local variable.
        bool flag = currentlyAllowed;

        // Pass the local variable by reference.
        Widgets.CheckboxLabeled(checkboxRect, size.ToString(), ref flag);

        // If the checkbox value has changed, update the filter list accordingly.
        if (flag != currentlyAllowed) {
            if (flag) {
                thing.sizeFilterList.Add(size);
            } else {
                thing.sizeFilterList.Remove(size);
            }
        }
    }

    public override void DoWindowContents(Rect inRect) {
        // Draw a label for instructions.
        Widgets.Label(new Rect(inRect.x, inRect.y, inRect.width, 30), "Select allowed types:");

        // Define a scrollable area.
        Rect outRect = new Rect(inRect.x, inRect.y + 30, inRect.width, inRect.height + 165);
        Rect viewRect = new Rect(0, 0, inRect.width - 16, 300);
        Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);

        // Iterate through all allowed spheres from Props and create a checkbox for each.
        for (int i = 0; i < thing.allowedSpheres.Count; i++) {
            AddCheckboxSpheres(i, viewRect);
        }

        AddCheckboxGemSize(6, viewRect, GemSize.Chip);
        AddCheckboxGemSize(7, viewRect, GemSize.Mark);
        AddCheckboxGemSize(8, viewRect, GemSize.Broam);

        Rect sliderRect = new Rect(0, thing.allowedSpheres.Count * 30, viewRect.width, 30);
        Widgets.EndScrollView();

        // Optionally, add a close button.
        if (Widgets.ButtonText(new Rect(inRect.width - 100, inRect.height - 35, 100, 30), "Close")) {
            Close();
        }
    }
}