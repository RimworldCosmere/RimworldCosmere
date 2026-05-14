using System;
using Cosmere.Lightweave.Input;
using Cosmere.Lightweave.Layout;
using Cosmere.Lightweave.Overlay;
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

public class Dialog_DeleteSaveConfirm : Window {
    private readonly Guid rootId = Guid.NewGuid();

    private readonly SaveFileInfo file;
    private readonly string displayName;
    private readonly Action onConfirm;

   public Dialog_DeleteSaveConfirm(SaveFileInfo file, string displayName, Action onConfirm) {
        this.file = file;
        this.displayName = displayName;
        this.onConfirm = onConfirm;
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
        if (acceptPressed) {
            Confirm();
        }

        LightweaveNode card = Stack.Create(SpacingScale.Sm, c => {
            c.Add(Eyebrow.Create("CL_LoadColony_Delete".Translate()));
            c.Add(Typography.Typography.Heading.Create(3, displayName));
            c.Add(Spacer.Fixed(SpacingScale.Xs));
            c.Add(Text.Create("CL_LoadColony_Delete_Confirm".Translate()));
            c.AddFlex(Spacer.Flex());
            c.Add(HStack.Create(SpacingScale.Sm, h => {
                h.AddFlex(Spacer.Flex());
                h.AddHug(Button.Create(
                    label: "CancelButton".Translate(),
                    onClick: () => Close(),
                    variant: ButtonVariant.Secondary
                ));
                h.AddHug(Button.Create(
                    label: "CL_LoadColony_Delete".Translate(),
                    onClick: Confirm,
                    variant: ButtonVariant.Danger
                ));
            }, style: new Style { Height = new Rem(2.5f) }));
        }, style: new Style { Padding = EdgeInsets.All(SpacingScale.Md) });

        return Dialog.Create(
            content: () => card,
            width: 480f,
            height: 240f,
            scrimColor: new Color(0f, 0f, 0f, 0.25f),
            cardBackground: BackgroundSpec.Blur(new Color(0f, 0f, 0f, 0.95f), 10f),
            vignetteIntensity: 0.35f,
            vignetteScale: 0.9f
        );
    }

    private void Confirm() {
        try {
            onConfirm?.Invoke();
        }
        catch (Exception ex) {
            LightweaveLog.Error("Delete save failed: " + ex);
        }
        Close();
    }
}
