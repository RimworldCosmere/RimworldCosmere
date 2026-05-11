using System;
using System.Runtime.CompilerServices;
using Cosmere.Lightweave.Doc;
using Cosmere.Lightweave.Hooks;
using Cosmere.Lightweave.Input;
using Cosmere.Lightweave.Overlay;
using Cosmere.Lightweave.Rendering;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using UnityEngine;
using Verse;

namespace Cosmere.Lightweave.Navigation;

[Doc(
    Id = "menu",
    Summary = "Anchored popover with header, search, rows, dividers, hotkeys, and submenus.",
    WhenToUse = "Trigger a list of one-click commands from a button or icon. Optional header (mono uppercase), search input, subtitle text, active checkmark, and hotkey badges.",
    SourcePath = "Lightweave/Lightweave/Navigation/Menu.cs",
    ShowRtl = true
)]
public static class Menu {
    private static readonly Rem RowHeight = new Rem(2.25f);
    private static readonly Rem RowHeightWithSubtitle = new Rem(2.75f);
    private static readonly Rem DividerHeight = new Rem(0.6875f);
    private static readonly Rem DefaultWidth = new Rem(15.625f);
    private static readonly Rem RowPadX = new Rem(1f);
    private static readonly Rem RowPadY = new Rem(0.5625f);
    private static readonly Rem RowGap = new Rem(0.75f);
    private static readonly Rem IconSize = new Rem(1f);
    private static readonly Rem HeaderHeight = new Rem(2.0f);
    private static readonly Rem HeaderPadX = new Rem(1f);
    private static readonly Rem SearchHeight = new Rem(2.625f);
    private static readonly Rem SearchPadX = new Rem(1f);
    private static readonly Rem SearchPadY = new Rem(0.5f);
    private static readonly Rem EmptyHeight = new Rem(2.5f);
    private static readonly Rem HotkeyTrailWidth = new Rem(2.5f);

    private static readonly Color PopBackdrop = new Color(15f / 255f, 12f / 255f, 8f / 255f, 0.96f);
    private static readonly Color RowHover = new Color(40f / 255f, 32f / 255f, 22f / 255f, 0.75f);

    public static MenuItem Item(
        string label,
        Action onInvoke,
        LightweaveNode? icon = null,
        bool disabled = false,
        string? subtitle = null,
        bool active = false,
        string? hotkey = null,
        bool danger = false
    ) {
        return new MenuItem(label, onInvoke, icon, disabled, null, false, danger, subtitle, active, hotkey);
    }

    public static MenuItem Submenu(
        string label,
        IReadOnlyList<MenuItem> children,
        LightweaveNode? icon = null,
        bool disabled = false,
        string? subtitle = null
    ) {
        return new MenuItem(label, null, icon, disabled, children, false, false, subtitle);
    }

    public static MenuItem Divider() {
        return new MenuItem(string.Empty, null, null, false, null, true);
    }

