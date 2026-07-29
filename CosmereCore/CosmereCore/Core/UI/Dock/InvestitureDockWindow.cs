using Cosmere.Core.Settings;
using Cosmere.Core.UI.Model;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Cosmere.Core.UI.Dock;

public sealed class InvestitureDockWindow : Verse.Window {
    private const float CollapsedWidth = 156f;
    private const float ExpandedWidth = 370f;
    private const float MarginTop = 114f;
    private const float MarginBottom = 185f;
    private const float TabPadding = 150f;
    private const float PinButtonHeight = 24f;
    private const float RibbonGap = 6f;
    private const float RibbonIcon = 40f;
    private const float RibbonBar = 6f;
    private const float RibbonPad = 7f;
    private const float RibbonBleed = 6f;

    // How far the pointer must travel before a press counts as a drag rather than a click.
    private const float DragThreshold = 4f;

    private static readonly Color RibbonInk = new Color(0.878f, 0.831f, 0.729f);
    private static readonly Color RibbonInkFaded = new Color(0.678f, 0.620f, 0.514f);
    private static readonly Color RibbonEdge = new Color(0.361f, 0.294f, 0.196f);
    private static readonly Color RibbonTrack = new Color(0.094f, 0.078f, 0.059f);

    // Icon and name on one line, reserve and reading on the next.
    private static float RibbonHeight => 13f + RibbonIcon + Text.LineHeightOf(GameFont.Tiny);

    private readonly DockAccordion accordion = new DockAccordion();
    private bool pinned;
    private bool dragging;
    private bool dragMoved;
    private Vector2 dragGrabOffset;
    private Vector2 dragStartMouse;

    // Where the player put it, before any clamping. Kept apart from windowRect because expanding
    // near an edge has to nudge the panel back on screen, and folding it away again should return
    // it to where they left it rather than leaving it stranded at the nudged spot.
    private Vector2? desiredPosition;

    // The footprint the player actually dragged. Expanding is measured against this so the panel can
    // open away from an edge instead of shoving the whole dock along it.
    private Vector2 collapsedSize = new Vector2(CollapsedWidth, RibbonHeight);

    public InvestitureDockWindow() {
        doCloseButton = false;
        doCloseX = false;
        closeOnClickedOutside = false;
        closeOnAccept = false;
        closeOnCancel = false;
        preventCameraMotion = false;

        // The drag is owned here rather than handed to GUI.DragWindow. That runs off IMGUI events at
        // the tail of the window's GUI pass and gives up the moment the cursor outruns the window or
        // the event stream skips, so a slow drag would simply stop. Tracking the button in Update
        // instead means the dock follows the pointer until the button is actually released.
        draggable = false;
        drawShadow = false;

        // The dock paints its own ground when open, and when closed the ribbons
        // carry theirs, so vanilla's window backing is only ever a box in the way.
        doWindowBackground = false;
        layer = WindowLayer.GameUI;
        focusWhenOpened = false;
    }

    protected override float Margin => 0f;

    public override Vector2 InitialSize => new Vector2(CurrentWidth(), ComputeHeight());

    protected override void SetInitialSizeAndPosition() {
        windowRect = ComputeRect(null, []);

        CoreModSettings settings = Mod.GetModSettings<CoreModSettings>();
        if (settings.dockPositionSet) desiredPosition = settings.dockPosition;
    }

    /// <summary>
    ///     Extends <paramref name="rect" /> past any screen edge the dock is sitting against, so the
    ///     border on that side falls outside the window and is clipped away. Sides that are free keep
    ///     their border, which is what makes a dragged dock read as a panel rather than as something
    ///     sliced off by the frame.
    /// </summary>
    /// <param name="horizontalOnly">
    ///     True for the collapsed ribbons: they are discrete strips stacked with gaps, so their top and
    ///     bottom edges are always interior and always want a border, whatever the dock is against.
    /// </param>
    private void TryBeginShiftDrag(Rect inRect) {
        if (!Event.current.shift) return;

        TryBeginDrag(inRect);
    }

