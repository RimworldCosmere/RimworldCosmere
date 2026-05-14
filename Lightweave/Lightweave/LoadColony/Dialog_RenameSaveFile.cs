using System;
using System.IO;
using Cosmere.Lightweave.Hooks;
using Cosmere.Lightweave.Input;
using Cosmere.Lightweave.Layout;
using Cosmere.Lightweave.Overlay;
using Cosmere.Lightweave.Rendering;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using RimWorld;
using UnityEngine;
using Verse;
using Display = Cosmere.Lightweave.Typography.Display;
using Eyebrow = Cosmere.Lightweave.Typography.Eyebrow;
using Text = Cosmere.Lightweave.Typography.Typography.Text;

namespace Cosmere.Lightweave.LoadColony;

public class Dialog_RenameSaveFile : Window {
    private readonly Guid rootId = Guid.NewGuid();

    private readonly SaveFileInfo file;
    private readonly string originalName;
    private readonly Action onAfter;

    private string newName;
    private string? validationError;
    private bool focusRequested;

    public Dialog_RenameSaveFile(SaveFileInfo file, string currentName, Action onAfter) {
        this.file = file;
        this.originalName = currentName;
        this.newName = currentName;
        this.onAfter = onAfter;
        doCloseX = false;
        doCloseButton = false;
        forcePause = true;
        absorbInputAroundWindow = false;
        closeOnAccept = false;
        closeOnCancel = true;
        closeOnClickedOutside = false;
        layer = WindowLayer.Super;
        drawShadow = false;
        doWindowBackground = false;
    }

    public override Vector2 InitialSize => new Vector2(UI.screenWidth, UI.screenHeight);

    protected override float Margin => 0f;

    public override void DoWindowContents(Rect inRect) {
        bool acceptPressed = false;
        if (Event.current.type == EventType.KeyDown &&
            (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter)) {
            acceptPressed = true;
            Event.current.Use();
        }

        LightweaveRoot.Render(inRect, rootId, () => Build(acceptPressed));
    }

    private LightweaveNode Build(bool acceptPressed) {
        UseFocus.FocusHandle focus = UseFocus.Use();
        if (!focusRequested) {
            focus.Request();
            focusRequested = true;
        }

        if (acceptPressed) {
            TryCommit();
        }

        LightweaveNode card = Stack.Create(SpacingScale.Sm, c => {
            c.Add(Eyebrow.Create("CL_LoadColony_Rename".Translate()));
            c.Add(Typography.Typography.Heading.Create(3, originalName));
            c.Add(Text.Create(
                "CL_LoadColony_Rename_Description".Translate(),
                style: new Style { TextColor = new ColorRef.Token(ThemeSlot.TextSecondary) }
            ));
            c.Add(Spacer.Fixed(SpacingScale.Xs));
            c.Add(TextField.Create(
                value: newName,
                onChange: v => {
                    newName = v;
                    validationError = null;
                },
                placeholder: originalName,
                focus: focus
            ));
            if (!string.IsNullOrEmpty(validationError)) {
                c.Add(Text.Create(
                    validationError!,
                    style: new Style { TextColor = new ColorRef.Token(ThemeSlot.StatusDanger) }
                ));
            }
            c.AddFlex(Spacer.Flex());
            c.Add(HStack.Create(SpacingScale.Sm, h => {
                h.AddFlex(Spacer.Flex());
                h.AddHug(Button.Create(
                    label: "CancelButton".Translate(),
                    onClick: () => Close(),
                    variant: ButtonVariant.Secondary
                ));
                h.AddHug(Button.Create(
                    label: "CL_LoadColony_Rename".Translate(),
                    onClick: TryCommit,
                    variant: ButtonVariant.Primary
                ));
            }, style: new Style { Height = new Rem(2.5f) }));
        }, style: new Style { Padding = EdgeInsets.All(SpacingScale.Md) });

        return Dialog.Create(
            content: () => card,
            width: 520f,
            height: 320f,
            scrimColor: new Color(0f, 0f, 0f, 0.25f),
            cardBackground: BackgroundSpec.Blur(new Color(0f, 0f, 0f, 0.95f), 10f),
            vignetteIntensity: 0.35f,
            vignetteScale: 0.9f
        );
    }

    private void TryCommit() {
        string trimmed = (newName ?? string.Empty).Trim();
        if (trimmed.Length == 0) {
            validationError = "NameIsInvalid".Translate();
            return;
        }
        if (trimmed == originalName) {
            Close();
            return;
        }
        if (trimmed.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0) {
            validationError = "NameIsInvalid".Translate();
            return;
        }
        try {
            string dir = file.FileInfo.DirectoryName ?? string.Empty;
            string ext = file.FileInfo.Extension;
            string newPath = Path.Combine(dir, trimmed + ext);
            if (File.Exists(newPath)) {
                validationError = "NameIsInvalid".Translate();
                return;
            }
            string oldSidecar = SaveSidecar.PathFor(file.FileInfo.FullName);
            file.FileInfo.MoveTo(newPath);
            if (File.Exists(oldSidecar)) {
                string newSidecar = SaveSidecar.PathFor(newPath);
                try {
                    File.Move(oldSidecar, newSidecar);
                }
                catch (Exception sidecarEx) {
                    LightweaveLog.Warning($"Rename sidecar failed: {sidecarEx.Message}");
                }
            }
            onAfter?.Invoke();
            Close();
        }
        catch (Exception ex) {
            LightweaveLog.Warning($"Rename save failed: {ex.Message}");
            validationError = ex.Message;
        }
    }
}
