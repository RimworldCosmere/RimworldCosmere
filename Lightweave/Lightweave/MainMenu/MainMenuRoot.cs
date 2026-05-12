using Cosmere.Lightweave.Layout;
using Cosmere.Lightweave.Rendering;
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
            style: new Style {
                Padding = new EdgeInsets(Left: SpacingScale.Lg, Right: SpacingScale.Lg, Top: new Rem(0.625f)),
            }
        );

        LightweaveNode topBar = Stack.Create(
            SpacingScale.None,
            s => {
                s.Add(titleBlock);
                s.Add(
                    StaggerIn.Wrap(MetadataTable.Create(save), 0f).WithStyle(
                        new Style {
                            Position = Position.Absolute,
                            Top = new Rem(0f),
                            Left = new Rem(0f),
                        }
                    )
                );
            }
        ).WithStyle(new Style { Position = Position.Relative });

        LightweaveNode bottomBlock = Container.Create(
            Stack.Create(
                new Rem(3f),
                s => {
                    s.Add(
                        StaggerIn.Wrap(ContinueCard.Create(save), 0.18f).WithStyle(
                            new Style {
                                Position = Position.Relative,
                                Top = new Rem(0.625f),
                            }
                        )
                    );
                    s.Add(
                        StaggerIn.Wrap(MenuButtons.Create(anyMapFiles), 0.24f).WithStyle(
                            new Style {
                                Position = Position.Relative,
                                Top = new Rem(-0.625f),
                            }
                        )
                    );
                }
            ),
            style: new Style {
                MaxWidth = new Rem(80f),
                Padding = new EdgeInsets(Left: SpacingScale.Lg, Right: SpacingScale.Lg),
            }
        );

        LightweaveNode content = Stack.Create(
            SpacingScale.None,
            root => {
                root.Add(Spacer.Fixed(new Rem(1.375f)));
                root.Add(topBar);
                root.AddFlex(Spacer.Flex());
                root.Add(bottomBlock);
                root.Add(Spacer.Fixed(new Rem(2.9375f)));
                root.Add(FootStrip.Create());
            }
        );

        return Vignette.Create(
            content,
            shape: VignetteShape.Radial,
            intensity: 0.75f,
            scale: 1.5f,
            color: ThemeSlot.OverlayDim
        );
    }
}
