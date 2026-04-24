using Cosmere.Core.UI.Skin;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Cosmere.Core.UI.Radial;

public sealed class RadialWindow : Verse.Window {
    private readonly RadialState state = new RadialState();
    private RadialSnapshot snapshot;

    public RadialWindow(RadialSnapshot snapshot) {
        this.snapshot = snapshot;
        doCloseButton = false;
        doCloseX = false;
        closeOnClickedOutside = false;
        closeOnAccept = false;
        closeOnCancel = false;
        preventCameraMotion = false;
        draggable = false;
        drawShadow = false;
        layer = WindowLayer.Super;
        focusWhenOpened = false;
        forcePause = false;
        state.Kind = RadialStateKind.SystemTier;
        AutoSkipOneOptionTiers();
    }

    protected override float Margin => 0f;

    public override Vector2 InitialSize => new Vector2(Verse.UI.screenWidth, Verse.UI.screenHeight);

    protected override void SetInitialSizeAndPosition() {
        windowRect = new Rect(0f, 0f, Verse.UI.screenWidth, Verse.UI.screenHeight);
    }

    public override void DoWindowContents(Rect inRect) {
        if (!snapshot.Pawn.Spawned || snapshot.Pawn.Dead) {
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

        Vector2 center = RadialAnchor.PawnScreenCenter(snapshot.Pawn);
        Vector2 mouse = Event.current.mousePosition;

        UpdateHover(center, mouse);
        DrawRings(center);
        RadialCenterPreview.Draw(center, state.ResolveHoveredLeaf(snapshot));
        HandleInput();
    }

    public override void OnCancelKeyPressed() {
        Close(false);
        Event.current?.Use();
    }

    private void UpdateHover(Vector2 center, Vector2 mouse) {
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
        ISystemSkin skin = SystemSkinRegistry.For(sys.SystemId);
        RadialRingRenderer.DrawAbilityRing(center, sub, skin, state.HoveredIndex);
    }

    private void HandleInput() {
        Event e = Event.current;
        if (e == null) return;

        if (e.type == EventType.MouseDown && e.button == 1) {
            if (state.Kind == RadialStateKind.SystemTier) {
                Close(false);
            } else {
                state.Back();
            }

            RimWorld.SoundDefOf.Click.PlayOneShotOnCamera();
            e.Use();
            return;
        }

        if (e.type == EventType.MouseDown && e.button == 0 && state.HoveredIndex >= 0) {
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
            case RadialStateKind.SubsectionTier:
                state.SelectedSubsectionIndex = state.HoveredIndex;
                state.Kind = RadialStateKind.AbilityTier;
                state.HoveredIndex = -1;
                break;
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

    public void TryCommitOnRelease(bool flareShift) {
        if (state.Kind == RadialStateKind.AbilityTier && state.HoveredIndex >= 0) {
            CommitAndClose(flareShift);
            return;
        }

        Close(false);
    }
}