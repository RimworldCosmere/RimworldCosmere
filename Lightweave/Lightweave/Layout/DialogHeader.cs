using System;
using System.Runtime.CompilerServices;
using Cosmere.Lightweave.Doc;
using Cosmere.Lightweave.Input;
using Cosmere.Lightweave.Rendering;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Theme;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Cosmere.Lightweave.Layout;

[Doc(
    Id = "dialog-header",
    Summary = "Dialog title bar with optional eyebrow breadcrumb, optional trailing text action, and optional close button.",
    WhenToUse = "Top of any redesigned dialog (Load Colony, Mods Config, Options). Pairs Display title + Eyebrow breadcrumb + IconButton close in a single primitive so headers stay consistent across the suite.",
    SourcePath = "Lightweave/Lightweave/Layout/DialogHeader.cs"
)]
public static class DialogHeader {
    public static LightweaveNode Create(
        [DocParam("Display title text. Rendered uppercase, tracked.")]
        string title,
        [DocParam("Optional eyebrow text rendered before the title in muted color.")]
        string? breadcrumb = null,
        [DocParam("Optional trailing text action (e.g. APPLY). Hidden when null.")]
        string? trailingActionLabel = null,
        [DocParam("Click handler for the trailing action.")]
        Action? onTrailingAction = null,
        [DocParam("Close button click handler. Close button is hidden when null.")]
        Action? onClose = null,
        [DocParam("Whether to draw the bottom divider line.")]
        bool drawDivider = true,
        [DocParam("Optional tab/filter pills rendered between title and close button. Each tab is (label, isActive, onClick).")]
        IReadOnlyList<DialogHeaderTab>? tabs = null,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        LightweaveNode node = NodeBuilder.New("DialogHeader", line, file);
        node.PreferredHeight = new Rem(4.5f).ToPixels();
        node.Paint = (rect, _) => {
            Theme.Theme theme = RenderContext.Current.Theme;
            Direction dir = RenderContext.Current.Direction;
            bool rtl = dir == Direction.Rtl;

            float padX = SpacingScale.Lg.ToPixels();
            float padY = SpacingScale.Md.ToPixels();
            float closeSize = new Rem(2.25f).ToPixels();
            float closePad = SpacingScale.Md.ToPixels();

            Rect closeRect = new Rect(
                rtl ? rect.x + closePad : rect.xMax - closeSize - closePad,
                rect.y + (rect.height - closeSize) * 0.5f,
                closeSize,
                closeSize
            );

            float trailingW = 0f;
            Rect trailingRect = default;
            if (!string.IsNullOrEmpty(trailingActionLabel)) {
                Font tFont = theme.GetFont(FontRole.Body);
                int tPx = Mathf.RoundToInt(new Rem(0.7f).ToFontPx());
                GUIStyle tStyle = GuiStyleCache.GetOrCreate(tFont, tPx, FontStyle.Bold);
                tStyle.alignment = TextAnchor.MiddleCenter;
                tStyle.clipping = TextClipping.Clip;
                string upper = trailingActionLabel!.ToUpperInvariant();
                GUIContent gc = new GUIContent(upper);
                trailingW = tStyle.CalcSize(gc).x + new Rem(1.5f).ToPixels();
                float trailingH = new Rem(1.6f).ToPixels();
                float reserve = closeSize + closePad * 2f;
                trailingRect = new Rect(
                    rtl ? rect.x + reserve : rect.xMax - reserve - trailingW,
                    rect.y + (rect.height - trailingH) * 0.5f,
                    trailingW,
                    trailingH
                );
                LightweaveHitTracker.Track(trailingRect);
                InteractionState ts = InteractionState.Resolve(trailingRect, null, false);
                Color textColor = ts.Hovered
                    ? theme.GetColor(ThemeSlot.TextPrimary)
                    : theme.GetColor(ThemeSlot.TextMuted);
                Color saved = GUI.color;
                GUI.color = textColor;
                tStyle.fontSize = tPx;
                float trackedAdvance = 1.5f;
                float totalW = 0f;
                for (int i = 0; i < upper.Length; i++) {
                    GUIContent ch = new GUIContent(upper[i].ToString());
                    totalW += tStyle.CalcSize(ch).x;
                    if (i < upper.Length - 1) totalW += trackedAdvance;
                }
                float startX = trailingRect.x + (trailingRect.width - totalW) * 0.5f;
                float cursor = startX;
                for (int i = 0; i < upper.Length; i++) {
                    string ch = upper[i].ToString();
                    GUIContent gcc = new GUIContent(ch);
                    float w = tStyle.CalcSize(gcc).x;
                    GUI.Label(RectSnap.Snap(new Rect(cursor, trailingRect.y, w, trailingRect.height)), ch, tStyle);
                    cursor += w + trackedAdvance;
                }
                GUI.color = saved;

                Event te = Event.current;
                if (te.type == EventType.MouseUp && te.button == 0 && trailingRect.Contains(te.mousePosition)) {
                    onTrailingAction?.Invoke();
                    SoundDefOf.Click.PlayOneShotOnCamera();
                    te.Use();
                }
                MouseoverSounds.DoRegion(trailingRect);
            }

            float tabsTotalW = 0f;
            if (tabs != null && tabs.Count > 0) {
                Font tabFont = theme.GetFont(FontRole.Body);
                int tabPx = Mathf.RoundToInt(new Rem(0.7f).ToFontPx());
                GUIStyle tabStyle = GuiStyleCache.GetOrCreate(tabFont, tabPx, FontStyle.Bold);
                tabStyle.alignment = TextAnchor.MiddleCenter;
                float tabH = new Rem(1.6f).ToPixels();
                float tabPadX = SpacingScale.Sm.ToPixels();
                float tabGap = SpacingScale.Xs.ToPixels();
                float reserve = closeSize + closePad * 2f + (trailingW > 0 ? trailingW + SpacingScale.Md.ToPixels() : 0f);
                float[] widths = new float[tabs.Count];
                for (int i = 0; i < tabs.Count; i++) {
                    GUIContent gc = new GUIContent(tabs[i].Label.ToUpperInvariant());
                    widths[i] = tabStyle.CalcSize(gc).x + tabPadX * 2f;
                    tabsTotalW += widths[i];
                    if (i < tabs.Count - 1) tabsTotalW += tabGap;
                }
                float tabsX = rtl ? rect.x + reserve : rect.xMax - reserve - tabsTotalW;
                float tabsY = rect.y + (rect.height - tabH) * 0.5f;
                float cursor = tabsX;
                for (int i = 0; i < tabs.Count; i++) {
                    DialogHeaderTab tab = tabs[i];
                    Rect tabRect = new Rect(cursor, tabsY, widths[i], tabH);
                    LightweaveHitTracker.Track(tabRect);
                    InteractionState ts = InteractionState.Resolve(tabRect, null, false);
                    bool active = tab.IsActive;
                    ThemeSlot fgSlot = active
                        ? ThemeSlot.SurfaceAccent
                        : (ts.Hovered ? ThemeSlot.TextPrimary : ThemeSlot.TextMuted);
                    Color saved = GUI.color;
                    GUI.color = theme.GetColor(fgSlot);
                    GUI.Label(RectSnap.Snap(tabRect), tab.Label.ToUpperInvariant(), tabStyle);
                    GUI.color = saved;

                    if (active) {
                        Color savedU = GUI.color;
                        GUI.color = theme.GetColor(ThemeSlot.SurfaceAccent);
                        Rect underline = new Rect(tabRect.x + tabPadX * 0.5f, tabRect.yMax - 2f, tabRect.width - tabPadX, 1f);
                        GUI.DrawTexture(RectSnap.Snap(underline), BaseContent.WhiteTex);
                        GUI.color = savedU;
                    }

                    Event tabEvt = Event.current;
                    if (tabEvt.type == EventType.MouseUp && tabEvt.button == 0 && tabRect.Contains(tabEvt.mousePosition)) {
                        tab.OnClick?.Invoke();
                        SoundDefOf.Click.PlayOneShotOnCamera();
                        tabEvt.Use();
                    }
                    MouseoverSounds.DoRegion(tabRect);

                    cursor += widths[i] + tabGap;
                }
            }

            float titleZoneRight = rect.xMax - closeSize - closePad * 2f - (trailingW > 0 ? trailingW + SpacingScale.Md.ToPixels() : 0f) - (tabsTotalW > 0 ? tabsTotalW + SpacingScale.Md.ToPixels() : 0f);
            float titleX = rect.x + padX;
            float titleZoneW = titleZoneRight - titleX;
            float titleY = rect.y + padY;
            float titleH = rect.height - padY * 2f;

            Font ebFont = theme.GetFont(FontRole.Body);
            int ebPx = Mathf.RoundToInt(new Rem(0.7f).ToFontPx());
            GUIStyle ebStyle = GuiStyleCache.GetOrCreate(ebFont, ebPx, FontStyle.Bold);
            ebStyle.alignment = TextAnchor.MiddleLeft;
            ebStyle.clipping = TextClipping.Clip;

            float ebW = 0f;
            string upperBreadcrumb = string.IsNullOrEmpty(breadcrumb) ? string.Empty : breadcrumb!.ToUpperInvariant();
            if (!string.IsNullOrEmpty(upperBreadcrumb)) {
                float tracking = 2f;
                float totalEb = 0f;
                for (int i = 0; i < upperBreadcrumb.Length; i++) {
                    GUIContent ch = new GUIContent(upperBreadcrumb[i].ToString());
                    totalEb += ebStyle.CalcSize(ch).x;
                    if (i < upperBreadcrumb.Length - 1) totalEb += tracking;
                }
                ebW = totalEb;
                Color savedEb = GUI.color;
                GUI.color = theme.GetColor(ThemeSlot.TextMuted);
                float cursorEb = titleX;
                for (int i = 0; i < upperBreadcrumb.Length; i++) {
                    string ch = upperBreadcrumb[i].ToString();
                    GUIContent gcc = new GUIContent(ch);
                    float w = ebStyle.CalcSize(gcc).x;
                    GUI.Label(RectSnap.Snap(new Rect(cursorEb, titleY, w, titleH)), ch, ebStyle);
                    cursorEb += w + tracking;
                }
                GUI.color = savedEb;
            }

            float titleStart = titleX + (ebW > 0 ? ebW + SpacingScale.Lg.ToPixels() : 0f);
            Font dispFont = theme.GetFont(FontRole.Display);
            int dispPx = Mathf.RoundToInt(new Rem(1.6f).ToFontPx());
            GUIStyle dispStyle = GuiStyleCache.GetOrCreate(dispFont, dispPx, FontStyle.Bold);
            dispStyle.alignment = TextAnchor.MiddleLeft;
            dispStyle.clipping = TextClipping.Clip;
            string upperTitle = (title ?? string.Empty).ToUpperInvariant();
            float titleSpacing = 4f;
            Color savedT = GUI.color;
            GUI.color = theme.GetColor(ThemeSlot.TextPrimary);
            float cursorT = titleStart;
            for (int i = 0; i < upperTitle.Length; i++) {
                string ch = upperTitle[i].ToString();
                GUIContent gcc = new GUIContent(ch);
                float w = dispStyle.CalcSize(gcc).x;
                if (cursorT + w > titleZoneRight) break;
                GUI.Label(RectSnap.Snap(new Rect(cursorT, titleY, w, titleH)), ch, dispStyle);
                cursorT += w + titleSpacing;
            }
            GUI.color = savedT;

            if (drawDivider) {
                float t = 1f;
                Rect lineRect = new Rect(rect.x, rect.yMax - t, rect.width, t);
                Color savedDiv = GUI.color;
                GUI.color = theme.GetColor(ThemeSlot.BorderSubtle);
                GUI.DrawTexture(RectSnap.Snap(lineRect), BaseContent.WhiteTex);
                GUI.color = savedDiv;
            }

            if (onClose != null) {
                LightweaveHitTracker.Track(closeRect);
                InteractionState closeState = InteractionState.Resolve(closeRect, null, false);
                ThemeSlot closeBorderSlot = closeState.Hovered ? ThemeSlot.BorderHover : ThemeSlot.BorderSubtle;
                ThemeSlot closeFgSlot = closeState.Hovered ? ThemeSlot.TextPrimary : ThemeSlot.TextMuted;
                BackgroundSpec? closeBg = closeState.Hovered ? BackgroundSpec.Of(ThemeSlot.SurfaceRaised) : null;
                PaintBox.Draw(closeRect, closeBg, BorderSpec.All(new Rem(1f / 16f), closeBorderSlot), null);

                Font xFont = theme.GetFont(FontRole.Body);
                int xPx = Mathf.RoundToInt(new Rem(1f).ToFontPx());
                GUIStyle xStyle = GuiStyleCache.GetOrCreate(xFont, xPx, FontStyle.Normal);
                xStyle.alignment = TextAnchor.MiddleCenter;
                Color savedX = GUI.color;
                GUI.color = theme.GetColor(closeFgSlot);
                GUI.Label(RectSnap.Snap(closeRect), "✕", xStyle);
                GUI.color = savedX;

                InteractionFeedback.Apply(closeRect, true, true);
                Event ce = Event.current;
                if (ce.type == EventType.MouseUp && ce.button == 0 && closeRect.Contains(ce.mousePosition)) {
                    onClose.Invoke();
                    SoundDefOf.Click.PlayOneShotOnCamera();
                    ce.Use();
                }
                MouseoverSounds.DoRegion(closeRect);
            }
        };
        return node;
    }

