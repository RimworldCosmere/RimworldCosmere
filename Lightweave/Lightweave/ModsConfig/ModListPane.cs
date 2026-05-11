using System;
using System.Collections.Generic;
using Cosmere.Lightweave.Input;
using Cosmere.Lightweave.Layout;
using Cosmere.Lightweave.Rendering;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using Cosmere.Lightweave.Typography;
using RimWorld;
using UnityEngine;
using Verse;
using Eyebrow = Cosmere.Lightweave.Typography.Eyebrow;
using Text = Cosmere.Lightweave.Typography.Typography.Text;

namespace Cosmere.Lightweave.ModsConfig;

public static class ModListPane {
    private static readonly Rem RowHeight = new Rem(3.25f);
    private static readonly Rem StripeWidth = new Rem(0.1875f);

    public static LightweaveNode Create(
        List<ModMetaData> mods,
        string? selected,
        Action<string> onSelect,
        RimWorld.Page_ModsConfig page
    ) {
        return Box.Create(
            children: c => c.Add(ScrollArea.Create(
                content: BuildList(mods, selected, onSelect)
            )),
            style: new Style {
                Padding = EdgeInsets.Zero,
                Background = BackgroundSpec.Of(ThemeSlot.SurfaceSunken),
            }
        );
    }

    private static LightweaveNode BuildList(
        List<ModMetaData> mods,
        string? selected,
        Action<string> onSelect
    ) {
        return Stack.Create(SpacingScale.None, s => {
            if (mods == null || mods.Count == 0) {
                s.Add(BuildEmptyState());
                return;
            }
            bool sectionedActive = false;
            bool sectionedInactive = false;
            for (int i = 0; i < mods.Count; i++) {
                ModMetaData mod = mods[i];
                if (mod.Active && !sectionedActive) {
                    s.Add(BuildSectionHeader("CL_ModsConfig_Section_Active".Translate()));
                    sectionedActive = true;
                }
                if (!mod.Active && !sectionedInactive) {
                    s.Add(BuildSectionHeader("CL_ModsConfig_Section_Inactive".Translate()));
                    sectionedInactive = true;
                }
                bool isSelected = string.Equals(mod.PackageId, selected, StringComparison.OrdinalIgnoreCase);
                s.Add(BuildRow(mod, isSelected, () => onSelect(mod.PackageId)));
            }
        });
    }

    private static LightweaveNode BuildSectionHeader(string label) {
        LightweaveNode node = NodeBuilder.New("ModListSection:" + label);
        node.PreferredHeight = new Rem(1.6f).ToPixels();
        node.Paint = (rect, _) => {
            Theme.Theme theme = RenderContext.Current.Theme;
            PaintBox.Draw(rect, BackgroundSpec.Of(ThemeSlot.SurfacePrimary), null, null);
            Font font = theme.GetFont(FontRole.BodyBold);
            int px = Mathf.RoundToInt(new Rem(0.65f).ToFontPx());
            GUIStyle style = GuiStyleCache.GetOrCreate(font, px, FontStyle.Bold);
            style.alignment = TextAnchor.MiddleLeft;
            Color saved = GUI.color;
            GUI.color = theme.GetColor(ThemeSlot.TextMuted);
            float padX = SpacingScale.Md.ToPixels();
            Rect labelRect = new Rect(rect.x + padX, rect.y, rect.width - padX * 2f, rect.height);
            GUI.Label(RectSnap.Snap(labelRect), label.ToUpperInvariant(), style);
            GUI.color = saved;
        };
        return node;
    }

