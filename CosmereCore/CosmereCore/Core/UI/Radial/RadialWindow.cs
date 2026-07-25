using Cosmere.Core.UI.Skin;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Cosmere.Core.UI.Radial;

public sealed class RadialWindow : Verse.Window {
    private readonly RadialState state = new RadialState();
    private RadialSnapshot snapshot;

    public bool BrowseMode;
    public Vector2 Anchor;
    private TimeSpeed? restoreSpeed;

    public RadialWindow(RadialSnapshot snapshot, bool anchorOnPawn = false) {
        this.snapshot = snapshot;
        doCloseButton = false;
        doCloseX = false;
        closeOnClickedOutside = false;
        closeOnAccept = false;
        closeOnCancel = false;
        preventCameraMotion = false;
        draggable = false;
        drawShadow = false;
        doWindowBackground = false;
        layer = WindowLayer.GameUI;
        focusWhenOpened = false;
        forcePause = false;
        state.Kind = RadialStateKind.SystemTier;
        AutoSkipOneOptionTiers();
        Anchor = RadialAnchor.Resolve(snapshot.Pawn, anchorOnPawn);
    }

    protected override float Margin => 0f;

    // Sized to the wheel rather than the screen: a fullscreen window swallows
    // every click outside the wheel for as long as it is open.
    private static float WindowExtent => RadialLayout.AbilityRingOuter + 24f;

    public override Vector2 InitialSize => new Vector2(WindowExtent * 2f, WindowExtent * 2f);

    protected override void SetInitialSizeAndPosition() {
        windowRect = new Rect(
            Anchor.x - WindowExtent,
            Anchor.y - WindowExtent,
            WindowExtent * 2f,
            WindowExtent * 2f
        );
    }

    public override void PostOpen() {
        base.PostOpen();
        if (!Mod.GetModSettings<Cosmere.Core.Settings.CoreModSettings>().radialPausesGame) return;

        TickManager ticks = Find.TickManager;
        if (ticks.CurTimeSpeed == TimeSpeed.Paused) return;

        // Only restore what we interrupted - a game already paused stays paused.
        restoreSpeed = ticks.CurTimeSpeed;
        ticks.CurTimeSpeed = TimeSpeed.Paused;
    }

    public override void DoWindowContents(Rect inRect) {
        if (!snapshot.Pawn.Spawned || snapshot.Pawn.Dead) {
            Close(false);
            return;
        }

        // The wheel acts on the selected pawn, so it has nothing to act on once
        // the selection moves elsewhere.
        if (Find.Selector.SingleSelectedThing != snapshot.Pawn) {
            Close(false);
            return;
        }

        RadialSnapshot? rebuilt = RadialSnapshotBuilder.Build(snapshot.Pawn);
        if (rebuilt == null) {
            Close(false);
            return;
        }

        snapshot = rebuilt;

        if (state.Kind != RadialStateKind.SystemTier) {
            if (state.SelectedSystemIndex < 0 || state.SelectedSystemIndex >= snapshot.Systems.Count) {
                Close(false);
                return;
            }

            if (state.Kind != RadialStateKind.SubsectionTier) {
                if (state.SelectedSubsectionIndex < 0 ||
                    state.SelectedSubsectionIndex >= snapshot.Systems[state.SelectedSystemIndex].Subsections.Count) {
                    Close(false);
                    return;
                }

                int leafCount = snapshot.Systems[state.SelectedSystemIndex]
                    .Subsections[state.SelectedSubsectionIndex].Leaves.Count;
                if (state.HoveredIndex >= leafCount) state.HoveredIndex = -1;
            }
        }

        // Drawing happens in window-local space, so the wheel sits at the
        // window's own centre rather than at the screen-space anchor.
        Vector2 center = new Vector2(WindowExtent, WindowExtent);
        Vector2 mouse = Event.current.mousePosition;

        UpdateHover(center, mouse);
        // Fills the window exactly; anything larger would clip to a hard edge.
        float vignetteSize = WindowExtent * 2f;
        Color prevGuiColor = GUI.color;
        GUI.color = new Color(0f, 0f, 0f, 0.38f);
        GUI.DrawTexture(
            new Rect(center.x - vignetteSize / 2f, center.y - vignetteSize / 2f, vignetteSize, vignetteSize),
            RadialWedgeTex.Vignette()
        );
        GUI.color = prevGuiColor;
        DrawRings(center);
        string breadcrumb = BuildBreadcrumb();
        RadialCenterPreview.Draw(
            center,
            state.ResolveHoveredLeaf(snapshot) ?? HoveredCollapsedLeaf(),
            HoveredWedgeTitle(),
            breadcrumb,
            BrowseMode,
            () => { if (state.Kind == RadialStateKind.SystemTier) Close(false); else state.Back(); },
            () => Close(false)
        );
        HandleInput();
    }

