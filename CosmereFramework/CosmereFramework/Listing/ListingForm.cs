using System;
using CosmereFramework.Extension;
using CosmereFramework.UI;
using UnityEngine;
using Verse;

namespace CosmereFramework.Listing;

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

public class ListingForm : Listing_Standard {
    public new Rect listingRect => base.listingRect;

    public void Fieldset(
        TaggedString header,
        Action<ListingForm> drawContents,
        SubListingOptions? subListingOptions = null,
        HeadingOptions? headingOptions = null,
        float? height = null
    ) {
        Heading(header, headingOptions);
        SubListing(drawContents, height, subListingOptions);
    }

    public void Contain(Rect rect, Action<ListingForm> drawContents) {
        Begin(rect);
        drawContents(this);
        End();
    }

    public void Contain(Rect rect, Action<Rect, ListingForm> drawContents) {
        Begin(rect);
        drawContents(rect, this);
        End();
    }

    public void Heading(
        string text,
        HeadingOptions? options = null
    ) {
        options ??= new HeadingOptions();

        curY += options.padding.top;
        curX += options.padding.left;

        using (options.textblock ?? new TextBlock(GameFont.Medium)) Label(text);
        if (options.lineSeparator) GapLine(4);

        curY += options.padding.bottom;
        curX -= options.padding.left;
    }

    protected virtual void SubListing(
        Action<ListingForm> drawContents,
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
            } else {
                Rect rect = GetRect(height.Value).ContractedBy(options.padding);
                ListingForm subListing = new ListingForm { verticalSpacing = options.verticalSpacing };
                subListing.Contain(rect, drawContents);
            }
        }

        Gap(verticalSpacing);
    }

    public void Field(
        TaggedString label,
        Action<ListingForm> drawContents,
        FieldOptions? fieldOptions = null,
        SubListingOptions? subListingOptions = null
    ) {
        Field(label, null, drawContents, fieldOptions, subListingOptions);
    }

    public void Field(
        TaggedString label,
        TaggedString? tooltip,
        Action<ListingForm> drawContents,
        FieldOptions? fieldOptions = null,
        SubListingOptions? subListingOptions = null
    ) {
        fieldOptions ??= new FieldOptions();
        subListingOptions ??= new SubListingOptions();

        SubListing(
            sub => {
                if (tooltip.HasValue) TooltipHandler.TipRegion(sub.listingRect, tooltip.Value);

                // Create the label
                sub.ColumnWidth = fieldOptions.labelWidth;
                Rect rect = sub.GetRect(fieldOptions.height);
                rect.width = fieldOptions.labelWidth;
                Widgets.Label(rect, label);

                // Split
                sub.NewColumn();

                // Create the field
                sub.ColumnWidth = Mathf.Max(
                    fieldOptions.minimumColumnWidth,
                    sub.listingRect.width - fieldOptions.labelWidth - fieldOptions.columnSpacing
                );
                using (new TextBlock(TextAnchor.MiddleCenter)) drawContents(sub);
            },
            subListingOptions.padding.top +
            subListingOptions.padding.bottom +
            fieldOptions.height +
            subListingOptions.verticalSpacing,
            subListingOptions.WithPadding(Padding.Zero)
        );
    }
}