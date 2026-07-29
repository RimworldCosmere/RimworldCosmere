using Concord;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.Core.Patch;

[Patch]
public abstract class CosmereSettingsWindowSizePatch : Dialog_ModSettings {
    [InjectField("mod")]
    private readonly Verse.Mod settingsMod = null!;

    [InjectField(nameof(global::Verse.Window.doCloseButton))]
    private new bool doCloseButton;

    [InjectField(nameof(global::Verse.Window.doCloseX))]
    private new bool doCloseX;

    protected CosmereSettingsWindowSizePatch(Verse.Mod mod) : base(mod) { }

    [Inject(At.Head, nameof(DoWindowContents))]
    private Control BeforeDoWindowContents(Rect inRect) {
        if (settingsMod is not Mod mod) return Control.Continue;

        // The window draws its own title in the sidebar and its own close in the top bar,
        // so both pieces of vanilla chrome are suppressed and the whole rect is ours.
        doCloseButton = false;
        doCloseX = false;

        GameFont previousFont = Text.Font;
        try {
            Text.Font = GameFont.Small;
            settingsMod.DoSettingsWindowContents(inRect);

            if (mod.ConsumeSettingsCloseRequest()) Close();
            return Control.Cancel;
        } finally {
            Text.Font = previousFont;
        }
    }

    [Inject(At.Head, nameof(PreClose))]
    private void BeforePreClose() {
        if (settingsMod is Mod mod) mod.ClearSettingsResetConfirmation();
    }

    [Inject(At.Return, "get_" + nameof(Margin))]
    private void AfterMargin(ControlHandle<float> ch) {
        if (settingsMod is not Mod) return;

        // Window contracts its rect by Margin before handing it to DoWindowContents, which
        // rings the whole surface in 18px of dead space. The sidebar, tabs and footer draw
        // their own edges, so the body wants the full rect.
        ch.ReturnValue = 0f;
    }

    [Inject(At.Return, nameof(InitialSize))]
    private void AfterInitialSize(ControlHandle<Vector2> ch) {
        if (settingsMod is not Mod) return;

        float targetWidth = Mathf.Min(1200f, Verse.UI.screenWidth - 40f);
        float targetHeight = Mathf.Min(1000f, Verse.UI.screenHeight - 40f);
        ch.ReturnValue = new Vector2(
            Mathf.Max(ch.ReturnValue.x, targetWidth),
            Mathf.Max(ch.ReturnValue.y, targetHeight)
        );
    }
}
