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
                        s.Add(StaggerIn.Wrap(expansions, 0.10f));
                    }
                }
            ),
            padding: new EdgeInsets(Left: SpacingScale.Lg, Right: SpacingScale.Lg, Top: new Rem(0.625f))
        );

        LightweaveNode topBar = HStack.Create(
            SpacingScale.None,
            h => {
                h.AddHug(StaggerIn.Wrap(MetadataTable.Create(save), 0f));
                h.AddFlex(titleBlock);
            }
        );

        LightweaveNode bottomBlock = Container.Create(
            Stack.Create(
                SpacingScale.Lg,
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
}