    private Rect BleedFlushEdges(Rect rect, bool horizontalOnly) {
        const float tolerance = 1f;

        float left = windowRect.x <= tolerance ? RibbonBleed : 0f;
        float right = windowRect.xMax >= Verse.UI.screenWidth - tolerance ? RibbonBleed : 0f;
        float top = !horizontalOnly && windowRect.y <= tolerance ? RibbonBleed : 0f;
        float bottom = !horizontalOnly && windowRect.yMax >= Verse.UI.screenHeight - tolerance ? RibbonBleed : 0f;

        return new Rect(rect.x - left, rect.y - top, rect.width + left + right, rect.height + top + bottom);
    }

    private void TryBeginDrag(Rect inRect) {
        Event e = Event.current;
        if (e.type != EventType.MouseDown || e.button != 0 || !inRect.Contains(e.mousePosition)) return;

        // Deliberately does not touch desiredPosition yet. A press that never moves is a click, and
        // adopting the drawn rect here rewrote the anchor to wherever the panel had been nudged or
        // flipped to - so closing a panel that had opened upward left it stuck at the top.
        dragging = true;
        dragMoved = false;
        dragStartMouse = Verse.UI.MousePositionOnUIInverted;
        dragGrabOffset = dragStartMouse - new Vector2(windowRect.x, windowRect.y);
        e.Use();
    }

    // Clamping and saving happen here rather than in DoWindowContents. GUI.DragWindow runs at the
    // very end of the window's GUI pass, after DoWindowContents, so a rect written from inside that
    // pass is immediately overwritten by the drag - the clamp was being applied and undone every
    // frame, which is what made dragging feel like it was asking permission to move.
    public override void WindowUpdate() {
        base.WindowUpdate();

        if (dragging) {
            if (Input.GetMouseButton(0)) {
                Vector2 mouse = Verse.UI.MousePositionOnUIInverted;
                if (dragMoved || (mouse - dragStartMouse).sqrMagnitude >= DragThreshold * DragThreshold) {
                    dragMoved = true;
                    desiredPosition = mouse - dragGrabOffset;
                }
            } else {
                dragging = false;
                if (dragMoved) SavePosition();
            }
        }

        ApplyPosition();
    }

    public override void PostClose() {
        base.PostClose();
        SavePosition();
    }

    public override void DoWindowContents(Rect inRect) {
        Pawn? pawn = GetSelectedPawn();
        IReadOnlyList<InvestitureSnapshot> snapshots = pawn != null
            ? InvestitureProviderRegistry.SnapshotsFor(pawn)
            : [];

        // Sized to what it actually draws. Holding the full screen height left a
        // tall dark slab over the map with a couple of orbs stranded at the top.
        // Only the size is reasserted: the position belongs to the player once they
        // have dragged it, and rewriting the whole rect each frame fought the drag.
        Rect desired = ComputeRect(pawn, snapshots);
        if (!Mathf.Approximately(windowRect.width, desired.width) ||
            !Mathf.Approximately(windowRect.height, desired.height)) {
            windowRect = new Rect(windowRect.x, windowRect.y, desired.width, desired.height);
            inRect = new Rect(0f, 0f, desired.width, desired.height);
        }

        // Taken from the computed size rather than from windowRect: the two disagree on the frame a
        // size change lands, and recording an expanded height as the collapsed footprint would
        // permanently skew which way the panel opens.
        if (!IsExpanded()) collapsedSize = new Vector2(desired.width, desired.height);

        if (pawn == null || snapshots.Count == 0) return;

        // Shift is the only way to move the dock, in either state, and this runs before the content
        // so it beats the controls underneath. Both states are wall-to-wall interactive - the
        // collapsed ribbons are buttons that open the dock, the expanded panel is sliders and
        // headers - so a plain drag would either be impossible or would fight whatever it started on.
        TryBeginShiftDrag(inRect);

        if (!IsExpanded()) {
            // Each ribbon carries its own ground, so a panel behind them would only
            // be a box around loose strips of parchment.
            DrawCollapsed(inRect, snapshots);
            return;
        }

        Rect panel = BleedFlushEdges(inRect, horizontalOnly: false);
        ParchmentTex.DrawField(panel);
        Widgets.DrawBoxSolidWithOutline(panel, Color.clear, new Color(0.361f, 0.294f, 0.196f));

        DockRenderContext ctx = new DockRenderContext {
            Density = PickDensity(pawn, snapshots, inRect.height),
        };

        DrawExpanded(inRect, pawn, snapshots, ctx);
    }

