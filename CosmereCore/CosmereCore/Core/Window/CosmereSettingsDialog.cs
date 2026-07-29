using Cosmere.Core.Settings.Layout;
using UnityEngine;
using Verse;

namespace Cosmere.Core.Window;

// Dialog_ModSettings hands DoWindowContents a rect already contracted by Window.Margin
// and opens a group that clips anything drawn back outside, so a surface that paints its
// own crest, sidebar and footer to the edge cannot be built inside it. This window exists
// to own that edge.
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

    // Dialog_ModSettings persisted on close and nothing else does, so this window carries
    // that responsibility now.
    public override void PreClose() {
        base.PreClose();
        mod.ClearSettingsResetConfirmation();
        mod.WriteSettings();
    }
}
