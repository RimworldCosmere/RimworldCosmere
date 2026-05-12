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

        float maxHeightPx = new Rem(26.5625f).ToPixels();

        LightweaveNode header = BuildHeader(all.Count);
        LightweaveNode searchField = SearchField.Create(
            value: query,
            onChange: onQueryChange,
            placeholder: "CL_MainMenu_Lang_SearchPlaceholder".Translate()
        );
        LightweaveNode divider = Divider.Horizontal();
        LightweaveNode listOrEmpty;
        if (filtered.Count == 0) {
            listOrEmpty = BuildEmpty();
        }
        else {
            listOrEmpty = ScrollArea.Create(BuildList(filtered, onDismiss));
        }

        Color bgColor = new Color(15f / 255f, 12f / 255f, 8f / 255f, 0.96f);

        LightweaveNode stack = Stack.Create(SpacingScale.None, s => {
            s.Add(header);
            s.Add(BuildSpacer(SpacingScale.Sm));
            s.Add(searchField);
            s.Add(BuildSpacer(SpacingScale.Sm));
            s.Add(divider);
            s.Add(BuildSpacer(SpacingScale.Xs));
            s.AddFlex(listOrEmpty);
        });

        LightweaveNode box = Box.Create(
            children: c => c.Add(stack),
            style: new Style {
                Padding = new EdgeInsets(Top: SpacingScale.Md, Bottom: SpacingScale.Md, Left: SpacingScale.Md, Right: SpacingScale.Md),
                Background = BackgroundSpec.Of(bgColor),
                Radius = RadiusSpec.All(RadiusScale.Lg),
            }
        );

        float fixedHeight = SpacingScale.Md.ToPixels() * 2f
            + (header.Measure?.Invoke(0f) ?? header.PreferredHeight ?? new Rem(1.5f).ToPixels())
            + SpacingScale.Sm.ToPixels()
            + (searchField.PreferredHeight ?? new Rem(1.75f).ToPixels())
            + SpacingScale.Sm.ToPixels()
            + (divider.PreferredHeight ?? new Rem(1f / 16f).ToPixels())
            + SpacingScale.Xs.ToPixels();

        float rowH = new Rem(1.65f).ToPixels();
        float listNatural = filtered.Count == 0
            ? new Rem(3f).ToPixels()
            : filtered.Count * rowH;

        float total = fixedHeight + listNatural;
        float capped = Mathf.Min(total, maxHeightPx);

        box.PreferredHeight = capped;
        box.Measure = _ => capped;

        return box;
    }

    private static LightweaveNode BuildList(List<LoadedLanguage> langs, Action onDismiss) {
        return Stack.Create(SpacingScale.None, s => {
            for (int i = 0; i < langs.Count; i++) {
                LoadedLanguage lang = langs[i];
                s.Add(MenuItem.Create(
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
        Style eyebrowStyle = new Style {
            FontFamily = FontRole.Mono,
            FontSize = new Rem(0.625f),
            LetterSpacing = Tracking.Of(0.18f),
            TextColor = ThemeSlot.TextMuted,
        };
        Style countStyle = new Style {
            FontFamily = FontRole.Mono,
            FontSize = new Rem(0.625f),
            LetterSpacing = Tracking.Of(0.18f),
            TextColor = ThemeSlot.TextMuted,
            TextAlign = TextAlign.End,
        };
        return HStack.Create(SpacingScale.Sm, h => {
            h.AddFlex(Eyebrow.Create("CL_MainMenu_Lang_HeaderTitle".Translate(), style: eyebrowStyle));
            h.Add(Eyebrow.Create("CL_MainMenu_Lang_HeaderCount".Translate(total.Named("COUNT")), style: countStyle), new Rem(8f).ToPixels());
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