    private bool IsExpanded() {
        return pinned;
    }

    private float CurrentWidth() {
        return IsExpanded() ? ExpandedWidth : CollapsedWidth;
    }

    private void DrawCollapsed(Rect inRect, IReadOnlyList<InvestitureSnapshot> snapshots) {
        float tinyH = Text.LineHeightOf(GameFont.Tiny);
        float y = inRect.y;

        for (int i = 0; i < snapshots.Count; i++) {
            InvestitureSnapshot snap = snapshots[i];
            IDockSection? section = DockSectionRegistry.For(snap.SystemId);
            if (section == null) continue;

            float aggregate = 0f;
            bool anyActive = false;
            bool anyFlaring = false;
            for (int c = 0; c < snap.Cells.Count; c++) {
                aggregate += snap.Cells[c].Bar.Fraction;
                anyActive |= snap.Cells[c].IsActive;
                anyFlaring |= snap.Cells[c].IsFlaring;
            }

            if (snap.Cells.Count > 0) aggregate /= snap.Cells.Count;

            Color accent = section.Skin.AccentColor;

            // Only bled past a side the dock is actually against, where the overhang and its border
            // are clipped away and the banner reads as coming out of the edge of the screen. Dragged
            // clear of that edge it is a floating strip, and wants its border back.
            Rect ribbon = BleedFlushEdges(
                new Rect(inRect.x, y, inRect.width - RibbonGap, RibbonHeight),
                horizontalOnly: true
            );

            // Each art tints its own sheet, faintly enough that both still read as
            // parchment rather than as coloured panels.
            Color prev = GUI.color;
            GUI.color = Color.Lerp(Color.white, accent, 0.16f);
            GUI.DrawTexture(ribbon, ParchmentTex.Sheet);
            GUI.color = prev;

            if (anyFlaring) {
                Widgets.DrawBoxSolid(ribbon, new Color(DockPalette.Flare.r, DockPalette.Flare.g, DockPalette.Flare.b, 0.20f));
            } else if (anyActive) {
                Widgets.DrawBoxSolid(ribbon, new Color(DockPalette.HotLabel.r, DockPalette.HotLabel.g, DockPalette.HotLabel.b, 0.16f));
            }

            Widgets.DrawBoxSolidWithOutline(ribbon, Color.clear, RibbonEdge);

            Rect icon = new Rect(inRect.x + RibbonPad, ribbon.y + 5f, RibbonIcon, RibbonIcon);
            Texture2D? sigil = section.Skin.Sigil;
            if (sigil != null) {
                // White, as every mark in the dock is. Matching the lettering's cream
                // read as part of the same inscription, but it cost contrast against
                // the parchment, and the mark has to carry the ribbon on its own.
                Color prevIcon = GUI.color;
                GUI.color = Color.white;
                GUI.DrawTexture(icon, sigil);
                GUI.color = prevIcon;
            } else {
                UIText.EllipsisLabel(
                    icon,
                    section.Skin.HeaderLabel.Substring(0, 1),
                    GameFont.Small,
                    TextAnchor.MiddleCenter,
                    RibbonInk
                );
            }

            UIText.EllipsisLabel(
                new Rect(icon.xMax + RibbonPad, icon.y, ribbon.xMax - icon.xMax - RibbonPad * 2f, RibbonIcon),
                section.Skin.HeaderLabel,
                GameFont.Tiny,
                TextAnchor.MiddleLeft,
                RibbonInk
            );

            float readingWidth = 34f;
            Rect readingRow = new Rect(inRect.x + RibbonPad, icon.yMax + 3f, ribbon.xMax - inRect.x - RibbonPad * 2f, tinyH);
            Rect bar = new Rect(
                readingRow.x,
                readingRow.y + (tinyH - RibbonBar) / 2f,
                readingRow.width - readingWidth - 4f,
                RibbonBar
            );
            Widgets.DrawBoxSolid(bar, RibbonTrack);
            Widgets.DrawBoxSolid(
                new Rect(bar.x, bar.y, bar.width * Mathf.Clamp01(aggregate), bar.height),
                accent
            );

            UIText.EllipsisLabel(
                new Rect(bar.xMax + 4f, readingRow.y, readingWidth, tinyH),
                Mathf.RoundToInt(aggregate * 100f) + "%",
                GameFont.Tiny,
                TextAnchor.MiddleRight,
                RibbonInkFaded
            );

            TooltipHandler.TipRegion(
                ribbon,
                "CC_Dock_Rail_Tip".Translate(
                    section.Skin.HeaderLabel.Named("SYSTEM"),
                    Mathf.RoundToInt(aggregate * 100f).Named("PERCENT")
                )
            );
            Widgets.DrawHighlightIfMouseover(ribbon);

            // Open onto the system that was actually clicked, not whichever
            // section happened to be expanded last.
            if (Widgets.ButtonInvisible(ribbon)) {
                pinned = true;
                accordion.ExpandedSystemId = section.SystemId;
                RimWorld.SoundDefOf.Click.PlayOneShotOnCamera();
            }

            y += RibbonHeight + RibbonGap;
        }
    }

