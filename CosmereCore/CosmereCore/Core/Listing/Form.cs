using System;
using Cosmere.Core.UI;
using UnityEngine;
using Verse;

namespace Cosmere.Core.Listing;

public record SubListingOptions {
    public const float Padding = 5f;
    public const float Inset = 30f;
    public const float VerticalSpacing = 2f;

    public Padding padding = new Padding(
        Padding,
        Padding,
        Padding,
        Padding + Inset
    );

    public TextBlock? textBlock;
    public float verticalSpacing = VerticalSpacing;

    public static SubListingOptions WithoutTopPadding() {
        SubListingOptions options = new SubListingOptions();
        options.padding.top = 0;

        return options;
    }

    public SubListingOptions WithPadding(Padding newPadding) {
        padding = newPadding;

        return this;
    }

    public SubListingOptions WithTextBlock(TextBlock newTextBlock) {
        textBlock = newTextBlock;

        return this;
    }
}

public record FieldOptions {
    public const float ColumnSpacing = 17f;
    public const float RowHeight = 40f;
    public const float LabelWidth = 150f;

    public float columnSpacing = ColumnSpacing;
    public float height = RowHeight;
    public float labelWidth = LabelWidth;
    public float minimumColumnWidth = 100;
}

public record HeadingOptions {
    public const float Padding = 12f;

    public bool lineSeparator = true;
    public Padding padding = new Padding(Padding);
    public TextBlock? textblock;
}

public class Form : FoundationListing {
    private readonly ScrollViewStatus scrollViewStatus = new ScrollViewStatus();
    public Form? parentListing;
    public ScrollView? scrollView;

    public float currentHeight {
        get => scrollView?.height ?? CurHeight;
        set {
            if (scrollView != null) scrollView.Value.height = value;
        }
    }

    public override void Begin(Rect rect) {
        ColumnWidth = rect.width - 36;
        base.Begin(rect);
        if (parentListing == null) {
            scrollView = new ScrollView(new Rect(0, 0, rect.width, rect.height), scrollViewStatus);
        }
    }

    public override void End() {
        base.End();
        if (parentListing == null) scrollView?.Dispose();
    }

    public void Fieldset(
        TaggedString header,
        Action<Form> drawContents,
        SubListingOptions? subListingOptions = null,
        HeadingOptions? headingOptions = null,
        float? height = null
    ) {
        Heading(header, headingOptions);
        SubListing(drawContents, height, subListingOptions);
    }

    public void Contain(Rect rect, Action<Form> drawContents) {
        Begin(rect);
        drawContents(this);
        End();
    }

    public void Heading(
        string text,
        HeadingOptions? options = null
    ) {
        options ??= new HeadingOptions();
        using (options.textblock ?? new TextBlock(GameFont.Medium)) {
            Pad(
                () => {
                    Label(text);
                    if (options.lineSeparator) {
                        GapLine(4);
                        currentHeight += 4;
                    }
                },
                options.padding
            );

            currentHeight += Text.CalcSize(text).y;
        }
    }

    public virtual void Pad(Action content, Padding padding) {
        curY += padding.top;
        curX += padding.left;
        content();
        curY += padding.bottom;
        curX -= padding.left;

        currentHeight += padding.top + padding.bottom;
    }

    protected virtual void SubListing(
        Action<Form> drawContents,
        float? height = null,
        SubListingOptions? options = null
    ) {
        options ??= new SubListingOptions();
        TextBlock textBlock = options.textBlock ?? new TextBlock(GameFont.Medium, TextAnchor.UpperLeft);
        using (textBlock) {
            if (height == null) {
                curY += options.padding.top;
                curX += options.padding.left;

                drawContents(this);

                curY += options.padding.bottom;
                curX -= options.padding.left;

                currentHeight += options.padding.top + options.padding.bottom;
            } else {
                Rect rect = GetRect(height.Value).ContractedBy(options.padding);
                Form sub = new Form
                    { verticalSpacing = options.verticalSpacing, parentListing = this };

                sub.Contain(rect, drawContents);
                currentHeight += height.Value + options.verticalSpacing;
            }
        }

        Gap(verticalSpacing);
        currentHeight += verticalSpacing;
    }

    public void Field(
        Action<Form> drawContents,
        FieldOptions? fieldOptions = null,
        SubListingOptions? subListingOptions = null
    ) {
        Field(null, null, drawContents, fieldOptions, subListingOptions);
    }

    public void Field(
        TaggedString label,
        Action<Form> drawContents,
        FieldOptions? fieldOptions = null,
        SubListingOptions? subListingOptions = null
    ) {
        Field(label, null, drawContents, fieldOptions, subListingOptions);
    }

    public void Field(
        TaggedString? label,
        TaggedString? tooltip,
        Action<Form> drawContents,
        FieldOptions? fieldOptions = null,
        SubListingOptions? subListingOptions = null
    ) {
        fieldOptions ??= new FieldOptions();
        subListingOptions ??= new SubListingOptions();

        float height = subListingOptions.padding.top +
                       subListingOptions.padding.bottom +
                       fieldOptions.height +
                       subListingOptions.verticalSpacing;

        SubListing(
            sub => {
                if (tooltip.HasValue) {
                    TooltipHandler.TipRegion(new Rect(0, 0, sub.listingRect.width, height), tooltip.Value);
                }

                // Create the label
                float originalWidth = sub.ColumnWidth;
                sub.ColumnWidth = fieldOptions.labelWidth;
                using (subListingOptions.textBlock ?? new TextBlock(TextAnchor.MiddleLeft))
                    sub.Label(label, fieldOptions.height);

                // Split
                sub.NewColumn();
                sub.ColumnWidth = originalWidth;

                // Create the field
                sub.ColumnWidth = Mathf.Max(
                    fieldOptions.minimumColumnWidth,
                    sub.ColumnWidth -
                    fieldOptions.labelWidth -
                    fieldOptions.columnSpacing -
                    subListingOptions.padding.left -
                    subListingOptions.padding.right -
                    16
                );
                using (new TextBlock(TextAnchor.MiddleCenter)) drawContents(sub);
                sub.ColumnWidth = originalWidth;
            },
            height,
            subListingOptions.WithPadding(Padding.Zero)
        );
    }
}