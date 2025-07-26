using System;
using Cosmere.Framework.Comp.Thing;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Cosmere.Framework.InspectorTab;

public class StorageWithInventory : ITab_Storage {
    private static readonly Vector2 WinSize = new Vector2(800f, 480f);
    private static readonly List<Verse.Thing> workingInvList = [];
    private readonly InnerStorage? storage;
    private Vector2 scrollPosition = Vector2.zero;
    private float scrollViewHeight;


    public StorageWithInventory() {
        size = WinSize;
        labelKey = "TabStorage";
    }

    public StorageWithInventory(string labelKey, InnerStorage? storage = null) {
        size = WinSize;
        this.labelKey = labelKey;
        this.storage = storage;
    }

    private InnerStorage SelStorage => storage ?? SelThing.TryGetComp<InnerStorage>();

    private IEnumerable<Verse.Thing> heldThings => SelStorage.innerContainer ?? [];

    protected override IStoreSettingsParent SelStoreSettingsParent => SelStorage;

    private bool CanControl {
        get {
            if (SelPawn != null) {
                if (SelPawn.Faction != Faction.OfPlayer && !SelPawn.IsPrisonerOfColony) return false;
                if (SelPawn.IsPrisonerOfColony && SelPawn.Spawned && !SelPawn.Map.mapPawns.AnyFreeColonistSpawned) {
                    return false;
                }

                if (
                    SelPawn.IsPrisonerOfColony &&
                    (PrisonBreakUtility.IsPrisonBreaking(SelPawn) || SelPawn.CurJob is { exitMapOnArrival: true })
                ) {
                    return false;
                }

                if (SelPawn.Downed || SelPawn.InMentalState || SelPawn.CarriedBy != null) return false;
            }

            if (SelThing.Faction != Faction.OfPlayer) return false;

            return true;
        }
    }

    private bool CanControlColonist => CanControl && (SelPawn?.IsColonistPlayerControlled ?? false);

    private float TopAreaHeight => IsPrioritySettingVisible ? 35 : 20;

    protected override void FillTab() {
        base.FillTab();
        Rect rect = new Rect(300, TopAreaHeight, WinSize.x - 300, WinSize.y - TopAreaHeight).ContractedBy(10f);

        using (new TextBlock(GameFont.Small, Color.white)) {
            Widgets.BeginGroup(rect);
            float curY = 0f;
            Widgets.ListSeparator(ref curY, rect.width, "Inventory".Translate());
            Rect outRect = new Rect(0, curY, rect.width, rect.height);
            Rect viewRect = new Rect(0, curY, rect.width - 16f, scrollViewHeight);
            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);

            workingInvList.Clear();
            workingInvList.AddRange(heldThings);
            foreach (Verse.Thing t in workingInvList) {
                DrawThingRow(ref curY, viewRect.width, t);
            }

            workingInvList.Clear();
            Widgets.EndScrollView();

            if (Event.current.type == EventType.Layout) {
                scrollViewHeight = curY + 20f;
            }

            Widgets.EndGroup();
        }
    }

    private void DrawThingRow(ref float y, float width, Verse.Thing thing) {
        Rect rect = new Rect(0f, y, width, 28f);
        Widgets.InfoCardButton(rect.width - 24f, y, thing);
        rect.width -= 24f;
        bool disabled = false;
        if (CanControl) {
            Rect rect2 = new Rect(rect.width - 24f, y, 24f, 24f);
            bool dropLocked = thing is Apparel apparel && SelPawn?.apparel != null && SelPawn.apparel.IsLocked(apparel);
            disabled = dropLocked;
            if (Mouse.IsOver(rect2)) {
                if (dropLocked) {
                    TooltipHandler.TipRegion(rect2, "DropThingLocked".Translate());
                } else {
                    TooltipHandler.TipRegion(rect2, "DropThing".Translate());
                }
            }

            Color color = disabled ? Color.grey : Color.white;
            Color mouseoverColor = disabled ? color : GenUI.MouseoverColor;
            if (Widgets.ButtonImage(rect2, TexButton.Drop, color, mouseoverColor, !disabled) && !disabled) {
                Action action = delegate {
                    SoundDefOf.Tick_High.PlayOneShotOnCamera();
                    InterfaceDrop(thing);
                };
                if (!ModsConfig.BiotechActive ||
                    !MechanitorUtility.TryConfirmBandwidthLossFromDroppingThing(SelPawn, thing, action)) {
                    action();
                }
            }

            rect.width -= 24f;
        }

        if (CanControlColonist) {
            if (SelPawn != null && FoodUtility.WillIngestFromInventoryNow(SelPawn, thing)) {
                Rect rect3 = new Rect(rect.width - 24f, y, 24f, 24f);
                TooltipHandler.TipRegionByKey(rect3, "ConsumeThing", thing.LabelNoCount, thing);
                if (Widgets.ButtonImage(rect3, TexButton.Ingest)) {
                    SoundDefOf.Tick_High.PlayOneShotOnCamera();
                    FoodUtility.IngestFromInventoryNow(SelPawn, thing);
                }
            }

            rect.width -= 24f;
        }

        Rect rect4 = rect;
        rect4.xMin = rect4.xMax - 60f;
        CaravanThingsTabUtility.DrawMass(thing, rect4);
        rect.width -= 60f;
        if (Mouse.IsOver(rect)) {
            GUI.color = ITab_Pawn_Gear.HighlightColor;
            GUI.DrawTexture(rect, TexUI.HighlightTex);
        }

        if (thing.def.DrawMatSingle != null && thing.def.DrawMatSingle.mainTexture != null) {
            Widgets.ThingIcon(new Rect(4f, y, 28f, 28f), thing);
        }

        Text.Anchor = TextAnchor.MiddleLeft;
        GUI.color = ITab_Pawn_Gear.ThingLabelColor;
        Rect rect5 = new Rect(36f, y, rect.width - 36f, rect.height);
        string text = thing.LabelCap;
        if (thing is Apparel ap && SelPawn?.outfits != null && SelPawn.outfits.forcedHandler.IsForced(ap)) {
            text += ", " + "ApparelForcedLower".Translate();
        }

        if (disabled) {
            text += " (" + "ApparelLockedLower".Translate() + ")";
        }

        Text.WordWrap = false;
        Widgets.Label(rect5, text.Truncate(rect5.width));
        Text.WordWrap = true;
        if (Mouse.IsOver(rect)) {
            TooltipHandler.TipRegion(rect, thing.GetTooltip());
        }

        y += 28f;
    }

    private void InterfaceDrop(Verse.Thing t) {
        if (t.def.destroyOnDrop) return;

        SelStorage.innerContainer.TryDrop(t, SelPawn.Position, SelPawn.Map, ThingPlaceMode.Near, out _);
    }
}