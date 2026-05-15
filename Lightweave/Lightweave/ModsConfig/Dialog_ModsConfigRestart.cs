using System;
using Cosmere.Lightweave.Input;
using Cosmere.Lightweave.Layout;
using Cosmere.Lightweave.Overlay;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using UnityEngine;
using Verse;
using Eyebrow = Cosmere.Lightweave.Typography.Eyebrow;
using Text = Cosmere.Lightweave.Typography.Typography.Text;

namespace Cosmere.Lightweave.ModsConfig;

public class Dialog_ModsConfigRestart : Window {
    private readonly Guid rootId = Guid.NewGuid();

    public Dialog_ModsConfigRestart() {
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
            DoRestart();
        }

        LightweaveNode card = Stack.Create(SpacingScale.Sm, c => {
            c.Add(Eyebrow.Create("CL_ModsConfig_Restart_Eyebrow".Translate()));
            c.Add(Typography.Typography.Heading.Create(3, "CL_ModsConfig_Restart_Heading".Translate()));
            c.Add(Spacer.Fixed(SpacingScale.Xs));
            c.Add(Text.Create("CL_ModsConfig_Restart_Body".Translate()));
            c.AddFlex(Spacer.Flex());
            c.Add(HStack.Create(SpacingScale.Sm, h => {
                h.AddFlex(Spacer.Flex());
                h.AddHug(Button.Create(
                    label: "CL_ModsConfig_Restart_Later".Translate(),
                    onClick: () => Close(),
                    variant: ButtonVariant.Ghost
                ));
                h.AddHug(Button.Create(
                    label: "CL_ModsConfig_Restart_Now".Translate(),
                    onClick: DoRestart,
                    variant: ButtonVariant.Primary
                ));
            }, style: new Style { Height = new Rem(2.5f) }));
        }, style: new Style { Padding = EdgeInsets.All(SpacingScale.Md) });

        return Dialog.Create(
            content: () => card,
            width: 520f,
            height: 240f,
            scrimColor: new Color(0f, 0f, 0f, 0.25f),
            cardBackground: BackgroundSpec.Blur(new Color(0f, 0f, 0f, 0.95f), 10f),
            vignetteIntensity: 0.35f,
            vignetteScale: 0.9f
        );
    }

    private void DoRestart() {
        try {
            GenCommandLine.Restart();
        }
        catch (Exception ex) {
            LightweaveLog.Error("Restart failed: " + ex);
        }
        Close();
    }
}