    public static LightweaveNode Create(
        [DocParam("Whether the menu is currently visible.")]
        bool isOpen,
        [DocParam("Screen-space rect the menu attaches to.")]
        Rect anchorRect,
        [DocParam("Items rendered top-to-bottom; mix MenuItem.Action, MenuItem.Submenu, and MenuItem.Divider.")]
        IReadOnlyList<MenuItem> items,
        [DocParam("Invoked when the menu requests dismissal (escape, outside-click, item activation).")]
        Action onDismiss,
        [DocParam("Horizontal alignment relative to the anchor.")]
        MenuAnchor anchor = MenuAnchor.Left,
        [DocParam("Whether the menu opens above or below the anchor.")]
        MenuDirection direction = MenuDirection.Down,
        [DocParam("Disambiguator when multiple menus share the same caller line.")]
        object? instanceKey = null,
        [DocParam("Optional header label rendered as a mono uppercase tracked title at the top.")]
        string? header = null,
        [DocParam("Optional secondary header text rendered to the right of the header label.")]
        string? headerMeta = null,
        [DocParam("If set, renders a search input above the rows that filters items by Label and Subtitle.")]
        string? searchPlaceholder = null,
        [DocParam("Explicit popover size in pixels; pass null for default width and content-based height.")]
        Vector2? size = null,
        Style? style = null,
        string[]? classes = null,
        string? id = null,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        string keySuffix = instanceKey == null ? string.Empty : "#" + instanceKey;
        string focusedKey = file + "#focused" + keySuffix;
        string submenuKey = file + "#submenu" + keySuffix;
        string searchKey = file + "#search" + keySuffix;

        if (!isOpen) {
            LightweaveNode empty = NodeBuilder.New("Menu:closed", line, file);
            empty.Paint = (_, _) => { };
            return empty;
        }

        Hooks.Hooks.StateHandle<int> focusedIndex = Hooks.Hooks.UseState(FirstSelectableIndex(items), line, focusedKey);
        Hooks.Hooks.StateHandle<int> openSubmenuIndex = Hooks.Hooks.UseState(-1, line, submenuKey);
        bool searchEnabled = searchPlaceholder != null;
        Hooks.Hooks.StateHandle<string> searchQuery = Hooks.Hooks.UseState(string.Empty, line, searchKey);

        IReadOnlyList<MenuItem> filteredItems = searchEnabled && !string.IsNullOrWhiteSpace(searchQuery.Value)
            ? FilterItems(items, searchQuery.Value)
            : items;

        bool hasHeader = !string.IsNullOrEmpty(header);
        float headerPx = hasHeader ? HeaderHeight.ToPixels() : 0f;
        float searchPx = searchEnabled ? SearchHeight.ToPixels() : 0f;
        float rowsPx = filteredItems.Count == 0 && searchEnabled
            ? EmptyHeight.ToPixels()
            : ComputeRowsHeight(filteredItems);
        float menuHeight = headerPx + searchPx + rowsPx;

        float menuWidth = size?.x ?? DefaultWidth.ToPixels();
        if (menuWidth <= 0f) {
            menuWidth = DefaultWidth.ToPixels();
        }

        LightweaveNode content = NodeBuilder.New("Menu:content", line, file);
        content.PreferredHeight = menuHeight;
        content.Paint = (rect, _) => {
            HandleKeyboard(filteredItems, focusedIndex, openSubmenuIndex, onDismiss);

            PaintBackdrop(rect);

            float cursorY = rect.y;

            if (hasHeader) {
                Rect headerRect = new Rect(rect.x, cursorY, rect.width, headerPx);
                PaintHeader(headerRect, header!, headerMeta);
                cursorY += headerPx;
                PaintHairline(rect.x, cursorY, rect.width);
            }

            if (searchEnabled) {
                Rect searchRect = new Rect(rect.x, cursorY, rect.width, searchPx);
                PaintSearch(searchRect, searchQuery, searchPlaceholder!);
                cursorY += searchPx;
                PaintHairline(rect.x, cursorY, rect.width);
            }

            int count = filteredItems.Count;
            if (count == 0 && searchEnabled) {
                Rect emptyRect = new Rect(rect.x, cursorY, rect.width, EmptyHeight.ToPixels());
                PaintEmpty(emptyRect);
                return;
            }

            for (int i = 0; i < count; i++) {
                MenuItem it = filteredItems[i];
                float rowH = it.IsDivider ? DividerHeight.ToPixels() : RowHeightFor(it);
                Rect rowRect = new Rect(rect.x, cursorY, rect.width, rowH);
                if (it.IsDivider) {
                    PaintDivider(rowRect);
                }
                else {
                    PaintRow(rowRect, it, i, focusedIndex, openSubmenuIndex, onDismiss);
                }

                cursorY += rowH;
            }

            int submenuIdx = openSubmenuIndex.Value;
            if (submenuIdx >= 0 && submenuIdx < count) {
                MenuItem submenuOwner = filteredItems[submenuIdx];
                if (!submenuOwner.IsDivider && submenuOwner.Children != null && submenuOwner.Children.Count > 0) {
                    float submenuY = rect.y + headerPx + searchPx;
                    for (int j = 0; j < submenuIdx; j++) {
                        submenuY += filteredItems[j].IsDivider ? DividerHeight.ToPixels() : RowHeightFor(filteredItems[j]);
                    }

                    float ownerH = RowHeightFor(submenuOwner);
                    Rect submenuAnchor = new Rect(rect.x, submenuY, rect.width, ownerH);
                    LightweaveNode submenu = Create(
                        true,
                        submenuAnchor,
                        submenuOwner.Children,
                        () => openSubmenuIndex.Set(-1),
                        MenuAnchor.Right,
                        direction,
                        instanceKey == null ? submenuIdx : (instanceKey, submenuIdx)
                    );
                    submenu.MeasuredRect = submenuAnchor;
                    LightweaveRoot.PaintSubtree(submenu, submenuAnchor);
                }
            }
        };

        Vector2 popoverSize = size ?? new Vector2(menuWidth, menuHeight);
        if (popoverSize.x <= 0f) popoverSize.x = menuWidth;
        if (popoverSize.y <= 0f) popoverSize.y = menuHeight;
        PopoverPlacement placement = ResolvePlacement(anchor, direction);
        return Popover.Create(
            true,
            anchorRect,
            placement,
            content,
            onDismiss,
            popoverSize,
            style,
            classes,
            id,
            line,
            file
        );
    }

