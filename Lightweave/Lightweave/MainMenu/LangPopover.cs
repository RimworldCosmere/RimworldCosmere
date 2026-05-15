using System;
using System.Collections.Generic;
using System.Linq;
using Cosmere.Lightweave.Input;
using Cosmere.Lightweave.Layout;
using Cosmere.Lightweave.Navigation;
using Cosmere.Lightweave.Rendering;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using UnityEngine;
using Verse;
using Verse.Sound;
using TypoText = Cosmere.Lightweave.Typography.Typography.Text;
using Eyebrow = Cosmere.Lightweave.Typography.Eyebrow;

namespace Cosmere.Lightweave.MainMenu;

public static class LangPopover {
    private static readonly Rem RowHeight = new Rem(3.25f);

    public static LightweaveNode Create(
        string query,
        Action<string> onQueryChange,
        Action onDismiss
    ) {
        List<LoadedLanguage> all = LanguageDatabase.AllLoadedLanguages.ToList();
        LoadedLanguage active = LanguageDatabase.activeLanguage;
        List<LoadedLanguage> filtered = SortSelectedFirst(Filter(all, query), active);

        float maxHeightPx = new Rem(26.5625f).ToPixels();

        LightweaveNode header = WrapHorizontalPad(BuildHeader(all.Count));
        LightweaveNode searchField = WrapHorizontalPad(SearchField.Create(
            value: query,
            onChange: onQueryChange,
            placeholder: "CL_MainMenu_Lang_SearchPlaceholder".Translate(),
            variant: SearchFieldVariant.Frosted
        ));
        LightweaveNode divider = Divider.Horizontal();
        LightweaveNode listOrEmpty;
        if (filtered.Count == 0) {
            listOrEmpty = BuildEmpty();
        }
        else {
            listOrEmpty = ScrollArea.Create(
                BuildList(filtered, active, onDismiss),
                scrollbarMode: ScrollbarMode.Auto
            );
        }

        Color bgColor = new Color(15f / 255f, 12f / 255f, 8f / 255f, 0.96f);

        LightweaveNode stack = Stack.Create(SpacingScale.None, s => {
            s.Add(header);
            s.Add(BuildSpacer(SpacingScale.Sm));
            s.Add(searchField);
            s.Add(BuildSpacer(SpacingScale.Sm));
            s.Add(divider);
            s.AddFlex(listOrEmpty);
        });

        LightweaveNode box = Box.Create(
            children: c => c.Add(stack),
            style: new Style {
                Padding = new EdgeInsets(Top: SpacingScale.Md, Bottom: SpacingScale.Md, Left: SpacingScale.None, Right: SpacingScale.None),
                Background = BackgroundSpec.Of(bgColor),
                Radius = RadiusSpec.All(RadiusScale.Lg),
            }
        );

        float fixedHeight = SpacingScale.Md.ToPixels() * 2f
            + (header.Measure?.Invoke(0f) ?? header.PreferredHeight ?? new Rem(1.95f).ToPixels())
            + SpacingScale.Sm.ToPixels()
            + (searchField.PreferredHeight ?? new Rem(1.75f).ToPixels())
            + SpacingScale.Sm.ToPixels()
            + (divider.PreferredHeight ?? new Rem(1f / 16f).ToPixels());

        float rowH = RowHeight.ToPixels();
        float listNatural = filtered.Count == 0
            ? new Rem(3f).ToPixels()
            : filtered.Count * rowH;

        float total = fixedHeight + listNatural;
        float capped = Mathf.Min(total, maxHeightPx);

        box.PreferredHeight = capped;
        box.Measure = _ => capped;

        return box;
    }

    private static List<LoadedLanguage> SortSelectedFirst(List<LoadedLanguage> langs, LoadedLanguage? active) {
        if (active == null) {
            return langs;
        }
        List<LoadedLanguage> sorted = new List<LoadedLanguage>(langs.Count);
        for (int i = 0; i < langs.Count; i++) {
            if (ReferenceEquals(langs[i], active)) {
                sorted.Add(langs[i]);
                break;
            }
        }
        for (int i = 0; i < langs.Count; i++) {
            if (!ReferenceEquals(langs[i], active)) {
                sorted.Add(langs[i]);
            }
        }
        return sorted;
    }

