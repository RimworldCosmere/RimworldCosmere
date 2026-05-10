using Cosmere.Lightweave.Layout;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;

namespace Cosmere.Lightweave.MainMenu;

public static class MainMenuRoot {
    public static LightweaveNode Build(
        bool anyMapFiles
    ) {
        SaveMetadata.LatestSave? save = SaveMetadata.Get();
        LightweaveNode? expansions = ExpansionRow.Create();

        LightweaveNode titleBlock = Container.Create(
            Stack.Create(
                SpacingScale.Lg,
                s => {
                    s.Add(StaggerIn.Wrap(TitleHero.Create(), 0.06f));
                    if (expansions != null) {
                        s.Add(StaggerIn.Wrap(BuildExpansionsBlock(expansions), 0.10f));
                    }
                }
            ),
            new Rem(60f),
            new EdgeInsets(Left: SpacingScale.Lg, Right: SpacingScale.Lg, Top: new Rem(0.625f))
        );

        LightweaveNode topBar = HStack.Create(
            SpacingScale.None,
            h => {
                h.Add(StaggerIn.Wrap(MetadataTable.Create(save), 0f), new Rem(22f).ToPixels());
                h.AddFlex(titleBlock);
                h.Add(Spacer.Fixed(new Rem(16f)), new Rem(16f).ToPixels());
            }
        );

        LightweaveNode bottomBlock = Container.Create(
            Stack.Create(
                SpacingScale.Md,
                s => {
                    s.Add(StaggerIn.Wrap(ContinueCard.Create(save), 0.18f));
                    s.Add(StaggerIn.Wrap(MenuButtons.Create(anyMapFiles), 0.24f));
                }
            ),
            new Rem(80f),
            new EdgeInsets(Left: SpacingScale.Lg, Right: SpacingScale.Lg)
        );

        return Stack.Create(
            SpacingScale.None,
            root => {
                root.Add(Spacer.Fixed(new Rem(1.375f)));
                root.Add(topBar);
                root.AddFlex(Spacer.Flex());
                root.Add(bottomBlock);
                root.Add(Spacer.Fixed(new Rem(6.6875f)));
                root.Add(FootStrip.Create());
            }
        );
    }

    private static LightweaveNode BuildExpansionsBlock(LightweaveNode row) {
        return row;
    }
}