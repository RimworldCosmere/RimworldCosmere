using Cosmere.Lightweave.Layout;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Verse;

namespace Cosmere.Lightweave.MainMenu;

public static class MenuButtons {
    private const float DockTileHeightRem = 5.125f;

    public static LightweaveNode Create(
        bool anyMapFiles
    ) {
        return HStack.Create(SpacingScale.Sm, h => {
            h.AddFlex(DockTile.Create("CL_MainMenu_NewColony".Translate(), "N", MainMenuActions.NewColony, false));
            h.AddFlex(DockTile.Create("CL_MainMenu_LoadGame".Translate(), "L", MainMenuActions.OpenLoadDialog, !anyMapFiles));
            h.AddFlex(DockTile.Create("CL_MainMenu_Tutorial".Translate(), string.Empty, MainMenuActions.Tutorial, false));
            h.AddFlex(DockTile.Create("CL_MainMenu_Mods".Translate(), "M", MainMenuActions.OpenMods, false));
            h.AddFlex(DockTile.Create("CL_MainMenu_Options".Translate(), "O", MainMenuActions.OpenOptions, false));
            h.AddFlex(MoreButton.Create());
        });
    }

}
