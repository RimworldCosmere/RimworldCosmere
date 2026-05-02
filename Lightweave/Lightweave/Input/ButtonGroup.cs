using System;
using System.Runtime.CompilerServices;
using Cosmere.Lightweave.Doc;
using Cosmere.Lightweave.Rendering;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using UnityEngine;
using Verse;
using Verse.Sound;
using static Cosmere.Lightweave.Hooks.Hooks;

namespace Cosmere.Lightweave.Input;

public sealed record ButtonGroupItem(
    string Label,
    Action OnClick,
    bool Disabled = false
);

[Doc(
    Id = "buttongroup",
    Summary = "Connected row of buttons sharing a single bordered frame.",
    WhenToUse = "Group a small set of related actions or filter choices together.",
    SourcePath = "Lightweave/Lightweave/Input/ButtonGroup.cs"
)]
public static class ButtonGroup {
    public static LightweaveNode Create(
        [DocParam("Items rendered as segments left-to-right.")]
        IReadOnlyList<ButtonGroupItem> items,
        [DocParam("Visual variant shared by every segment.")]
        ButtonVariant variant = ButtonVariant.Secondary,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        LightweaveNode node = NodeBuilder.New($"ButtonGroup:{variant}", line, file);
        node.PreferredHeight = new Rem(1.75f).ToPixels();

        node.Paint = (rect, _) => {
            if (items == null || items.Count == 0) {
                return;
            }

            Theme.Theme theme = RenderContext.Current.Theme;
            Direction dir = RenderContext.Current.Direction;
            bool rtl = dir == Direction.Rtl;

            int count = items.Count;

            Font font = theme.GetFont(FontRole.BodyBold);
            int pixelSize = Mathf.RoundToInt(new Rem(0.875f).ToFontPx());
            GUIStyle style = GuiStyleCache.Get(font, pixelSize, FontStyle.Bold);
            style.alignment = TextAnchor.MiddleCenter;
            style.wordWrap = false;

            float borderPx = new Rem(1f / 16f).ToPixels();
            Rem outerCornerRem = new Rem(0.25f);

            InteractionState groupState = new InteractionState(false, false, false, false);
            ThemeSlot? outerBorderSlot = ButtonVariants.Border(variant, groupState);

            Rect contentRect;
            Rem innerCornerRem;
            if (outerBorderSlot.HasValue) {
                PaintBox.Draw(
                    rect,
                    new BackgroundSpec.Solid(outerBorderSlot.Value),
                    null,
                    RadiusSpec.All(outerCornerRem)
                );
                contentRect = new Rect(
                    rect.x + borderPx,
                    rect.y + borderPx,
                    Mathf.Max(0f, rect.width - borderPx * 2f),
                    Mathf.Max(0f, rect.height - borderPx * 2f)
                );
                innerCornerRem = new Rem(Mathf.Max(0f, outerCornerRem.Value - 1f / 16f));
            } else {
                contentRect = rect;
                innerCornerRem = outerCornerRem;
            }

            float segmentWidth = contentRect.width / count;

            Event e = Event.current;
            Color savedColor = GUI.color;

            for (int i = 0; i < count; i++) {
                int logicalIndex = rtl ? count - 1 - i : i;
                ButtonGroupItem item = items[logicalIndex];

                Rect segRect = new Rect(
                    contentRect.x + i * segmentWidth,
                    contentRect.y,
                    segmentWidth,
                    contentRect.height
                );

                bool isFirst = logicalIndex == 0;
                bool isLast = logicalIndex == count - 1;

                InteractionState state = InteractionState.Resolve(segRect, null, item.Disabled);
                ThemeSlot bgSlot = ButtonVariants.Background(variant, state);

                Rem leadingCorner = isFirst ? innerCornerRem : new Rem(0f);
                Rem trailingCorner = isLast ? innerCornerRem : new Rem(0f);
                RadiusSpec segRadius = rtl
                    ? new RadiusSpec(
                        TopStart: trailingCorner,
                        TopEnd: leadingCorner,
                        BottomStart: trailingCorner,
                        BottomEnd: leadingCorner
                    )
                    : new RadiusSpec(
                        TopStart: leadingCorner,
                        TopEnd: trailingCorner,
                        BottomStart: leadingCorner,
                        BottomEnd: trailingCorner
                    );

                PaintBox.Draw(segRect, new BackgroundSpec.Solid(bgSlot), null, segRadius);

                float overlay = ButtonVariants.OverlayAlpha(state);
                if (overlay > 0f) {
                    Color overlayColor = state.Pressed
                        ? new Color(0f, 0f, 0f, overlay)
                        : new Color(1f, 1f, 1f, overlay);
                    PaintBox.Draw(segRect, new BackgroundSpec.Solid(overlayColor), null, segRadius);
                }

                if (!item.Disabled) {
                    MouseoverSounds.DoRegion(segRect);
                }

                ThemeSlot fgSlot = ButtonVariants.Foreground(variant, state);
                GUI.color = theme.GetColor(fgSlot);
                GUI.Label(RectSnap.Snap(segRect), item.Label, style);
                GUI.color = savedColor;

                if (i < count - 1) {
                    float sepX = segRect.xMax - borderPx * 0.5f;
                    Rect sepRect = new Rect(sepX, segRect.y, borderPx, segRect.height);
                    ThemeSlot sepSlot = outerBorderSlot ?? ThemeSlot.BorderDefault;
                    PaintBox.Draw(sepRect, new BackgroundSpec.Solid(sepSlot), null, null);
                }

                if (!item.Disabled &&
                    e.type == EventType.MouseUp &&
                    e.button == 0 &&
                    segRect.Contains(e.mousePosition)) {
                    item.OnClick?.Invoke();
                    e.Use();
                }
            }
        };

        return node;
    }

