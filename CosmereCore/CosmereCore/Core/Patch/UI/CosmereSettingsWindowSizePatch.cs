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

    protected CosmereSettingsWindowSizePatch(Verse.Mod mod) : base(mod) { }

    [Inject(At.Head, nameof(DoWindowContents))]
    private Control BeforeDoWindowContents(Rect inRect) {
        if (settingsMod is not Mod mod) return Control.Continue;

        doCloseButton = false;
        GameFont previousFont = Text.Font;
        try {
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(0f, 0f, inRect.width - 167f, 35f), settingsMod.SettingsCategory());
            Text.Font = GameFont.Small;
            Rect content = new Rect(0f, 40f, inRect.width, inRect.height - 40f);
            settingsMod.DoSettingsWindowContents(content);

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
