using Cosmere.Core.UI.Model;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Cosmere.Core.UI.Dock;

public sealed class InvestitureDockWindow : Verse.Window {
    private const float CollapsedWidth = 48f;
    private const float ExpandedWidth = 280f;
    private const float MarginTop = 114f;
    private const float MarginBottom = 185f;
    private const float TabPadding = 150f;
    private const float PinButtonHeight = 24f;

    private readonly DockAccordion accordion = new DockAccordion();
    private bool hovered;
    private bool pinned;

    public InvestitureDockWindow() {
        doCloseButton = false;
        doCloseX = false;
        closeOnClickedOutside = false;
        closeOnAccept = false;
        closeOnCancel = false;
        preventCameraMotion = false;
        draggable = false;
        drawShadow = false;
        layer = WindowLayer.GameUI;
        focusWhenOpened = false;
    }

    protected override float Margin => 0f;

    public override Vector2 InitialSize => new Vector2(CurrentWidth(), ComputeHeight());

    protected override void SetInitialSizeAndPosition() {
        windowRect = ComputeRect();
    }

    public override void DoWindowContents(Rect inRect) {
        hovered = Mouse.IsOver(inRect);
        Rect desired = ComputeRect();
        if (windowRect != desired) {
            windowRect = desired;
            inRect = new Rect(0f, 0f, desired.width, desired.height);
        }

        Widgets.DrawBoxSolid(inRect, new Color(0.05f, 0.05f, 0.08f, 0.75f));
        Widgets.DrawBox(inRect);

        Pawn? pawn = GetSelectedPawn();
        if (pawn == null) return;

        IReadOnlyList<InvestitureSnapshot> snapshots = InvestitureProviderRegistry.SnapshotsFor(pawn);
        if (snapshots.Count == 0) return;

        DockRenderContext ctx = new DockRenderContext {
            Density = PickDensity(pawn, snapshots, inRect.height),
        };

        if (IsExpanded()) {
            DrawExpanded(inRect, pawn, snapshots, ctx);
        }
        else {
            DrawCollapsed(inRect, snapshots);
        }
    }

    private bool IsExpanded() {
        return pinned || hovered;
    }

    private float CurrentWidth() {
        return IsExpanded() ? ExpandedWidth : CollapsedWidth;
    }

    private void DrawCollapsed(Rect inRect, IReadOnlyList<InvestitureSnapshot> snapshots) {
        const float orbSize = 28f;
        const float railBarWidth = 3f;
        float y = inRect.y + 8f;
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

            Rect orbRect = new Rect(inRect.x + 6f, y, orbSize, orbSize);
            Color accent = section.Skin.AccentColor;
            if (anyFlaring) Widgets.DrawBoxSolid(orbRect.ExpandedBy(2f), new Color(DockPalette.Flare.r, DockPalette.Flare.g, DockPalette.Flare.b, 0.35f));
            else if (anyActive) Widgets.DrawBoxSolid(orbRect.ExpandedBy(2f), new Color(DockPalette.HotLabel.r, DockPalette.HotLabel.g, DockPalette.HotLabel.b, 0.3f));
            Widgets.DrawBoxSolid(orbRect, DockPalette.PanelRaised);
            Widgets.DrawBoxSolidWithOutline(orbRect, Color.clear, accent);
            Texture2D? sigil = section.Skin.Sigil;
            if (sigil != null) GUI.DrawTexture(orbRect.ContractedBy(4f), sigil);

            Rect railBar = new Rect(orbRect.xMax + 4f, y, railBarWidth, orbSize);
            Widgets.DrawBoxSolid(railBar, DockPalette.Panel);
            Rect railFill = new Rect(railBar.x, railBar.yMax - railBar.height * Mathf.Clamp01(aggregate), railBarWidth, railBar.height * Mathf.Clamp01(aggregate));
            Widgets.DrawBoxSolid(railFill, accent);

            TooltipHandler.TipRegion(orbRect, section.Skin.HeaderLabel);
            y += orbSize + 10f;
        }
    }

    private void DrawExpanded(
        Rect inRect,
        Pawn pawn,
        IReadOnlyList<InvestitureSnapshot> snapshots,
        DockRenderContext ctx
    ) {
        Rect pinRect = new Rect(inRect.xMax - PinButtonHeight - 4f, inRect.y + 4f, PinButtonHeight, PinButtonHeight);
        string pinLabel = pinned ? "x" : "o";
        if (Widgets.ButtonText(pinRect, pinLabel)) {
            pinned = !pinned;
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

    private Rect ComputeRect() {
        return new Rect(0f, MarginTop, CurrentWidth(), ComputeHeight());
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