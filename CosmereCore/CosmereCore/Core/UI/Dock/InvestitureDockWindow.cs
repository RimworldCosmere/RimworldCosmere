using Cosmere.Core.UI.Model;
using Cosmere.System.Scadrial.Gene;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Cosmere.Core.UI.Dock;

public sealed class InvestitureDockWindow : Verse.Window {
    private const float CollapsedWidth = 48f;
    private const float ExpandedWidth = 280f;
    private const float MarginTop = 64f;
    private const float BottomTabBarHeight = 35f;
    private const float PinButtonHeight = 24f;

    private readonly DockAccordion accordion = new();
    private bool pinned;
    private bool hovered;

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

        List<InvestitureSnapshot> snapshots = PawnInvestitureProviders.SnapshotsFor(pawn);
        if (snapshots.Count == 0) return;

        DockRenderContext ctx = new() {
            TwinbornPairs = BuildTwinbornPairs(pawn, snapshots),
            Density = PickDensity(pawn, snapshots, inRect.height),
        };

        if (IsExpanded()) {
            DrawExpanded(inRect, pawn, snapshots, ctx);
        } else {
            DrawCollapsed(inRect, snapshots);
        }
    }

    private bool IsExpanded() => pinned || hovered;

    private float CurrentWidth() => IsExpanded() ? ExpandedWidth : CollapsedWidth;

    private void DrawCollapsed(Rect inRect, List<InvestitureSnapshot> snapshots) {
        float iconSize = 32f;
        float y = inRect.y + 8f;
        for (int i = 0; i < snapshots.Count; i++) {
            IDockSection? section = DockSectionRegistry.For(snapshots[i].SystemId);
            if (section == null) continue;
            Rect headerRect = new Rect(inRect.x + (inRect.width - iconSize) / 2f, y, iconSize, iconSize);
            section.DrawHeader(headerRect, expanded: false);
            y += iconSize + 6f;
        }
    }

    private void DrawExpanded(
        Rect inRect,
        Pawn pawn,
        List<InvestitureSnapshot> snapshots,
        DockRenderContext ctx
    ) {
        Rect pinRect = new Rect(inRect.xMax - PinButtonHeight - 4f, inRect.y + 4f, PinButtonHeight, PinButtonHeight);
        string pinLabel = pinned ? "x" : "o";
        if (Widgets.ButtonText(pinRect, pinLabel)) {
            pinned = !pinned;
            RimWorld.SoundDefOf.Click.PlayOneShotOnCamera();
        }

        Rect bodyRect = new Rect(inRect.x, inRect.y + PinButtonHeight + 8f, inRect.width, inRect.height - PinButtonHeight - 8f);
        accordion.Draw(bodyRect, pawn, snapshots, ctx);
    }

    private static Dictionary<string, TwinbornPair> BuildTwinbornPairs(
        Pawn pawn,
        List<InvestitureSnapshot> snapshots
    ) {
        Dictionary<string, TwinbornPair> pairs = new();
        if (pawn.genes == null) return pairs;

        bool hasAllomancy = false;
        bool hasFeruchemy = false;
        for (int i = 0; i < snapshots.Count; i++) {
            if (snapshots[i].SystemId == "Allomancy") hasAllomancy = true;
            else if (snapshots[i].SystemId == "Feruchemy") hasFeruchemy = true;
        }
        if (!hasAllomancy || !hasFeruchemy) return pairs;

        Dictionary<string, Allomancer> allomancers = new();
        Dictionary<string, Feruchemist> feruchemists = new();

        List<Verse.Gene> all = pawn.genes.GenesListForReading;
        for (int i = 0; i < all.Count; i++) {
            if (all[i] is Allomancer a && !a.Overridden) allomancers[a.metal.defName] = a;
            else if (all[i] is Feruchemist f && !f.Overridden) feruchemists[f.metal.defName] = f;
        }

        foreach (KeyValuePair<string, Allomancer> kv in allomancers) {
            if (feruchemists.TryGetValue(kv.Key, out Feruchemist? f)) {
                pairs[kv.Key] = new TwinbornPair(kv.Key, kv.Value, f);
            }
        }
        return pairs;
    }

    private DockDensityMode PickDensity(
        Pawn pawn,
        List<InvestitureSnapshot> snapshots,
        float availableHeight
    ) {
        float needed = 0f;
        DockRenderContext probeCtx = new() { TwinbornPairs = BuildTwinbornPairs(pawn, snapshots) };
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
        float screenHeight = (float)Verse.UI.screenHeight;
        float bottomEdge = screenHeight - BottomTabBarHeight;
        WindowStack? stack = Find.WindowStack;
        if (stack != null) {
            MainTabWindow? openTab = stack.WindowOfType<MainTabWindow>();
            if (openTab != null) {
                float tabTop = openTab.windowRect.yMin;
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
        return PawnInvestitureProviders.HasAnyInvestment(pawn);
    }

    public static Pawn? GetSelectedPawn() {
        List<object> selected = Find.Selector.SelectedObjectsListForReading;
        if (selected.Count != 1) return null;
        return selected[0] as Pawn;
    }
}
