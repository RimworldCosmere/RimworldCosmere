using Cosmere.Core.UI.Model;
using UnityEngine;
using Verse;

namespace Cosmere.Core.UI.Dock;

public sealed class InvestitureDockWindow : Verse.Window {
    private const float CollapsedWidth = 48f;
    private const float MarginTop = 64f;
    private const float MarginBottom = 160f;

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

    public override Vector2 InitialSize => new Vector2(
        CollapsedWidth,
        (float)Verse.UI.screenHeight - MarginTop - MarginBottom
    );

    protected override void SetInitialSizeAndPosition() {
        windowRect = new Rect(
            0f,
            MarginTop,
            CollapsedWidth,
            (float)Verse.UI.screenHeight - MarginTop - MarginBottom
        );
    }

    public override void DoWindowContents(Rect inRect) {
        Widgets.DrawBoxSolid(inRect, new Color(0.05f, 0.05f, 0.08f, 0.65f));
        Widgets.DrawBox(inRect);
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