    public override void OnCancelKeyPressed() {
        Close(false);
        Event.current?.Use();
    }

    private RadialLeaf? HoveredCollapsedLeaf() {
        if (state.Kind != RadialStateKind.SubsectionTier) return null;
        if (state.HoveredIndex < 0) return null;

        RadialSystem sys = snapshot.Systems[state.SelectedSystemIndex];
        if (state.HoveredIndex >= sys.Subsections.Count) return null;

        RadialSubsection hovered = sys.Subsections[state.HoveredIndex];
        return hovered.Leaves.Count == 1 ? hovered.Leaves[0] : null;
    }

    private string? HoveredWedgeTitle() {
        if (state.HoveredIndex < 0) return null;
        switch (state.Kind) {
            case RadialStateKind.SystemTier:
                return state.HoveredIndex < snapshot.Systems.Count
                    ? snapshot.Systems[state.HoveredIndex].Label
                    : null;
            case RadialStateKind.SubsectionTier: {
                RadialSystem sys = snapshot.Systems[state.SelectedSystemIndex];
                return state.HoveredIndex < sys.Subsections.Count
                    ? sys.Subsections[state.HoveredIndex].Label
                    : null;
            }
            default:
                return null;
        }
    }

    private string BuildBreadcrumb() {
        if (state.Kind == RadialStateKind.SystemTier) return "";
        if (state.Kind == RadialStateKind.SubsectionTier) {
            return snapshot.Systems[state.SelectedSystemIndex].Label;
        }

        RadialSystem sys = snapshot.Systems[state.SelectedSystemIndex];
        RadialSubsection sub = sys.Subsections[state.SelectedSubsectionIndex];
        return $"{sys.Label} - {sub.Label}";
    }

    private void UpdateHover(Vector2 center, Vector2 mouse) {
        // Inside the centre disc the last hovered wedge stays selected, so its
        // detail and the info button remain reachable while the cursor travels
        // in to press them.
        if ((mouse - center).sqrMagnitude <= RadialLayout.CenterRadius * RadialLayout.CenterRadius) {
            return;
        }

        switch (state.Kind) {
            case RadialStateKind.SystemTier:
                state.HoveredIndex = RadialLayout.HitTest(
                    mouse,
                    center,
                    RadialLayout.SystemRingInner,
                    RadialLayout.SystemRingOuter,
                    snapshot.Systems.Count
                );
                break;
            case RadialStateKind.SubsectionTier: {
                    RadialSystem sys = snapshot.Systems[state.SelectedSystemIndex];
                    state.HoveredIndex = RadialLayout.HitTest(
                        mouse,
                        center,
                        RadialLayout.SubsectionRingInner,
                        RadialLayout.SubsectionRingOuter,
                        sys.Subsections.Count
                    );
                    break;
                }
            case RadialStateKind.AbilityTier: {
                    RadialSubsection sub = snapshot
                        .Systems[state.SelectedSystemIndex]
                        .Subsections[state.SelectedSubsectionIndex];
                    state.HoveredIndex = RadialLayout.HitTest(
                        mouse,
                        center,
                        RadialLayout.AbilityRingInner,
                        RadialLayout.AbilityRingOuter,
                        sub.Leaves.Count
                    );
                    break;
                }
        }
    }

    private void DrawRings(Vector2 center) {
        if (state.Kind == RadialStateKind.SystemTier) {
            RadialRingRenderer.DrawSystemRing(center, snapshot.Systems, state.HoveredIndex);
            return;
        }

        RadialSystem sys = snapshot.Systems[state.SelectedSystemIndex];

        if (state.Kind == RadialStateKind.SubsectionTier) {
            RadialRingRenderer.DrawSubsectionRing(center, sys, state.HoveredIndex);
            return;
        }

        RadialSubsection sub = sys.Subsections[state.SelectedSubsectionIndex];
        ISystemSkin skin = SystemSkinRegistry.ForOrFallback(sys.SystemId);
        RadialRingRenderer.DrawAbilityRing(center, sub, skin, state.HoveredIndex);
    }