    [DocVariant("CC_Playground_Label_Primary")]
    public static DocSample DocsPrimary() {
        bool forced = RenderContext.Current.ForceDisabled;
        StateHandle<string> lastPick = UseState<string>(
            (string)"CC_Playground_buttongroup_None".Translate()
        );

        List<ButtonGroupItem> items = new List<ButtonGroupItem> {
            new ButtonGroupItem(
                (string)"CC_Playground_buttongroup_Day".Translate(),
                () => lastPick.Set("Day"),
                forced
            ),
            new ButtonGroupItem(
                (string)"CC_Playground_buttongroup_Week".Translate(),
                () => lastPick.Set("Week"),
                forced
            ),
            new ButtonGroupItem(
                (string)"CC_Playground_buttongroup_Month".Translate(),
                () => lastPick.Set("Month"),
                forced
            ),
        };

        return new DocSample(Create(items, ButtonVariant.Primary));
    }

    [DocVariant("CC_Playground_Label_Secondary")]
    public static DocSample DocsSecondary() {
        bool forced = RenderContext.Current.ForceDisabled;
        StateHandle<string> lastPick = UseState<string>(
            (string)"CC_Playground_buttongroup_None".Translate()
        );

        List<ButtonGroupItem> items = new List<ButtonGroupItem> {
            new ButtonGroupItem(
                (string)"CC_Playground_buttongroup_Refresh".Translate(),
                () => lastPick.Set("Refresh"),
                forced
            ),
            new ButtonGroupItem(
                (string)"CC_Playground_buttongroup_Export".Translate(),
                () => lastPick.Set("Export"),
                forced
            ),
            new ButtonGroupItem(
                (string)"CC_Playground_buttongroup_Delete".Translate(),
                () => lastPick.Set("Delete"),
                forced
            ),
            new ButtonGroupItem(
                (string)"CC_Playground_buttongroup_More".Translate(),
                () => lastPick.Set("More"),
                true
            ),
        };

        return new DocSample(Create(items));
    }

    [DocUsage]
    public static DocSample DocsUsage() {
        StateHandle<string> lastPick = UseState<string>(
            (string)"CC_Playground_buttongroup_None".Translate()
        );

        List<ButtonGroupItem> items = new List<ButtonGroupItem> {
            new ButtonGroupItem(
                (string)"CC_Playground_buttongroup_Day".Translate(),
                () => lastPick.Set("Day")),
            new ButtonGroupItem(
                (string)"CC_Playground_buttongroup_Week".Translate(),
                () => lastPick.Set("Week")),
            new ButtonGroupItem(
                (string)"CC_Playground_buttongroup_Month".Translate(),
                () => lastPick.Set("Month")),
        };

        return new DocSample(Create(items));
    }
}