    private void DrawExpanded(
        Rect inRect,
        Pawn pawn,
        IReadOnlyList<InvestitureSnapshot> snapshots,
        DockRenderContext ctx
    ) {
        Rect headerBand = new Rect(inRect.x, inRect.y, inRect.width, PinButtonHeight + 8f);
        Widgets.DrawBoxSolid(headerBand, new Color(0.078f, 0.063f, 0.043f, 0.45f));
        Widgets.DrawBoxSolid(
            new Rect(headerBand.x, headerBand.yMax - 1f, headerBand.width, 1f),
            new Color(0.361f, 0.294f, 0.196f)
        );

        Rect pinRect = new Rect(inRect.xMax - PinButtonHeight - 4f, inRect.y + 4f, PinButtonHeight, PinButtonHeight);
        UIText.EllipsisLabel(
            new Rect(headerBand.x + 10f, headerBand.y, headerBand.width - PinButtonHeight - 22f, headerBand.height),
            pawn.LabelShortCap,
            GameFont.Small,
            TextAnchor.MiddleLeft,
            RibbonInk
        );
        TooltipHandler.TipRegion(pinRect, "CC_Dock_Collapse".Translate());
        if (Widgets.ButtonImage(pinRect.ContractedBy(5f), TexButton.CloseXSmall, true)) {
            pinned = false;
            RimWorld.SoundDefOf.Click.PlayOneShotOnCamera();
        }

        Rect bodyRect = new Rect(
            inRect.x,
            inRect.y + PinButtonHeight + 8f,
            inRect.width,
            inRect.height - PinButtonHeight - 8f
        );
        accordion.Draw(bodyRect, pawn, snapshots, ctx);
    }

    private DockDensityMode PickDensity(
        Pawn pawn,
        IReadOnlyList<InvestitureSnapshot> snapshots,
        float availableHeight
    ) {
        float needed = 0f;
        DockRenderContext probeCtx = new DockRenderContext();
        for (int i = 0; i < snapshots.Count; i++) {
            IDockSection? section = DockSectionRegistry.For(snapshots[i].SystemId);
            if (section == null) continue;
            needed += section.GetHeaderHeight();
            needed += section.GetExpandedBodyHeight(pawn, snapshots[i], probeCtx);
        }

        return needed > availableHeight ? DockDensityMode.Compact : DockDensityMode.Full;
    }

    private Rect ComputeRect(Pawn? pawn, IReadOnlyList<InvestitureSnapshot> snapshots) {
        float available = ComputeHeight();
        float width = CurrentWidth();
        float height = Mathf.Min(ContentHeight(pawn, snapshots), available);

        CoreModSettings settings = Mod.GetModSettings<CoreModSettings>();
        return settings.dockPositionSet
            ? new Rect(settings.dockPosition.x, settings.dockPosition.y, width, height)
            : new Rect(0f, MarginTop, width, height);
    }