    private void HandleInput() {
        Event e = Event.current;
        if (e == null) return;

        if (e.type == EventType.MouseDown && e.button == 1) {
            if (state.Kind == RadialStateKind.SystemTier) {
                Close(false);
            }
            else {
                state.Back();
            }

            RimWorld.SoundDefOf.Click.PlayOneShotOnCamera();
            e.Use();
            return;
        }

        if (e.type == EventType.MouseDown && e.button == 0 && state.HoveredIndex >= 0 && !CursorInCentre()) {
            Advance();
            RimWorld.SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
            e.Use();
        }
    }

    private void Advance() {
        switch (state.Kind) {
            case RadialStateKind.SystemTier:
                state.SelectedSystemIndex = state.HoveredIndex;
                state.Kind = RadialStateKind.SubsectionTier;
                state.HoveredIndex = -1;
                AutoSkipOneOptionTiers();
                break;
            case RadialStateKind.SubsectionTier: {
                state.SelectedSubsectionIndex = state.HoveredIndex;
                state.Kind = RadialStateKind.AbilityTier;
                RadialSubsection chosen = snapshot
                    .Systems[state.SelectedSystemIndex]
                    .Subsections[state.SelectedSubsectionIndex];
                if (chosen.Leaves.Count == 1) {
                    state.HoveredIndex = 0;
                    CommitAndClose(ShiftHeld());
                    return;
                }

                state.HoveredIndex = -1;
                break;
            }
            case RadialStateKind.AbilityTier:
                CommitAndClose(false);
                break;
        }
    }

    private void AutoSkipOneOptionTiers() {
        while (true) {
            if (state.Kind == RadialStateKind.SystemTier && snapshot.Systems.Count == 1) {
                state.SelectedSystemIndex = 0;
                state.Kind = RadialStateKind.SubsectionTier;
                continue;
            }

            if (state.Kind == RadialStateKind.SubsectionTier) {
                RadialSystem sys = snapshot.Systems[state.SelectedSystemIndex];
                if (sys.Subsections.Count == 1) {
                    state.SelectedSubsectionIndex = 0;
                    state.Kind = RadialStateKind.AbilityTier;
                    continue;
                }

            }

            break;
        }

        state.HoveredIndex = -1;
    }

    public void CommitAndClose(bool flareShift) {
        RadialLeaf? leaf = state.ResolveHoveredLeaf(snapshot);
        if (leaf != null) {
            string subId = snapshot
                .Systems[state.SelectedSystemIndex]
                .Subsections[state.SelectedSubsectionIndex]
                .SubsectionId;
            RadialDispatcher.Dispatch(snapshot.Pawn, leaf, subId, flareShift);
        }

        Close(false);
    }

    private bool CursorInCentre() {
        // Called both from inside the window, where the event is window-local,
        // and from the hotkey poll outside it, so always measure in screen space.
        Vector2 mouse = Verse.UI.MousePositionOnUIInverted;
        return (mouse - windowRect.center).sqrMagnitude <= RadialLayout.CenterRadius * RadialLayout.CenterRadius;
    }

    private static bool ShiftHeld() {
        return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
    }

    public void TryCommitOnRelease(bool flareShift) {
        if (CursorInCentre()) {
            Close(false);
            return;
        }

        if (state.Kind == RadialStateKind.AbilityTier && state.HoveredIndex >= 0) {
            CommitAndClose(flareShift);
            return;
        }

        if (state.Kind == RadialStateKind.SubsectionTier && state.HoveredIndex >= 0) {
            RadialSubsection hovered = snapshot
                .Systems[state.SelectedSystemIndex]
                .Subsections[state.HoveredIndex];
            if (hovered.Leaves.Count == 1) {
                state.SelectedSubsectionIndex = state.HoveredIndex;
                state.Kind = RadialStateKind.AbilityTier;
                state.HoveredIndex = 0;
                CommitAndClose(flareShift);
                return;
            }
        }

        Close(false);
    }

    public override void PostClose() {
        base.PostClose();
        RadialController.NotifyClosed(this);

        if (restoreSpeed == null) return;

        Find.TickManager.CurTimeSpeed = restoreSpeed.Value;
        restoreSpeed = null;
    }
}