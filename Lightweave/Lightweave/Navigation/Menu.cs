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
    Summary = "Anchored popover with rows, dividers, and submenus.",
    WhenToUse = "Trigger a list of one-click commands from a button or icon.",
    SourcePath = "Lightweave/Lightweave/Navigation/Menu.cs",
    ShowRtl = true
)]
public static class Menu {
    private static readonly Rem RowHeight = new Rem(1.75f);
    private static readonly Rem DividerHeight = new Rem(0.5f);
    private static readonly Rem DefaultWidth = new Rem(15f);
    private static readonly Rem RowPadding = new Rem(0.5f);
    private static readonly Rem IconSize = new Rem(1f);

    public static MenuItem Item(
        string label,
        Action onInvoke,
        LightweaveNode? icon = null,
        bool disabled = false
    ) {
        return new MenuItem(label, onInvoke, icon, disabled);
    }

    public static MenuItem Submenu(
        string label,
        IReadOnlyList<MenuItem> children,
        LightweaveNode? icon = null,
        bool disabled = false
    ) {
        return new MenuItem(label, null, icon, disabled, children);
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
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        string keySuffix = instanceKey == null ? string.Empty : "#" + instanceKey;
        string focusedKey = file + "#focused" + keySuffix;
        string submenuKey = file + "#submenu" + keySuffix;

        if (!isOpen) {
            LightweaveNode empty = NodeBuilder.New("Menu:closed", line, file);
            empty.Paint = (_, _) => { };
            return empty;
        }

        Hooks.Hooks.StateHandle<int> focusedIndex = Hooks.Hooks.UseState(FirstSelectableIndex(items), line, focusedKey);
        Hooks.Hooks.StateHandle<int> openSubmenuIndex = Hooks.Hooks.UseState(-1, line, submenuKey);

        float menuHeight = ComputeMenuHeight(items);

        LightweaveNode content = NodeBuilder.New("Menu:content", line, file);
        content.PreferredHeight = menuHeight;
        content.Paint = (rect, _) => {
            HandleKeyboard(items, focusedIndex, openSubmenuIndex, onDismiss);

            float cursorY = rect.y;
            int count = items.Count;
            for (int i = 0; i < count; i++) {
                MenuItem it = items[i];
                float rowH = it.IsDivider ? DividerHeight.ToPixels() : RowHeight.ToPixels();
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
                MenuItem submenuOwner = items[submenuIdx];
                if (!submenuOwner.IsDivider && submenuOwner.Children != null && submenuOwner.Children.Count > 0) {
                    float submenuY = rect.y;
                    for (int j = 0; j < submenuIdx; j++) {
                        submenuY += items[j].IsDivider ? DividerHeight.ToPixels() : RowHeight.ToPixels();
                    }

                    float ownerH = RowHeight.ToPixels();
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

        Vector2 size = new Vector2(DefaultWidth.ToPixels(), menuHeight);
        PopoverPlacement placement = ResolvePlacement(anchor, direction);
        return Popover.Create(
            true,
            anchorRect,
            placement,
            content,
            onDismiss,
            size,
            line,
            file
        );
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

    private static float ComputeMenuHeight(IReadOnlyList<MenuItem> items) {
        float total = 0f;
        float rowH = RowHeight.ToPixels();
        float dividerH = DividerHeight.ToPixels();
        for (int i = 0; i < items.Count; i++) {
            total += items[i].IsDivider ? dividerH : rowH;
        }

        return total;
    }

    private static int FirstSelectableIndex(IReadOnlyList<MenuItem> items) {
        for (int i = 0; i < items.Count; i++) {
            if (!items[i].IsDivider && !items[i].Disabled) {
                return i;
            }
        }

        return -1;
    }

    private static void PaintDivider(Rect rowRect) {
        Theme.Theme theme = RenderContext.Current.Theme;
        float inset = RowPadding.ToPixels();
        float thickness = Mathf.Max(1f, new Rem(1f / 16f).ToPixels());
        float midY = rowRect.y + rowRect.height / 2f - thickness / 2f;
        Rect line = new Rect(rowRect.x + inset, midY, rowRect.width - inset * 2f, thickness);
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

        float highlightInset = new Rem(0.25f).ToPixels();
        Rect highlightRect = new Rect(
            rowRect.x + highlightInset,
            rowRect.y + highlightInset,
            Mathf.Max(0f, rowRect.width - highlightInset * 2f),
            Mathf.Max(0f, rowRect.height - highlightInset * 2f)
        );
        RadiusSpec highlightRadius = RadiusSpec.All(new Rem(0.5f));

        if (!item.Disabled && hovering) {
            BackgroundSpec hoverBg = BackgroundSpec.Of(ThemeSlot.SurfaceAccent);
            PaintBox.Draw(highlightRect, hoverBg, null, highlightRadius);
        }

        if (focused && !hovering && !item.Disabled) {
            BackgroundSpec focusFill = BackgroundSpec.Of(ThemeSlot.SurfaceRaised);
            BorderSpec focusBorder = BorderSpec.All(new Rem(1f / 16f), ThemeSlot.BorderFocus);
            PaintBox.Draw(highlightRect, focusFill, focusBorder, highlightRadius);
        }

        float padPx = RowPadding.ToPixels();
        float iconPx = IconSize.ToPixels();
        bool rtl = dir == Direction.Rtl;

        float labelStartX = rowRect.x + padPx;
        float labelEndX = rowRect.xMax - padPx;

        if (item.Icon != null) {
            float iconX = rtl
                ? rowRect.xMax - padPx - iconPx
                : rowRect.x + padPx;
            Rect iconRect = new Rect(iconX, rowRect.y + (rowRect.height - iconPx) / 2f, iconPx, iconPx);
            item.Icon.MeasuredRect = iconRect;
            LightweaveRoot.PaintSubtree(item.Icon, iconRect);
            if (rtl) {
                labelEndX = iconX - padPx;
            }
            else {
                labelStartX = iconX + iconPx + padPx;
            }
        }

        if (hasChildren) {
            float chevronPx = iconPx;
            float chevronX = rtl
                ? rowRect.x + padPx
                : rowRect.xMax - padPx - chevronPx;
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
                labelStartX = chevronX + chevronPx + padPx;
            }
            else {
                labelEndX = chevronX - padPx;
            }
        }

        Rect labelRect = new Rect(labelStartX, rowRect.y, labelEndX - labelStartX, rowRect.height);
        Font labelFont = theme.GetFont(FontRole.Body);
        int labelSize = Mathf.RoundToInt(new Rem(0.875f).ToFontPx());
        GUIStyle labelStyle = GuiStyleCache.GetOrCreate(labelFont, labelSize);
        labelStyle.alignment = Typography.Typography.ResolveAnchor(TextAlign.Start, dir);
        ThemeSlot labelSlot = item.Disabled ? ThemeSlot.TextMuted : ThemeSlot.TextPrimary;
        Color savedColor = GUI.color;
        GUI.color = theme.GetColor(labelSlot);
        GUI.Label(RectSnap.Snap(labelRect), item.Label, labelStyle);
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

    [DocVariant("CC_Playground_Label_Default")]
    public static DocSample DocsDefault() {
        return new DocSample(() => {
            Hooks.Hooks.StateHandle<bool> open = Hooks.Hooks.UseState(false);
            Hooks.Hooks.RefHandle<Rect> anchor = Hooks.Hooks.UseRef(default(Rect));

            List<MenuItem> exportChildren = new List<MenuItem> {
                Menu.Item((string)"CC_Playground_Navigation_Menu_ExportPng".Translate(), () => open.Set(false)),
                Menu.Item((string)"CC_Playground_Navigation_Menu_ExportSvg".Translate(), () => open.Set(false)),
            };

            List<MenuItem> items = new List<MenuItem> {
                Menu.Item((string)"CC_Playground_Navigation_Menu_Open".Translate(), () => open.Set(false)),
                Menu.Item((string)"CC_Playground_Navigation_Menu_Save".Translate(), () => open.Set(false)),
                Menu.Item((string)"CC_Playground_Navigation_Menu_SaveAs".Translate(), () => open.Set(false)),
                Menu.Divider(),
                Menu.Submenu((string)"CC_Playground_Navigation_Menu_Export".Translate(), exportChildren),
                Menu.Divider(),
                Menu.Item((string)"CC_Playground_Navigation_Menu_Close".Translate(), () => open.Set(false)),
            };

            LightweaveNode trigger = NodeBuilder.New("MenuTrigger", 0, nameof(Menu));
            LightweaveNode button = Button.Create(
                (string)"CC_Playground_Menu_TriggerOpen".Translate(),
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