    // Held fully on screen rather than merely reachable: a dock half off the edge is clipped and
    // unusable, and a resolution change can otherwise strand it outside the view entirely. The clamp
    // lands on windowRect only - desiredPosition keeps the unclamped intent.
    private void ApplyPosition() {
        Vector2 anchor = desiredPosition ?? new Vector2(windowRect.x, windowRect.y);
        float width = windowRect.width;
        float height = windowRect.height;

        // Open away from an edge there is no room against, keeping the far side pinned to the
        // collapsed footprint - the dock unfolds leftward or upward the way a menu flips near a
        // screen edge, rather than sliding bodily along the edge and appearing to jump.
        float x = anchor.x + width <= Verse.UI.screenWidth ? anchor.x : anchor.x + collapsedSize.x - width;
        float y = anchor.y + height <= Verse.UI.screenHeight ? anchor.y : anchor.y + collapsedSize.y - height;

        float maxX = Mathf.Max(0f, Verse.UI.screenWidth - width);
        float maxY = Mathf.Max(0f, Verse.UI.screenHeight - height);
        x = Mathf.Clamp(x, 0f, maxX);
        y = Mathf.Clamp(y, 0f, maxY);
        if (Mathf.Approximately(x, windowRect.x) && Mathf.Approximately(y, windowRect.y)) return;

        windowRect = new Rect(x, y, windowRect.width, windowRect.height);
    }

    private void SavePosition() {
        if (desiredPosition is not { } wanted) return;

        CoreModSettings settings = Mod.GetModSettings<CoreModSettings>();
        if (settings.dockPositionSet && settings.dockPosition == wanted) return;

        settings.dockPosition = wanted;
        settings.dockPositionSet = true;
        LoadedModManager.GetMod<Mod>()?.WriteSettings();
    }

    private float ContentHeight(Pawn? pawn, IReadOnlyList<InvestitureSnapshot> snapshots) {
        if (snapshots.Count == 0 || pawn == null) return RibbonHeight;

        if (!IsExpanded()) {
            // Gaps sit between the strips only: the first is flush with the top of the window and
            // the last with the bottom, so the dock is exactly as tall as what it draws.
            return snapshots.Count * RibbonHeight + Mathf.Max(0, snapshots.Count - 1) * RibbonGap;
        }

        float height = PinButtonHeight + 8f;
        float sectionMax = Mod.GetModSettings<CoreModSettings>().dockSectionMaxHeight;
        DockRenderContext probeCtx = new DockRenderContext();
        for (int i = 0; i < snapshots.Count; i++) {
            IDockSection? section = DockSectionRegistry.For(snapshots[i].SystemId);
            if (section == null) continue;

            height += section.GetHeaderHeight();
            if (accordion.ExpandedSystemId == section.SystemId) {
                // Capped the same way the accordion caps it, or the window would size
                // itself to a body the accordion is about to put in a scroll view.
                height += Mathf.Min(section.GetExpandedBodyHeight(pawn, snapshots[i], probeCtx), sectionMax);
            }
        }

        return height;
    }

    private static float ComputeHeight() {
        float screenHeight = Verse.UI.screenHeight;
        float bottomEdge = screenHeight - MarginBottom;
        WindowStack? stack = Find.WindowStack;
        if (stack != null) {
            MainTabWindow? openTab = stack.WindowOfType<MainTabWindow>();
            if (openTab != null) {
                float tabTop = openTab.windowRect.yMin - TabPadding;
                if (tabTop > MarginTop && tabTop < bottomEdge) {
                    bottomEdge = tabTop;
                }
            }
        }

        return Mathf.Max(0f, bottomEdge - MarginTop);
    }

    public static bool ShouldShow() {
        Pawn? pawn = GetSelectedPawn();
        if (pawn == null) return false;
        return InvestitureProviderRegistry.HasAnyInvestment(pawn);
    }

    public static Pawn? GetSelectedPawn() {
        List<object> selected = Find.Selector.SelectedObjectsListForReading;
        if (selected.Count != 1) return null;
        return selected[0] as Pawn;
    }
}