    private static LightweaveNode BuildRow(ModMetaData mod, bool isSelected, Action onClick) {
        LightweaveNode node = NodeBuilder.New("ModListRow:" + mod.PackageId);
        node.PreferredHeight = RowHeight.ToPixels();
        node.Paint = (rect, _) => {
            Theme.Theme theme = RenderContext.Current.Theme;
            InteractionState state = InteractionState.Resolve(rect, null, false);

            ThemeSlot bgSlot = isSelected
                ? ThemeSlot.SurfaceRaised
                : (state.Hovered ? ThemeSlot.SurfaceRaised : ThemeSlot.SurfaceSunken);
            PaintBox.Draw(rect, BackgroundSpec.Of(bgSlot), null, null);

            if (isSelected) {
                Rect stripe = new Rect(rect.x, rect.y, StripeWidth.ToPixels(), rect.height);
                PaintBox.Draw(stripe, BackgroundSpec.Of(ThemeSlot.SurfaceAccent), null, null);
            }

            float padX = SpacingScale.Md.ToPixels();
            float padY = SpacingScale.Sm.ToPixels();
            Rect content = new Rect(rect.x + padX, rect.y + padY, rect.width - padX * 2f, rect.height - padY * 2f);

            Font bodyFont = theme.GetFont(FontRole.BodyBold);
            int titlePx = Mathf.RoundToInt(new Rem(0.9f).ToFontPx());
            GUIStyle titleStyle = GuiStyleCache.GetOrCreate(bodyFont, titlePx, FontStyle.Bold);
            titleStyle.alignment = TextAnchor.UpperLeft;

            Font tinyFont = theme.GetFont(FontRole.Body);
            int detailPx = Mathf.RoundToInt(new Rem(0.65f).ToFontPx());
            GUIStyle detailStyle = GuiStyleCache.GetOrCreate(tinyFont, detailPx, FontStyle.Normal);
            detailStyle.alignment = TextAnchor.UpperLeft;
            GUIStyle detailRightStyle = GuiStyleCache.GetOrCreate(tinyFont, detailPx, FontStyle.Normal);
            detailRightStyle.alignment = TextAnchor.UpperRight;

            string title = mod.Name ?? mod.PackageId ?? string.Empty;
            string author = ResolveAuthor(mod);
            string source = ResolveSource(mod);
            string version = string.IsNullOrEmpty(mod.ModVersion) ? "—" : "v" + mod.ModVersion;

            float titleH = titlePx + 4f;
            float detailH = detailPx + 4f;

            float authorW = string.IsNullOrEmpty(author)
                ? 0f
                : detailRightStyle.CalcSize(new GUIContent(author)).x;

            Color saved = GUI.color;
            GUI.color = theme.GetColor(ThemeSlot.TextPrimary);
            float titleW = content.width - (authorW > 0f ? authorW + SpacingScale.Sm.ToPixels() : 0f);
            Rect titleRect = new Rect(content.x, content.y, titleW, titleH);
            GUI.Label(RectSnap.Snap(titleRect), title, titleStyle);

            if (authorW > 0f) {
                GUI.color = theme.GetColor(ThemeSlot.TextMuted);
                Rect authorRect = new Rect(content.xMax - authorW, content.y + (titleH - detailH) * 0.5f + 1f, authorW, detailH);
                GUI.Label(RectSnap.Snap(authorRect), author, detailRightStyle);
            }

            GUI.color = theme.GetColor(ThemeSlot.TextMuted);
            Rect detailRect = new Rect(content.x, titleRect.yMax + 2f, content.width, detailH);
            GUI.Label(RectSnap.Snap(detailRect), source + "  ·  " + version, detailStyle);
            GUI.color = saved;

            InteractionFeedback.Apply(rect, true, true);

            Event e = Event.current;
            if (e.type == EventType.MouseUp && e.button == 0 && rect.Contains(e.mousePosition)) {
                onClick?.Invoke();
                e.Use();
            }
        };
        return node;
    }

    private static LightweaveNode BuildEmptyState() {
        return Container.Create(
            child: Stack.Create(SpacingScale.Xs, s => {
                s.Add(Eyebrow.Create("CL_ModsConfig_Empty_Eyebrow".Translate()));
                s.Add(Text.Create(
                    "CL_ModsConfig_Empty_Body".Translate(),
                    wrap: true,
                    style: new Style { TextColor = ThemeSlot.TextSecondary }
                ));
            }),
            style: new Style {
                Padding = EdgeInsets.All(SpacingScale.Lg),
            }
        );
    }

    private static string ResolveDetail(ModMetaData mod) {
        string source = ResolveSource(mod);
        string version = string.IsNullOrEmpty(mod.ModVersion) ? "—" : mod.ModVersion;
        return source + "  ·  v" + version;
    }

    private static string ResolveAuthor(ModMetaData mod) {
        string author = mod.AuthorsString;
        return string.IsNullOrEmpty(author) ? string.Empty : author;
    }

    private static string ResolveSource(ModMetaData mod) {
        if (mod.IsCoreMod) {
            return "CL_ModsConfig_Source_Core".Translate();
        }
        if (mod.Official) {
            return "CL_ModsConfig_Source_Expansion".Translate();
        }
        if (mod.OnSteamWorkshop) {
            return "CL_ModsConfig_Source_Workshop".Translate();
        }
        return "CL_ModsConfig_Source_Local".Translate();
    }
}
