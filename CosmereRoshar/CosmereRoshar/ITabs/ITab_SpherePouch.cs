using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace CosmereRoshar;

public class ITab_SpherePouch : ITab {
    private static readonly Vector2 WinSize = new Vector2(300f, 480f);
    private Vector2 scrollPosition = Vector2.zero;

    public ITab_SpherePouch() {
        size = WinSize;
        labelKey = "whtwl_TabSpherePouch"; // Localization key
    }


    public override bool IsVisible {
        get {
            if (SelPawn == null) return false;

            // Check if the pawn has a Sphere Pouch equipped 
            return SelPawn.apparel?.WornApparel?.Any(a => a.TryGetComp<CompSpherePouch>() != null) ?? false;
        }
    }

    private Pawn SelPawn => SelThing as Pawn;

    private CompSpherePouch PouchSpheres {
        get {
            return SelPawn.apparel?.WornApparel.Select(a => a.GetComp<CompSpherePouch>())
                .FirstOrDefault(comp => comp != null);
        }
    }

    protected override void FillTab() {
        if (PouchSpheres == null) return;

        List<Thing> pouchContents = PouchSpheres.storedSpheres;

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
                PouchSpheres.RemoveSphereFromPouch(i, SelPawn);
                break;
            }

            if (Mouse.IsOver(rowRect)) {
                CompStormlight comp = item.TryGetComp<CompStormlight>();
                if (comp != null) {
                    string tooltip = $"Stormlight: {comp.Stormlight} / {comp.CurrentMaxStormlight}";
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
                PouchSpheres.Props.allowedSpheres.FirstOrDefault(def => SelPawn.Map.listerThings.ThingsOfDef(def).Any()
                );
            Thing sphere = GenClosest.ClosestThing_Global(
                SelPawn.Position,
                SelPawn.Map.listerThings.ThingsOfDef(matchingSphereDef),
                50f
            );
            if (sphere != null && PouchSpheres.AddSphereToPouch(sphere as ThingWithComps)) {
                sphere.DeSpawn();
            }
        }

        Widgets.EndGroup();
    }
}