    private static LightweaveNode BuildList(List<LoadedLanguage> langs, LoadedLanguage? active, Action onDismiss) {
        return Stack.Create(SpacingScale.None, s => {
            for (int i = 0; i < langs.Count; i++) {
                LoadedLanguage lang = langs[i];
                bool isSelected = active != null && ReferenceEquals(lang, active);
                s.Add(BuildLangRow(lang, isSelected, () => {
                    LanguageDatabase.SelectLanguage(lang);
                    onDismiss?.Invoke();
                }));
            }
        });
    }

    private static LightweaveNode BuildLangRow(LoadedLanguage lang, bool isSelected, Action onClick) {
        string nativeName = lang.FriendlyNameNative ?? lang.folderName;
        string englishName = lang.FriendlyNameEnglish ?? string.Empty;
        string? iso = LookupIso(lang.LegacyFolderName) ?? LookupIso(lang.folderName);

        LightweaveNode node = NodeBuilder.New("LangRow:" + lang.folderName);
        node.PreferredHeight = RowHeight.ToPixels();
        node.Paint = (rect, _) => {
            Theme.Theme theme = RenderContext.Current.Theme;
            Direction dir = RenderContext.Current.Direction;
            bool rtl = dir == Direction.Rtl;

            InteractionState state = InteractionState.Resolve(rect, null, false);
            bool hovered = state.Hovered;

            if (isSelected) {
                Color accent = theme.GetColor(ThemeSlot.SurfaceAccent);
                accent.a = 0.12f;
                PaintBox.Draw(rect, BackgroundSpec.Of(accent), null, null);
            }
            else if (hovered) {
                Color hover = theme.GetColor(ThemeSlot.SurfaceRaised);
                hover.a = 0.45f;
                PaintBox.Draw(rect, BackgroundSpec.Of(hover), null, null);
            }

            float padX = new Rem(1f).ToPixels();
            float isoW = new Rem(2.5f).ToPixels();
            float checkW = new Rem(1.5f).ToPixels();
            float gap = new Rem(0.5f).ToPixels();

            Rect isoRect;
            Rect checkRect;
            Rect labelRect;
            if (rtl) {
                isoRect = new Rect(rect.xMax - padX - isoW, rect.y, isoW, rect.height);
                checkRect = new Rect(rect.x + padX, rect.y, checkW, rect.height);
                float labelLeft = checkRect.xMax + gap;
                float labelRight = isoRect.x - gap;
                labelRect = new Rect(labelLeft, rect.y, Mathf.Max(0f, labelRight - labelLeft), rect.height);
            }
            else {
                isoRect = new Rect(rect.x + padX, rect.y, isoW, rect.height);
                checkRect = new Rect(rect.xMax - padX - checkW, rect.y, checkW, rect.height);
                float labelLeft = isoRect.xMax + gap;
                float labelRight = checkRect.x - gap;
                labelRect = new Rect(labelLeft, rect.y, Mathf.Max(0f, labelRight - labelLeft), rect.height);
            }

            Color savedGui = GUI.color;

            if (!string.IsNullOrEmpty(iso)) {
                Font isoFont = theme.GetFont(FontRole.Mono);
                int isoPx = Mathf.RoundToInt(new Rem(0.7f).ToFontPx());
                GUIStyle isoStyle = GuiStyleCache.GetOrCreate(isoFont, isoPx, FontStyle.Bold);
                isoStyle.alignment = TextAnchor.MiddleCenter;
                GUI.color = theme.GetColor(isSelected ? ThemeSlot.SurfaceAccent : ThemeSlot.TextMuted);
                GUI.Label(RectSnap.Snap(isoRect), iso, isoStyle);
            }

            Font topFont = theme.GetFont(FontRole.BodyBold);
            int topPx = Mathf.RoundToInt(new Rem(0.95f).ToFontPx());
            GUIStyle topStyle = GuiStyleCache.GetOrCreate(topFont, topPx, FontStyle.Bold);
            topStyle.alignment = rtl ? TextAnchor.UpperRight : TextAnchor.UpperLeft;
            topStyle.clipping = TextClipping.Clip;

            bool showEnglish = !string.IsNullOrEmpty(englishName)
                && !string.Equals(englishName, nativeName, StringComparison.OrdinalIgnoreCase);

            int botPx = Mathf.RoundToInt(new Rem(0.72f).ToFontPx());
            float topLineH = topPx + 6f;
            float botLineH = botPx + 4f;
            float lineGap = new Rem(0.35f).ToPixels();
            float blockH = showEnglish ? (topLineH + lineGap + botLineH) : topLineH;
            float blockY = labelRect.y + (labelRect.height - blockH) * 0.5f;

            GUI.color = theme.GetColor(isSelected ? ThemeSlot.SurfaceAccent : ThemeSlot.TextPrimary);
            Rect topRect = new Rect(labelRect.x, blockY, labelRect.width, topLineH);
            GUI.Label(RectSnap.Snap(topRect), nativeName, topStyle);

            if (showEnglish) {
                Font botFont = theme.GetFont(FontRole.Body);
                GUIStyle botStyle = GuiStyleCache.GetOrCreate(botFont, botPx, FontStyle.Normal);
                botStyle.alignment = rtl ? TextAnchor.UpperRight : TextAnchor.UpperLeft;
                botStyle.clipping = TextClipping.Clip;
                GUI.color = theme.GetColor(ThemeSlot.TextMuted);
                Rect botRect = new Rect(labelRect.x, blockY + topLineH + lineGap, labelRect.width, botLineH);
                GUI.Label(RectSnap.Snap(botRect), englishName, botStyle);
            }

            if (isSelected) {
                Font checkFont = theme.GetFont(FontRole.Body);
                int checkPx = Mathf.RoundToInt(new Rem(1.05f).ToFontPx());
                GUIStyle checkStyle = GuiStyleCache.GetOrCreate(checkFont, checkPx);
                checkStyle.alignment = TextAnchor.MiddleCenter;
                GUI.color = theme.GetColor(ThemeSlot.SurfaceAccent);
                GUI.Label(RectSnap.Snap(checkRect), "✓", checkStyle);
            }

            GUI.color = savedGui;

            if (!isSelected) {
                MouseoverSounds.DoRegion(rect);
            }
            Event e = Event.current;
            if (e.type == EventType.MouseUp && e.button == 0 && rect.Contains(e.mousePosition)) {
                onClick?.Invoke();
                e.Use();
            }
        };
        return node;
    }

