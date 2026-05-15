using System.Runtime.CompilerServices;
using Cosmere.Lightweave.Input;
using Cosmere.Lightweave.Overlay;
using Cosmere.Lightweave.Rendering;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using Cosmere.Lightweave.Typography;
using UnityEngine;
using Verse;
using static Cosmere.Lightweave.Hooks.Hooks;

namespace Cosmere.Lightweave.MainMenu;

public static class LangSelectField {


    public static LightweaveNode Create(
        bool disabled = false,
        Style? style = null,
        string[]? classes = null,
        string? id = null,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        string openKey = file + "#langfield_open";
        string anchorKey = file + "#langfield_anchor";
        string queryKey = file + "#langfield_query";

        LightweaveNode node = NodeBuilder.New("LangSelectField", line, file);
        node.ApplyStyling("lang-select-field", style, classes, id);
        node.PreferredHeight = SelectorTrigger.Height.ToPixels();

        node.Paint = (allocatedRect, paintChildren) => {
            Rect rect = SelectorTrigger.ComputeTriggerRect(allocatedRect);
            Theme.Theme theme = RenderContext.Current.Theme;
            Direction dir = RenderContext.Current.Direction;
            bool rtl = dir == Direction.Rtl;

            StateHandle<bool> open = UseState(false, line, openKey);
            StateHandle<Rect> anchor = UseState(Rect.zero, line, anchorKey);
            StateHandle<string> query = UseState(string.Empty, line, queryKey);

            anchor.Set(rect);

            if (open.Value && SingletonOverlayRegistry.ShouldClose(openKey)) {
                open.Set(false);
                query.Set(string.Empty);
            }

            InteractionState state = InteractionState.Resolve(rect, null, disabled);
            bool active = state.Hovered || state.Pressed || open.Value;

            BackdropBlur.Draw(rect, active ? 8f : 6f);
            Color translucent = new Color(20f / 255f, 16f / 255f, 11f / 255f, active ? 0.88f : 0.78f);
            ThemeSlot? borderSlot = ButtonVariants.Border(ButtonVariant.Frosted, state);
            BorderSpec? borderSpec = borderSlot.HasValue
                ? BorderSpec.All(new Rem(1f / 16f), borderSlot.Value)
                : null;
            RadiusSpec radiusSpec = RadiusSpec.All(RadiusScale.Sm);
            PaintBox.Draw(rect, BackgroundSpec.Of(translucent), borderSpec, radiusSpec);

            float overlay = ButtonVariants.OverlayAlpha(state);
            if (overlay > 0f) {
                Color overlayColor = InteractionFeedback.OverlayColor(theme, state, overlay);
                PaintBox.Draw(rect, BackgroundSpec.Of(overlayColor), null, radiusSpec);
            }

            SelectorTrigger.Layout layout = SelectorTrigger.ComputeLayout(rect, dir);
            Rect labelRect = layout.LabelRect;
            Rect chevronRect = layout.ChevronRect;

            LoadedLanguage activeLang = LanguageDatabase.activeLanguage;
            string labelText = activeLang != null
                ? (activeLang.FriendlyNameNative ?? activeLang.folderName)
                : "English";

            ThemeSlot fgSlot = ButtonVariants.Foreground(ButtonVariant.Frosted, state);
            Font labelFont = theme.GetFont(FontRole.BodyBold);
            int labelPixelSize = Mathf.RoundToInt(new Rem(0.875f).ToFontPx());
            GUIStyle labelStyle = GuiStyleCache.GetOrCreate(labelFont, labelPixelSize, FontStyle.Normal);
            labelStyle.alignment = Typography.Typography.ResolveAnchor(TextAlign.Start, dir);

            Color savedColor = GUI.color;
            GUI.color = theme.GetColor(fgSlot);
            GUI.Label(RectSnap.Snap(labelRect), labelText, labelStyle);

            SelectorTrigger.DrawChevron(chevronRect, fgSlot, theme);
            GUI.color = savedColor;

            if (!disabled) {
                Event e = Event.current;
                if (e.type == EventType.MouseDown && e.button == 0 && rect.Contains(e.mousePosition)) {
                    e.Use();
                }
                else if (e.type == EventType.MouseUp && e.button == 0 && rect.Contains(e.mousePosition)) {
                    bool willOpen = !open.Value;
                    open.Set(willOpen);
                    if (willOpen) {
                        SingletonOverlayRegistry.Open(openKey);
                    }
                    else {
                        SingletonOverlayRegistry.Close(openKey);
                        query.Set(string.Empty);
                    }
                    e.Use();
                }
            }

            LightweaveNode popover = Popover.Create(
                isOpen: open.Value,
                anchorRect: anchor.Value,
                placement: PopoverPlacement.Bottom,
                content: LangPopover.Create(
                    query.Value,
                    q => query.Set(q),
                    () => {
                        open.Set(false);
                        SingletonOverlayRegistry.Close(openKey);
                        query.Set(string.Empty);
                    }
                ),
                onDismiss: () => {
                    open.Set(false);
                    SingletonOverlayRegistry.Close(openKey);
                    query.Set(string.Empty);
                },
                preferredSize: new Vector2(new Rem(21f).ToPixels(), -1f),
                line: line,
                file: file
            );
            popover.MeasuredRect = rect;
            LightweaveRoot.PaintSubtree(popover, rect);

            paintChildren();
        };

        return node;
    }
}