    [DocVariant("CL_Playground_Layout_DialogHeader_TitleOnly")]
    public static DocSample DocsTitleOnly() {
        return new DocSample(() => DialogHeader.Create("Settings"));
    }

    [DocVariant("CL_Playground_Layout_DialogHeader_WithBreadcrumb", Order = 1)]
    public static DocSample DocsBreadcrumb() {
        return new DocSample(() => DialogHeader.Create("Audio", breadcrumb: "Options", onClose: () => { }));
    }

    [DocVariant("CL_Playground_Layout_DialogHeader_FullChrome", Order = 2)]
    public static DocSample DocsFullChrome() {
        return new DocSample(() => DialogHeader.Create(
            "Mods",
            breadcrumb: "Configuration",
            trailingActionLabel: "Apply",
            onTrailingAction: () => { },
            onClose: () => { }
        ));
    }

    [DocUsage]
    public static DocSample DocsUsage() {
        return new DocSample(() => DialogHeader.Create("Load colony", breadcrumb: "Saves", onClose: () => { }));
    }
}


public readonly struct DialogHeaderTab {
    public string Label { get; }
    public bool IsActive { get; }
    public Action OnClick { get; }

    public DialogHeaderTab(string label, bool isActive, Action onClick) {
        Label = label;
        IsActive = isActive;
        OnClick = onClick;
    }
}