    private static LightweaveNode BuildHeader(int total) {
        Style eyebrowStyle = new Style {
            FontFamily = FontRole.Mono,
            FontSize = new Rem(0.8125f),
            LetterSpacing = Tracking.Of(0.18f),
            TextColor = ThemeSlot.TextMuted,
        };
        Style countStyle = new Style {
            FontFamily = FontRole.Mono,
            FontSize = new Rem(0.8125f),
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

    private static LightweaveNode WrapHorizontalPad(LightweaveNode child) {
        return Box.Create(
            children: c => c.Add(child),
            style: new Style {
                Padding = new EdgeInsets(Top: SpacingScale.None, Bottom: SpacingScale.None, Left: SpacingScale.Md, Right: SpacingScale.Md),
            }
        );
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

    private static readonly Dictionary<string, string> IsoCodes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
        { "Catalan", "CA" },
        { "ChineseSimplified", "ZH" },
        { "ChineseTraditional", "ZH" },
        { "Czech", "CS" },
        { "Danish", "DA" },
        { "Dutch", "NL" },
        { "English", "EN" },
        { "Estonian", "ET" },
        { "Finnish", "FI" },
        { "French", "FR" },
        { "German", "DE" },
        { "Greek", "EL" },
        { "Hungarian", "HU" },
        { "Italian", "IT" },
        { "Japanese", "JA" },
        { "Korean", "KO" },
        { "Norwegian", "NO" },
        { "Polish", "PL" },
        { "Portuguese", "PT" },
        { "PortugueseBrazilian", "PT" },
        { "Romanian", "RO" },
        { "Russian", "RU" },
        { "Slovak", "SK" },
        { "Spanish", "ES" },
        { "SpanishLatin", "ES" },
        { "Swedish", "SV" },
        { "Turkish", "TR" },
        { "Ukrainian", "UK" },
        { "Vietnamese", "VI" },
        { "Bulgarian", "BG" },
        { "Belarusian", "BE" },
        { "Lithuanian", "LT" },
        { "Latvian", "LV" },
        { "Croatian", "HR" },
        { "Serbian", "SR" },
        { "Slovenian", "SL" },
        { "Hebrew", "HE" },
        { "Arabic", "AR" },
        { "Thai", "TH" },
        { "Indonesian", "ID" },
    };

    private static string? LookupIso(string? folderName) {
        if (string.IsNullOrEmpty(folderName)) {
            return null;
        }
        return IsoCodes.TryGetValue(folderName!, out string iso) ? iso : null;
    }
}
