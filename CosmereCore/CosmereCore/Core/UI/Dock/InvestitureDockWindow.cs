using Cosmere.Core.UI.Model;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.Core.UI.Dock;

public sealed class InvestitureDockWindow : Verse.Window {
    private const float CollapsedWidth = 48f;
    private const float MarginTop = 64f;
    private const float BottomTabBarHeight = 35f;

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

    public override Vector2 InitialSize => new Vector2(CollapsedWidth, ComputeHeight());

    protected override void SetInitialSizeAndPosition() {
        windowRect = ComputeRect();
    }

    public override void DoWindowContents(Rect inRect) {
        Rect desired = ComputeRect();
        if (windowRect != desired) {
            windowRect = desired;
            inRect = new Rect(0f, 0f, desired.width, desired.height);
        }
        Widgets.DrawBoxSolid(inRect, new Color(0.05f, 0.05f, 0.08f, 0.65f));
        Widgets.DrawBox(inRect);
    }

    private static Rect ComputeRect() {
        return new Rect(0f, MarginTop, CollapsedWidth, ComputeHeight());
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
