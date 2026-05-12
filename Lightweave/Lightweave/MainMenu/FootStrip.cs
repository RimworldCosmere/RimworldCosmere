using Cosmere.Lightweave.Layout;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using Verse;

namespace Cosmere.Lightweave.MainMenu;

public static class FootStrip {
    public static LightweaveNode Create() {
        return Box.Create(
            children: c => c.Add(HStack.Create(SpacingScale.Md, h => {
                h.AddFlex(Spacer.Flex());
                h.AddHug(LangButton.Create());
                h.AddHug(FootLink.Create(
                    label: "CL_MainMenu_Credits".Translate(),
                    onClick: MainMenuActions.OpenCredits
                ));
            })),
            style: new Style {
                Padding = new EdgeInsets(
                    Left: SpacingScale.Lg,
                    Right: SpacingScale.Lg,
                    Top: SpacingScale.Sm,
                    Bottom: SpacingScale.Sm
                ),
            }
        );
    }
}
