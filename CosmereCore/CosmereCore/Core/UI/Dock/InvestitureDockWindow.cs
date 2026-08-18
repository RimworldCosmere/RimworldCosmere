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

    /// <summary>
    ///     Where the player put it, before any clamping. Kept apart from windowRect because expanding
    ///     near an edge nudges the panel back on screen, and folding away again should return it to where they left it.
    /// </summary>
    private Vector2? desiredPosition;

    /// <summary>
    ///     The footprint the player actually dragged. Expanding is measured against this so the panel
    ///     can open away from an edge instead of shoving the whole dock along it.
    /// </summary>
    private Vector2 collapsedSize = new Vector2(CollapsedWidth, RibbonHeight);

    public InvestitureDockWindow() {
        doCloseButton = false;
        doCloseX = false;
        closeOnClickedOutside = false;
        closeOnAccept = false;
        closeOnCancel = false;
        preventCameraMotion = false;

        // Owned here, not by GUI.DragWindow - that gives up when the cursor outruns the window or an event is skipped.
        draggable = false;
        drawShadow = false;

        // The dock and ribbons paint their own ground, so vanilla's window backing is only ever in the way.
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

        // Deliberately skips desiredPosition - adopting the drawn rect stuck an upward-nudged panel at the top.
        dragging = true;
        dragMoved = false;
        dragStartMouse = Verse.UI.MousePositionOnUIInverted;
        dragGrabOffset = dragStartMouse - new Vector2(windowRect.x, windowRect.y);
        e.Use();
    }

    /// <summary>
    ///     Clamping and saving happen here rather than in DoWindowContents, because GUI.DragWindow runs
    ///     after it and would immediately overwrite a rect written there - the clamp was being applied and undone every frame.
    /// </summary>
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

        // Only size is reasserted each frame - position belongs to the player once dragged.
        Rect desired = ComputeRect(pawn, snapshots);
        if (!Mathf.Approximately(windowRect.width, desired.width) ||
            !Mathf.Approximately(windowRect.height, desired.height)) {
            windowRect = new Rect(windowRect.x, windowRect.y, desired.width, desired.height);
            inRect = new Rect(0f, 0f, desired.width, desired.height);
        }

        // Taken from computed size, not windowRect - using windowRect could record an expanded height as collapsed.
        if (!IsExpanded()) collapsedSize = new Vector2(desired.width, desired.height);

        if (pawn == null || snapshots.Count == 0) return;

        // Runs before the content, so shift-drag beats the wall-to-wall interactive controls underneath.
        TryBeginShiftDrag(inRect);

        if (!IsExpanded()) {
            // Each ribbon carries its own ground, so a panel behind them is unnecessary.
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

            // Bled only past a side the dock is against - dragged clear of the edge, the ribbon wants its border back.
            Rect ribbon = BleedFlushEdges(
                new Rect(inRect.x, y, inRect.width - RibbonGap, RibbonHeight),
                horizontalOnly: true
            );

            // Tinted faintly enough that it still reads as parchment, not a coloured panel.
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
                // White, not the lettering's cream - the sigil needs the contrast to carry the ribbon on its own.
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

            // Open onto the system that was actually clicked, not whichever section happened to be expanded last.
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

    /// <summary>
    ///     Held fully on screen rather than merely reachable - a dock half off the edge is clipped
    ///     and unusable. The clamp lands on windowRect only; desiredPosition keeps the unclamped intent.
    /// </summary>
    private void ApplyPosition() {
        Vector2 anchor = desiredPosition ?? new Vector2(windowRect.x, windowRect.y);
        float width = windowRect.width;
        float height = windowRect.height;

        // Opens away from a screen edge like a flipping menu, not a sliding one - far side stays pinned to footprint.
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
            // Gaps sit only between strips - first and last are flush with the window, so height matches what is drawn.
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
                // Capped the same way the accordion caps it, or the window sizes to a body about to be scrolled.
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
