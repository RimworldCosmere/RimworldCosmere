using System.Collections.Generic;
using System.Linq;
using CosmereRoshar.Comp.Apparel;
using CosmereRoshar.Comp.Thing;
using RimWorld;
using UnityEngine;
using Verse;

namespace CosmereRoshar.ITabs;

public class TabSpherePouch : ITab {
    private static readonly Vector2 WinSize = new Vector2(300f, 480f);
    private Vector2 scrollPosition = Vector2.zero;

    public TabSpherePouch() {
        size = WinSize;
        labelKey = "whtwl_TabSpherePouch"; // Localization key
    }


    public override bool IsVisible {
        get {
            if (selPawn == null) return false;

            // Check if the pawn has a Sphere Pouch equipped 
            return selPawn.apparel?.WornApparel?.Any(a => a.TryGetComp<CompSpherePouch>() != null) ?? false;
        }
    }

    private Pawn selPawn => SelThing as Pawn;

    private CompSpherePouch pouchSpheres {
        get {
            return selPawn.apparel?.WornApparel.Select(a => a.GetComp<CompSpherePouch>())
                .FirstOrDefault(comp => comp != null);
        }
    }

    protected override void FillTab() {
        if (pouchSpheres == null) return;

        List<Thing> pouchContents = pouchSpheres.storedSpheres;

        Rect rect = new Rect(0f, 0f, WinSize.x, WinSize.y).ContractedBy(10f);
        Widgets.BeginGroup(rect);

        // Define the list area
        Rect scrollRect = new Rect(0f, 0f, rect.width - 20f, rect.height);
        Rect viewRect = new Rect(0f, 0f, scrollRect.width - 20f, pouchContents.Count * 30f);

        // Scroll view for the item list
        Widgets.BeginScrollView(scrollRect, ref scrollPosition, viewRect);
        float y = 30f;
        for (int i = 0; i < pouchContents.Count; i++) {
            Thing item = pouchContents[i];
            Rect rowRect = new Rect(0f, y, viewRect.width, 28f);
            Widgets.DrawHighlightIfMouseover(rowRect);
            Widgets.Label(new Rect(5f, y + 5f, viewRect.width - 10f, 24f), item.LabelCap);
            Rect buttonRect = new Rect(rowRect.width - 30f, y + 4f, 24f, 24f);

            if (Widgets.ButtonImage(buttonRect, TexCommand.RemoveRoutePlannerWaypoint)) {
                //change icon later to something better
                pouchSpheres.RemoveSphereFromPouch(i, selPawn);
                break;
            }

            if (Mouse.IsOver(rowRect)) {
                Stormlight comp = item.TryGetComp<Stormlight>();
                if (comp != null) {
                    string tooltip = $"Stormlight: {comp.currentStormlight} / {comp.currentMaxStormlight}";
                    Rect tooltipRect = new Rect(0f, 0f, 150f, 30f);
                    Widgets.Label(
                        new Rect(
                            tooltipRect.x + 5f,
                            tooltipRect.y + 5f,
                            tooltipRect.width - 10f,
                            tooltipRect.height - 10f
                        ),
                        tooltip
                    );
                }
            }

            y += 30f;
        }

        Widgets.EndScrollView();

        // Add button for adding spheres
        Rect addButtonRect = new Rect(0f, rect.height - 35f, rect.width, 30f);
        if (Widgets.ButtonText(addButtonRect, "Add Sphere")) {
            ThingDef matchingSphereDef =
                pouchSpheres.props.allowedSpheres.FirstOrDefault(def => selPawn.Map.listerThings.ThingsOfDef(def).Any()
                );
            Thing sphere = GenClosest.ClosestThing_Global(
                selPawn.Position,
                selPawn.Map.listerThings.ThingsOfDef(matchingSphereDef),
                50f
            );
            if (sphere != null && pouchSpheres.AddSphereToPouch(sphere as ThingWithComps)) {
                sphere.DeSpawn();
            }
        }

        Widgets.EndGroup();
    }
}