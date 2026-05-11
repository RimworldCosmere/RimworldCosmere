using System;
using System.Runtime.CompilerServices;
using Cosmere.Lightweave.Hooks;
using static Cosmere.Lightweave.Hooks.Hooks;
using Cosmere.Lightweave.Navigation;
using Cosmere.Lightweave.Patch;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Types;
using UnityEngine;
using Verse;

namespace Cosmere.Lightweave.MainMenu;

public static class MoreButton {
    private static readonly Rem TileHeight = new Rem(4.0f);
    private static readonly Rem MenuWidth = new Rem(18f);

    public static LightweaveNode Create(
        Style? style = null,
        string[]? classes = null,
        string? id = null,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        StateHandle<bool> open = UseState(false, line, file);
        StateHandle<Rect> anchor = UseState(Rect.zero, line + 1, file);

        LightweaveNode node = NodeBuilder.New("MoreButton", line, file);
        node.ApplyStyling("more-button", style, classes, id);
        node.PreferredHeight = TileHeight.ToPixels();

        Action toggle = () => open.Set(!open.Value);

        LightweaveNode tile = DockTile.Create(
            "CL_MainMenu_More".Translate(),
            hotkey: string.Empty,
            toggle,
            disabled: false,
            chevron: true
        );

        LightweaveNode menu = Menu.Create(
            isOpen: open.Value,
            anchorRect: anchor.Value,
            items: BuildItems(() => open.Set(false)),
            onDismiss: () => open.Set(false),
            anchor: MenuAnchor.Left,
            direction: MenuDirection.Up,
            instanceKey: "more-menu",
            header: (string)"CL_MainMenu_More_Header".Translate(),
            headerMeta: null,
            searchPlaceholder: null,
            size: new Vector2(MenuWidth.ToPixels(), -1f)
        );

        node.Children.Add(tile);
        node.Children.Add(menu);

        node.Paint = (rect, _) => {
            anchor.Set(rect);
            tile.MeasuredRect = rect;
            menu.MeasuredRect = rect;
            LightweaveRoot.PaintSubtree(tile, rect);
            LightweaveRoot.PaintSubtree(menu, rect);
        };

        return node;
    }

    public static IReadOnlyList<MenuItem> BuildItems(Action onDismiss) {
        List<MenuItem> items = new List<MenuItem> {
            Menu.Item(
                (string)"CL_MainMenu_Playground".Translate(),
                () => Run(DevPlaygroundButtonPatch.OpenPlayground, onDismiss),
                subtitle: (string)"CL_MainMenu_More_Sub_Playground".Translate()
            ),
            Menu.Divider(),
            Menu.Item(
                (string)"CL_MainMenu_Credits".Translate(),
                () => Run(MainMenuActions.OpenCredits, onDismiss),
                subtitle: (string)"CL_MainMenu_More_Sub_Credits".Translate()
            ),
            Menu.Item(
                (string)"CL_MainMenu_SteamWorkshop".Translate(),
                () => Run(OpenSteamWorkshop, onDismiss),
                subtitle: (string)"CL_MainMenu_More_Sub_SteamWorkshop".Translate()
            ),
            Menu.Item(
                (string)"CL_MainMenu_OfficialDiscord".Translate(),
                () => Run(OpenOfficialDiscord, onDismiss),
                subtitle: (string)"CL_MainMenu_More_Sub_OfficialDiscord".Translate()
            ),
            Menu.Item(
                (string)"CL_MainMenu_WikiTutorials".Translate(),
                () => Run(OpenWiki, onDismiss),
                subtitle: (string)"CL_MainMenu_More_Sub_WikiTutorials".Translate()
            ),
            Menu.Item(
                (string)"CL_MainMenu_BugReport".Translate(),
                () => Run(OpenBugReport, onDismiss),
                subtitle: (string)"CL_MainMenu_More_Sub_BugReport".Translate()
            ),
            Menu.Divider(),
            Menu.Item(
                (string)"CL_MainMenu_EulaLicenses".Translate(),
                () => Run(OpenEula, onDismiss)
            ),
            Menu.Item(
                (string)"CL_MainMenu_PrivacyPolicy".Translate(),
                () => Run(OpenPrivacy, onDismiss)
            ),
        };

        if (Prefs.DevMode) {
            items.Add(Menu.Divider());
            items.Add(Menu.Item(
                (string)"CL_MainMenu_DevQuickTest".Translate(),
                () => Run(MainMenuActions.DevQuickTest, onDismiss)
            ));
            if (LanguageDatabase.activeLanguage == LanguageDatabase.defaultLanguage &&
                LanguageDatabase.activeLanguage.anyError) {
                items.Add(Menu.Item(
                    (string)"CL_MainMenu_TranslationReport".Translate(),
                    () => Run(MainMenuActions.SaveTranslationReport, onDismiss)
                ));
            }
        }

        items.Add(Menu.Divider());
        items.Add(Menu.Item(
            (string)"CL_MainMenu_QuitToOS".Translate(),
            () => Run(MainMenuActions.QuitToOS, onDismiss),
            danger: true,
            subtitle: (string)"CL_MainMenu_More_Sub_QuitToOS".Translate()
        ));

        return items;
    }

    private static void Run(Action action, Action onDismiss) {
        action?.Invoke();
        onDismiss?.Invoke();
    }

    private static void OpenSteamWorkshop() {
        Application.OpenURL("https://steamcommunity.com/app/294100/workshop/");
    }

    private static void OpenOfficialDiscord() {
        Application.OpenURL("https://discord.gg/rimworld");
    }

    private static void OpenWiki() {
        Application.OpenURL("https://rimworldwiki.com/");
    }

    private static void OpenBugReport() {
        Application.OpenURL("https://ludeon.com/forums/index.php?board=8.0");
    }

    private static void OpenEula() {
        Application.OpenURL("https://rimworldgame.com/eula/");
    }

    private static void OpenPrivacy() {
        Application.OpenURL("https://rimworldgame.com/privacy/");
    }
}
