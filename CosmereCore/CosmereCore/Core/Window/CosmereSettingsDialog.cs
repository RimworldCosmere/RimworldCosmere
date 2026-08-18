using Cosmere.Core.Settings.Layout;
using UnityEngine;
using Verse;

namespace Cosmere.Core.Window;

/// <summary>
///     Owns the window edge itself: Dialog_ModSettings clips drawing to a rect inside its own
///     margin, so a surface that paints its own crest, sidebar, and footer to the edge cannot live inside it.
/// </summary>
public sealed class CosmereSettingsDialog : global::Verse.Window {
    private readonly Core.Mod mod;

    public CosmereSettingsDialog(Core.Mod mod) {
        this.mod = mod;
        forcePause = true;
        absorbInputAroundWindow = true;
        closeOnClickedOutside = true;

        // The crest draws its own close mark and the footer its own Close button.
        doCloseX = false;
        doCloseButton = false;
    }

    public override Vector2 InitialSize => new Vector2(
        SettingsWindowLayoutMath.PreferredWindowWidth(global::Verse.UI.screenWidth),
        SettingsWindowLayoutMath.PreferredWindowHeight(global::Verse.UI.screenHeight)
    );

    protected override float Margin => 0f;

    public override void DoWindowContents(Rect inRect) {
        GameFont previousFont = Text.Font;
        try {
            Text.Font = GameFont.Small;
            mod.DoSettingsWindowContents(inRect);

            if (mod.ConsumeSettingsCloseRequest()) Close();
        } finally {
            Text.Font = previousFont;
        }
    }

    /// <summary>
    ///     Persists settings on close. Dialog_ModSettings did this and nothing else does now.
    /// </summary>
    public override void PreClose() {
        base.PreClose();
        mod.ClearSettingsResetConfirmation();
        mod.WriteSettings();
    }
}