    private static IReadOnlyList<MenuItem> FilterItems(IReadOnlyList<MenuItem> items, string query) {
        string q = query.Trim();
        if (q.Length == 0) {
            return items;
        }

        List<MenuItem> result = new List<MenuItem>(items.Count);
        for (int i = 0; i < items.Count; i++) {
            MenuItem it = items[i];
            if (it.IsDivider) {
                continue;
            }

            if (Match(it.Label, q) || Match(it.Subtitle, q)) {
                result.Add(it);
            }
        }

        return result;
    }

    private static bool Match(string? haystack, string needle) {
        return !string.IsNullOrEmpty(haystack) &&
               haystack!.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private static float RowHeightFor(MenuItem item) {
        return string.IsNullOrEmpty(item.Subtitle) ? RowHeight.ToPixels() : RowHeightWithSubtitle.ToPixels();
    }

    private static float ComputeRowsHeight(IReadOnlyList<MenuItem> items) {
        float total = 0f;
        float dividerH = DividerHeight.ToPixels();
        for (int i = 0; i < items.Count; i++) {
            total += items[i].IsDivider ? dividerH : RowHeightFor(items[i]);
        }

        return total;
    }

    private static PopoverPlacement ResolvePlacement(MenuAnchor anchor, MenuDirection direction) {
        if (anchor == MenuAnchor.Right) {
            return PopoverPlacement.End;
        }

        if (direction == MenuDirection.Up) {
            return PopoverPlacement.Top;
        }

        return PopoverPlacement.Bottom;
    }

    private static int FirstSelectableIndex(IReadOnlyList<MenuItem> items) {
        for (int i = 0; i < items.Count; i++) {
            if (!items[i].IsDivider && !items[i].Disabled) {
                return i;
            }
        }

        return -1;
    }

    private static void PaintBackdrop(Rect rect) {
        if (Event.current.type != EventType.Repaint) {
            return;
        }

        BackgroundSpec bg = BackgroundSpec.Of(PopBackdrop);
        PaintBox.Draw(rect, bg, null, RadiusSpec.All(RadiusScale.Lg));
    }

    private static void PaintHeader(Rect rect, string label, string? meta) {
        if (Event.current.type != EventType.Repaint) {
            return;
        }

        Theme.Theme theme = RenderContext.Current.Theme;
        Direction dir = RenderContext.Current.Direction;
        float padX = HeaderPadX.ToPixels();
        Font font = theme.GetFont(FontRole.Mono);
        int px = Mathf.RoundToInt(new Rem(0.6875f).ToFontPx());
        GUIStyle style = GuiStyleCache.GetOrCreate(font, px, FontStyle.Normal);
        style.alignment = dir == Direction.Rtl ? TextAnchor.MiddleRight : TextAnchor.MiddleLeft;

        Rect labelRect = new Rect(rect.x + padX, rect.y, rect.width - padX * 2f, rect.height);
        Color saved = GUI.color;
        GUI.color = theme.GetColor(ThemeSlot.TextMuted);
        GUI.Label(RectSnap.Snap(labelRect), label.ToUpperInvariant(), style);

        if (!string.IsNullOrEmpty(meta)) {
            GUIStyle metaStyle = GuiStyleCache.GetOrCreate(font, px, FontStyle.Normal);
            metaStyle.alignment = dir == Direction.Rtl ? TextAnchor.MiddleLeft : TextAnchor.MiddleRight;
            GUI.color = theme.GetColor(ThemeSlot.TextMuted);
            GUI.Label(RectSnap.Snap(labelRect), meta!.ToUpperInvariant(), metaStyle);
        }

        GUI.color = saved;
    }

    private static void PaintHairline(float x, float y, float width) {
        if (Event.current.type != EventType.Repaint) {
            return;
        }

        Theme.Theme theme = RenderContext.Current.Theme;
        float thickness = Mathf.Max(1f, new Rem(1f / 16f).ToPixels());
        Rect line = new Rect(x, y - thickness * 0.5f, width, thickness);
        Color saved = GUI.color;
        GUI.color = theme.GetColor(ThemeSlot.BorderSubtle);
        GUI.DrawTexture(RectSnap.Snap(line), Texture2D.whiteTexture);
        GUI.color = saved;
    }

    private static void PaintSearch(Rect rect, Hooks.Hooks.StateHandle<string> query, string placeholder) {
        float padX = SearchPadX.ToPixels();
        float padY = SearchPadY.ToPixels();
        Rect inner = new Rect(rect.x + padX, rect.y + padY, rect.width - padX * 2f, rect.height - padY * 2f);
        LightweaveNode field = SearchField.Create(query.Value, q => query.Set(q), placeholder);
        field.MeasuredRect = inner;
        LightweaveRoot.PaintSubtree(field, inner);
    }

    private static void PaintEmpty(Rect rect) {
        if (Event.current.type != EventType.Repaint) {
            return;
        }

        Theme.Theme theme = RenderContext.Current.Theme;
        Direction dir = RenderContext.Current.Direction;
        float padX = RowPadX.ToPixels();
        Font font = theme.GetFont(FontRole.Body);
        int px = Mathf.RoundToInt(new Rem(0.75f).ToFontPx());
        GUIStyle style = GuiStyleCache.GetOrCreate(font, px, FontStyle.Italic);
        style.alignment = dir == Direction.Rtl ? TextAnchor.MiddleRight : TextAnchor.MiddleLeft;

        Color saved = GUI.color;
        GUI.color = theme.GetColor(ThemeSlot.TextMuted);
        Rect labelRect = new Rect(rect.x + padX, rect.y, rect.width - padX * 2f, rect.height);
        GUI.Label(RectSnap.Snap(labelRect), (string)"CL_Menu_NoMatches".Translate(), style);
        GUI.color = saved;
    }

    private static void PaintDivider(Rect rowRect) {
        if (Event.current.type != EventType.Repaint) {
            return;
        }

        Theme.Theme theme = RenderContext.Current.Theme;
        float thickness = Mathf.Max(1f, new Rem(1f / 16f).ToPixels());
        float midY = rowRect.y + rowRect.height / 2f - thickness / 2f;
        Rect line = new Rect(rowRect.x, midY, rowRect.width, thickness);
        Color saved = GUI.color;
        GUI.color = theme.GetColor(ThemeSlot.BorderSubtle);
        GUI.DrawTexture(RectSnap.Snap(line), Texture2D.whiteTexture);
        GUI.color = saved;
    }

    private static void PaintRow(
        Rect rowRect,
        MenuItem item,
        int index,
        Hooks.Hooks.StateHandle<int> focusedIndex,
        Hooks.Hooks.StateHandle<int> openSubmenuIndex,
        Action onDismiss
    ) {
        Theme.Theme theme = RenderContext.Current.Theme;
        Direction dir = RenderContext.Current.Direction;
        Event e = Event.current;
        bool hovering = !item.Disabled && rowRect.Contains(e.mousePosition);
        bool focused = focusedIndex.Value == index;
        bool hasChildren = item.Children != null && item.Children.Count > 0;
        bool hasSubtitle = !string.IsNullOrEmpty(item.Subtitle);
        bool rtl = dir == Direction.Rtl;

        if (hovering) {
            if (hasChildren) {
                if (openSubmenuIndex.Value != index) {
                    openSubmenuIndex.Set(index);
                }
            }
            else {
                if (openSubmenuIndex.Value != -1) {
                    openSubmenuIndex.Set(-1);
                }
            }

            if (focusedIndex.Value != index) {
                focusedIndex.Set(index);
            }
        }

        if (e.type == EventType.Repaint) {
            if (!item.Disabled && hovering) {
                PaintBox.Draw(rowRect, BackgroundSpec.Of(RowHover), null, RadiusSpec.All(RadiusScale.None));
            }
            else if (focused && !item.Disabled) {
                Color focusFill = theme.GetColor(ThemeSlot.SurfaceRaised);
                focusFill.a = 0.35f;
                PaintBox.Draw(rowRect, BackgroundSpec.Of(focusFill), null, RadiusSpec.All(RadiusScale.None));
            }
        }

        float padX = RowPadX.ToPixels();
        float padY = RowPadY.ToPixels();
        float gap = RowGap.ToPixels();
        float iconPx = IconSize.ToPixels();

        float labelStartX = rowRect.x + padX;
        float labelEndX = rowRect.xMax - padX;

        if (item.Icon != null) {
            float iconX = rtl
                ? rowRect.xMax - padX - iconPx
                : rowRect.x + padX;
            Rect iconRect = new Rect(iconX, rowRect.y + (rowRect.height - iconPx) / 2f, iconPx, iconPx);
            item.Icon.MeasuredRect = iconRect;
            LightweaveRoot.PaintSubtree(item.Icon, iconRect);
            if (rtl) {
                labelEndX = iconX - gap;
            }
            else {
                labelStartX = iconX + iconPx + gap;
            }
        }

        if (hasChildren) {
            float chevronPx = iconPx;
            float chevronX = rtl
                ? rowRect.x + padX
                : rowRect.xMax - padX - chevronPx;
            Rect chevronRect = new Rect(chevronX, rowRect.y, chevronPx, rowRect.height);
            Font chevronFont = theme.GetFont(FontRole.Body);
            int chevronSize = Mathf.RoundToInt(new Rem(1.25f).ToFontPx());
            GUIStyle chevronStyle = GuiStyleCache.GetOrCreate(chevronFont, chevronSize);
            chevronStyle.alignment = rtl ? TextAnchor.MiddleLeft : TextAnchor.MiddleRight;
            Color chevronSaved = GUI.color;
            GUI.color = theme.GetColor(ThemeSlot.TextMuted);
            string chevronGlyph = rtl ? "‹" : "›";
            GUI.Label(RectSnap.Snap(chevronRect), chevronGlyph, chevronStyle);
            GUI.color = chevronSaved;
            if (rtl) {
                labelStartX = chevronX + chevronPx + gap;
            }
            else {
                labelEndX = chevronX - gap;
            }
        }
        else if (item.Active) {
            float checkPx = iconPx;
            float checkX = rtl
                ? rowRect.x + padX
                : rowRect.xMax - padX - checkPx;
            Rect checkRect = new Rect(checkX, rowRect.y, checkPx, rowRect.height);
            Font checkFont = theme.GetFont(FontRole.Body);
            int checkSize = Mathf.RoundToInt(new Rem(0.875f).ToFontPx());
            GUIStyle checkStyle = GuiStyleCache.GetOrCreate(checkFont, checkSize);
            checkStyle.alignment = rtl ? TextAnchor.MiddleLeft : TextAnchor.MiddleRight;
            Color checkSaved = GUI.color;
            GUI.color = theme.GetColor(ThemeSlot.AccentMuted);
            GUI.Label(RectSnap.Snap(checkRect), "✓", checkStyle);
            GUI.color = checkSaved;
            if (rtl) {
                labelStartX = checkX + checkPx + gap;
            }
            else {
                labelEndX = checkX - gap;
            }
        }
        else if (!string.IsNullOrEmpty(item.Hotkey)) {
            float hotPx = HotkeyTrailWidth.ToPixels();
            float hotX = rtl
                ? rowRect.x + padX
                : rowRect.xMax - padX - hotPx;
            Rect hotRect = new Rect(hotX, rowRect.y, hotPx, rowRect.height);
            Font hotFont = theme.GetFont(FontRole.Mono);
            int hotSize = Mathf.RoundToInt(new Rem(0.6875f).ToFontPx());
            GUIStyle hotStyle = GuiStyleCache.GetOrCreate(hotFont, hotSize);
            hotStyle.alignment = rtl ? TextAnchor.MiddleLeft : TextAnchor.MiddleRight;
            Color hotSaved = GUI.color;
            GUI.color = theme.GetColor(ThemeSlot.TextMuted);
            GUI.Label(RectSnap.Snap(hotRect), "[" + item.Hotkey!.ToUpperInvariant() + "]", hotStyle);
            GUI.color = hotSaved;
            if (rtl) {
                labelStartX = hotX + hotPx + gap;
            }
            else {
                labelEndX = hotX - gap;
            }
        }

        Rect labelArea = new Rect(labelStartX, rowRect.y, Mathf.Max(0f, labelEndX - labelStartX), rowRect.height);
        Font labelFont = theme.GetFont(FontRole.Body);
        int labelSize = Mathf.RoundToInt(new Rem(0.8125f).ToFontPx());
        GUIStyle labelStyle = GuiStyleCache.GetOrCreate(labelFont, labelSize);

        ThemeSlot labelSlot;
        if (item.Disabled) {
            labelSlot = ThemeSlot.TextMuted;
        }
        else if (item.Danger) {
            labelSlot = ThemeSlot.StatusDanger;
        }
        else if (item.Active) {
            labelSlot = ThemeSlot.AccentMuted;
        }
        else {
            labelSlot = ThemeSlot.TextPrimary;
        }

        Color savedColor = GUI.color;
        GUI.color = theme.GetColor(labelSlot);

        if (hasSubtitle) {
            int subSize = Mathf.RoundToInt(new Rem(0.65625f).ToFontPx());
            GUIStyle subStyle = GuiStyleCache.GetOrCreate(labelFont, subSize);
            float labelTextH = new Rem(1.05f).ToPixels();
            float subTextH = new Rem(0.9f).ToPixels();
            float innerGap = new Rem(0.15f).ToPixels();
            float totalH = labelTextH + innerGap + subTextH;
            float topPad = Mathf.Max(0f, (labelArea.height - totalH) * 0.5f);

            Rect labelLine = new Rect(
                labelArea.x,
                labelArea.y + topPad,
                labelArea.width,
                labelTextH
            );
            labelStyle.alignment = rtl ? TextAnchor.UpperRight : TextAnchor.UpperLeft;
            GUI.Label(RectSnap.Snap(labelLine), item.Label, labelStyle);

            Rect subLine = new Rect(
                labelArea.x,
                labelArea.y + topPad + labelTextH + innerGap,
                labelArea.width,
                subTextH
            );
            subStyle.alignment = rtl ? TextAnchor.UpperRight : TextAnchor.UpperLeft;
            GUI.color = theme.GetColor(ThemeSlot.TextMuted);
            GUI.Label(RectSnap.Snap(subLine), item.Subtitle!, subStyle);
        }
        else {
            labelStyle.alignment = Typography.Typography.ResolveAnchor(TextAlign.Start, dir);
            GUI.Label(RectSnap.Snap(labelArea), item.Label, labelStyle);
        }

        GUI.color = savedColor;

        if (!item.Disabled && e.type == EventType.MouseUp && e.button == 0 && rowRect.Contains(e.mousePosition)) {
            if (hasChildren) {
                openSubmenuIndex.Set(index);
            }
            else {
                item.OnInvoke?.Invoke();
                onDismiss?.Invoke();
            }

            e.Use();
        }
    }

    private static void HandleKeyboard(
        IReadOnlyList<MenuItem> items,
        Hooks.Hooks.StateHandle<int> focusedIndex,
        Hooks.Hooks.StateHandle<int> openSubmenuIndex,
        Action onDismiss
    ) {
        Event e = Event.current;
        if (e.type != EventType.KeyDown) {
            return;
        }

        Direction dir = RenderContext.Current.Direction;
        bool rtl = dir == Direction.Rtl;
        int count = items.Count;
        int current = focusedIndex.Value;
        if (current < 0 || current >= count) {
            current = FirstSelectableIndex(items);
        }

        switch (e.keyCode) {
            case KeyCode.UpArrow:
                focusedIndex.Set(PrevSelectable(items, current));
                e.Use();
                break;
            case KeyCode.DownArrow:
                focusedIndex.Set(NextSelectable(items, current));
                e.Use();
                break;
            case KeyCode.Return:
            case KeyCode.KeypadEnter:
                if (current >= 0 && current < count && !items[current].IsDivider && !items[current].Disabled) {
                    MenuItem chosen = items[current];
                    if (chosen.Children != null && chosen.Children.Count > 0) {
                        openSubmenuIndex.Set(current);
                    }
                    else {
                        chosen.OnInvoke?.Invoke();
                        onDismiss?.Invoke();
                    }
                }

                e.Use();
                break;
            case KeyCode.Escape:
                onDismiss?.Invoke();
                e.Use();
                break;
            case KeyCode.RightArrow:
                if (!rtl &&
                    current >= 0 &&
                    current < count &&
                    !items[current].IsDivider &&
                    items[current].Children != null &&
                    items[current].Children!.Count > 0) {
                    openSubmenuIndex.Set(current);
                    e.Use();
                }

                break;
            case KeyCode.LeftArrow:
                if (rtl &&
                    current >= 0 &&
                    current < count &&
                    !items[current].IsDivider &&
                    items[current].Children != null &&
                    items[current].Children!.Count > 0) {
                    openSubmenuIndex.Set(current);
                    e.Use();
                }

                break;
        }
    }

    private static int NextSelectable(IReadOnlyList<MenuItem> items, int from) {
        int count = items.Count;
        if (count == 0) return -1;
        int i = from;
        for (int step = 0; step < count; step++) {
            i = Math.Min(count - 1, i + 1);
            if (!items[i].IsDivider && !items[i].Disabled) {
                return i;
            }

            if (i == count - 1) break;
        }

        return from;
    }

    private static int PrevSelectable(IReadOnlyList<MenuItem> items, int from) {
        int i = from;
        for (int step = 0; step < items.Count; step++) {
            i = Math.Max(0, i - 1);
            if (!items[i].IsDivider && !items[i].Disabled) {
                return i;
            }

            if (i == 0) break;
        }

        return from;
    }

    [DocVariant("CL_Playground_Label_Default")]
    public static DocSample DocsDefault() {
        return new DocSample(() => {
            Hooks.Hooks.StateHandle<bool> open = Hooks.Hooks.UseState(false);
            Hooks.Hooks.RefHandle<Rect> anchor = Hooks.Hooks.UseRef(default(Rect));

            List<MenuItem> exportChildren = new List<MenuItem> {
                Menu.Item((string)"CL_Playground_Navigation_Menu_ExportPng".Translate(), () => open.Set(false)),
                Menu.Item((string)"CL_Playground_Navigation_Menu_ExportSvg".Translate(), () => open.Set(false)),
            };

            List<MenuItem> items = new List<MenuItem> {
                Menu.Item((string)"CL_Playground_Navigation_Menu_Open".Translate(), () => open.Set(false)),
                Menu.Item((string)"CL_Playground_Navigation_Menu_Save".Translate(), () => open.Set(false)),
                Menu.Item((string)"CL_Playground_Navigation_Menu_SaveAs".Translate(), () => open.Set(false)),
                Menu.Divider(),
                Menu.Submenu((string)"CL_Playground_Navigation_Menu_Export".Translate(), exportChildren),
                Menu.Divider(),
                Menu.Item((string)"CL_Playground_Navigation_Menu_Close".Translate(), () => open.Set(false)),
            };

            LightweaveNode trigger = NodeBuilder.New("MenuTrigger", 0, nameof(Menu));
            LightweaveNode button = Button.Create(
                (string)"CL_Playground_Menu_TriggerOpen".Translate(),
                () => open.Set(!open.Value),
                ButtonVariant.Secondary
            );
            trigger.Children.Add(button);
            trigger.Measure = w => button.Measure?.Invoke(w) ?? button.PreferredHeight ?? 0f;
            trigger.Paint = (rect, _) => {
                anchor.Current = rect;
                button.MeasuredRect = rect;
                LightweaveRoot.PaintSubtree(button, rect);
            };

            LightweaveNode menu = Menu.Create(
                open.Value,
                anchor.Current,
                items,
                () => open.Set(false),
                MenuAnchor.Left,
                MenuDirection.Down,
                "playground-menu"
            );

            LightweaveNode composed = NodeBuilder.New("MenuHost", 0, nameof(Menu));
            composed.Children.Add(trigger);
            composed.Children.Add(menu);
            composed.Measure = w => trigger.Measure?.Invoke(w) ?? trigger.PreferredHeight ?? 0f;
            composed.Paint = (rect, _) => {
                trigger.MeasuredRect = rect;
                LightweaveRoot.PaintSubtree(trigger, rect);
                menu.MeasuredRect = rect;
                LightweaveRoot.PaintSubtree(menu, rect);
            };

            return composed;
        });
    }

    [DocVariant("CL_Playground_Label_WithHeaderAndSearch")]
    public static DocSample DocsWithHeaderAndSearch() {
        return new DocSample(() => {
            Hooks.Hooks.StateHandle<bool> open = Hooks.Hooks.UseState(false);
            Hooks.Hooks.RefHandle<Rect> anchor = Hooks.Hooks.UseRef(default(Rect));

            List<MenuItem> items = new List<MenuItem> {
                Menu.Item("English", () => open.Set(false), active: true, subtitle: "Default"),
                Menu.Item("Castellano", () => open.Set(false)),
                Menu.Item("Deutsch", () => open.Set(false)),
                Menu.Item("Espanol (Latinoamerica)", () => open.Set(false)),
                Menu.Item("Francais", () => open.Set(false)),
                Menu.Item("Italiano", () => open.Set(false)),
                Menu.Item("Polski", () => open.Set(false)),
                Menu.Item("Portugues do Brasil", () => open.Set(false)),
                Menu.Item("Russian", () => open.Set(false)),
            };

            LightweaveNode trigger = NodeBuilder.New("MenuTrigger", 0, nameof(Menu));
            LightweaveNode button = Button.Create(
                "Language",
                () => open.Set(!open.Value),
                ButtonVariant.Secondary
            );
            trigger.Children.Add(button);
            trigger.Measure = w => button.Measure?.Invoke(w) ?? button.PreferredHeight ?? 0f;
            trigger.Paint = (rect, _) => {
                anchor.Current = rect;
                button.MeasuredRect = rect;
                LightweaveRoot.PaintSubtree(button, rect);
            };

            LightweaveNode menu = Menu.Create(
                open.Value,
                anchor.Current,
                items,
                () => open.Set(false),
                MenuAnchor.Left,
                MenuDirection.Down,
                "playground-menu-search",
                "Language",
                null,
                "Search"
            );

            LightweaveNode composed = NodeBuilder.New("MenuHost", 0, nameof(Menu));
            composed.Children.Add(trigger);
            composed.Children.Add(menu);
            composed.Measure = w => trigger.Measure?.Invoke(w) ?? trigger.PreferredHeight ?? 0f;
            composed.Paint = (rect, _) => {
                trigger.MeasuredRect = rect;
                LightweaveRoot.PaintSubtree(trigger, rect);
                menu.MeasuredRect = rect;
                LightweaveRoot.PaintSubtree(menu, rect);
            };

            return composed;
        });
    }

    [DocUsage]
    public static DocSample DocsUsage() {
        return new DocSample(() => {
            Hooks.Hooks.StateHandle<bool> open = Hooks.Hooks.UseState(false);
            Hooks.Hooks.RefHandle<Rect> anchor = Hooks.Hooks.UseRef(default(Rect));

            List<MenuItem> items = new List<MenuItem> {
                Menu.Item("Open", () => open.Set(false)),
                Menu.Item("Save", () => open.Set(false)),
                Menu.Divider(),
                Menu.Item("Close", () => open.Set(false)),
            };

            return Menu.Create(
                open.Value,
                anchor.Current,
                items,
                () => open.Set(false)
            );
        });
    }
}