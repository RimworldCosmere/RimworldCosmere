using System;
using System.Collections.Generic;
using System.Linq;
using Cosmere.Lightweave.Input;
using Cosmere.Lightweave.Layout;
using Cosmere.Lightweave.Navigation;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using UnityEngine;
using Verse;
using TypoText = Cosmere.Lightweave.Typography.Typography.Text;
using Eyebrow = Cosmere.Lightweave.Typography.Eyebrow;

namespace Cosmere.Lightweave.MainMenu;

public static class LangPopover {
    public static LightweaveNode Create(
        string query,
        Action<string> onQueryChange,
        Action onDismiss
    ) {
        List<LoadedLanguage> all = LanguageDatabase.AllLoadedLanguages.ToList();
        List<LoadedLanguage> filtered = Filter(all, query);

        return Box.Create(
            children: c => c.Add(Stack.Create(SpacingScale.None, s => {
                s.Add(BuildHeader(all.Count));
                s.Add(BuildSpacer(SpacingScale.Sm));
                s.Add(SearchField.Create(
                    value: query,
                    onChange: onQueryChange,
                    placeholder: "CL_MainMenu_Lang_SearchPlaceholder".Translate()
                ));
                s.Add(BuildSpacer(SpacingScale.Sm));
                s.Add(Divider.Horizontal());
                s.Add(BuildSpacer(SpacingScale.Xs));

                if (filtered.Count == 0) {
                    s.Add(BuildEmpty());
                } else {
                    s.Add(BuildList(filtered, onDismiss));
                }
            })),
            style: new Style {
                Padding = new EdgeInsets(Top: SpacingScale.Md, Bottom: SpacingScale.Md, Left: SpacingScale.Md, Right: SpacingScale.Md),
            }
        );
    }

    private static LightweaveNode BuildList(List<LoadedLanguage> langs, Action onDismiss) {
        return Stack.Create(SpacingScale.None, s => {
            for (int i = 0; i < langs.Count; i++) {
                LoadedLanguage lang = langs[i];
                s.Add(MenuRow.Create(
                    FormatLabel(lang),
                    () => {
                        LanguageDatabase.SelectLanguage(lang);
                        onDismiss?.Invoke();
                    }
                ));
            }
        });
    }

    private static LightweaveNode BuildHeader(int total) {
        return HStack.Create(SpacingScale.Sm, h => {
            h.AddFlex(Eyebrow.Create("CL_MainMenu_Lang_HeaderTitle".Translate()));
            h.Add(Eyebrow.Create("CL_MainMenu_Lang_HeaderCount".Translate(total.Named("COUNT"))), new Rem(8f).ToPixels());
        });
    }

    private static LightweaveNode BuildEmpty() {
        return Box.Create(
            children: c => c.Add(TypoText.Create(
                "CL_MainMenu_Lang_NoMatches".Translate(),
                style: new Style { TextColor = ThemeSlot.TextMuted, TextAlign = TextAlign.Center }
            )),
            style: new Style {
                Padding = new EdgeInsets(Top: SpacingScale.Md, Bottom: SpacingScale.Md, Left: SpacingScale.Md, Right: SpacingScale.Md),
            }
        );
    }

    private static LightweaveNode BuildSpacer(Rem size) {
        return Spacer.Fixed(size);
    }

    private static string FormatLabel(LoadedLanguage lang) {
        string native = lang.FriendlyNameNative ?? lang.folderName;
        string english = lang.FriendlyNameEnglish;
        if (string.IsNullOrEmpty(english) || string.Equals(english, native, StringComparison.OrdinalIgnoreCase)) {
            return native;
        }
        return native + "  ·  " + english;
    }

    private static List<LoadedLanguage> Filter(List<LoadedLanguage> all, string query) {
        if (string.IsNullOrWhiteSpace(query)) {
            return all;
        }
        string q = query.Trim();
        return all.Where(l =>
            (l.FriendlyNameNative != null && l.FriendlyNameNative.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0) ||
            (l.FriendlyNameEnglish != null && l.FriendlyNameEnglish.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0) ||
            (l.folderName != null && l.folderName.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0) ||
            (l.LegacyFolderName != null && l.LegacyFolderName.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0)
        ).ToList();
    }
}
