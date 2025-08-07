using Cosmere.Framework.UI;
using UnityEngine;
using Verse;

namespace Cosmere.Framework.Window;

[StaticConstructorOnStartup]
public abstract class BaseWindow : Verse.Window {
    protected static readonly Color HeaderTextColor = new ColorInt(26, 58, 97).ToColor;
    protected static readonly Texture2D HeaderBackground = ContentFinder<Texture2D>.Get("Window/BackgroundWide");
    protected static readonly Color BodyColor = Widgets.WindowBGFillColor;
    protected static readonly Color SurgeColor = Widgets.MenuSectionBGFillColor;
    protected static readonly Color FooterColor = Widgets.MenuSectionBGFillColor;
    protected static readonly Color BorderColor = new ColorInt(208, 164, 80).ToColor;
    protected static readonly Texture2D BorderTexture = BorderColor.ToSolidColorTexture();
    protected static readonly Texture2D CloseButton = ContentFinder<Texture2D>.Get("UI/Buttons/Abandon");

    private readonly ScrollViewStatus scrollViewStatus = new ScrollViewStatus();

    protected BaseWindow() {
        forcePause = true;
        draggable = true;
        doWindowBackground = false;
    }

    protected BaseWindow(Vector2 initialWindowSize) : this() {
        this.initialWindowSize = initialWindowSize;
    }

    protected virtual Vector2 initialWindowSize { get; }

    protected virtual bool hasFooter => false;
    protected virtual bool drawBorder => true;
    protected virtual float headerHeight => Spacing.Get(6);
    protected virtual float footerHeight => hasFooter ? Spacing.Get(4) : 0;
    protected virtual float footerButtonHeight => Spacing.Get(2);
    protected virtual float bodyHeight => initialWindowSize.y - headerHeight - footerHeight;
    protected virtual Padding padding => Padding.Spacing;

    protected virtual float bodyPadding => Spacing.Get(scrollViewStatus.scrollVisibile ? 2 : 4);
    protected override float Margin => drawBorder ? 1 : 0;
    public sealed override Vector2 InitialSize => initialWindowSize;

    protected virtual TextAnchor headerAlignment => TextAnchor.MiddleCenter;
    protected virtual Color headerTextColor => HeaderTextColor;
    protected virtual GameFont titleFont => GameFont.Medium;
    protected virtual GameFont subtitleFont => GameFont.Small;
    protected abstract TaggedString GetTitle();

    protected virtual TaggedString? GetSubtitle() {
        return null;
    }

    protected virtual void DrawHeader(Rect rect) {
        Rect innerRect = rect.ContractedBy(padding);
        Listing_Standard listing = new Listing_Standard { maxOneColumn = true };
        listing.Begin(innerRect);

        using (new TextBlock(headerAlignment, headerTextColor)) {
            DrawHeaderContent(listing, innerRect);
        }

        listing.End();
    }

    protected virtual void DrawHeaderContent(Listing_Standard listing, Rect innerRect) {
        using (new TextBlock(titleFont)) listing.Label($"<b>{GetTitle()}</b>");

        TaggedString? subtitle = GetSubtitle();
        if (subtitle == null) return;

        using (new TextBlock(subtitleFont)) listing.Label(subtitle);
    }

    protected virtual float DrawBody(Rect rect) {
        rect = rect.ContractedBy(bodyPadding, 0);
        if (scrollViewStatus.scrollVisibile) {
            rect = new Rect(rect.x + Spacing.Get(1.25), rect.y, rect.width - Spacing.Get(1.25), rect.height);
        }

        Listing_Standard listing = new Listing_Standard { maxOneColumn = true, verticalSpacing = 0 };

        listing.Begin(rect);
        DrawBodyContent(listing);
        listing.End();
        listing.Gap(Spacing.Get());

        return listing.CurHeight;
    }

    protected abstract void DrawBodyContent(Listing_Standard listing);

    protected virtual void DrawFooter(Rect rect) { }

    protected virtual bool CloseButtonFor(Rect rectToClose) {
        float padding = Spacing.Get(.5);
        float imageWidth = Spacing.Get(1.5);

        return Widgets.ButtonImage(
            new Rect(
                rectToClose.x + rectToClose.width - padding - imageWidth,
                rectToClose.y + padding,
                imageWidth,
                imageWidth
            ),
            CloseButton
        );
    }

    public override void DoWindowContents(Rect inRect) {
        Rect headerRect = new Rect(inRect.x, inRect.y, inRect.width, headerHeight);
        Rect bodyRect = new Rect(
            inRect.x,
            inRect.y + headerHeight,
            inRect.width,
            bodyHeight
        );
        Rect footerRect = new Rect(inRect.x, inRect.y + inRect.height - footerHeight, inRect.width, footerHeight);

        if (drawBorder) Widgets.DrawBox(headerRect, 1, BorderTexture);
        GUI.DrawTexture(headerRect.ContractedBy(Margin), HeaderBackground);
        DrawHeader(headerRect);

        if (CloseButtonFor(inRect.AtZero())) {
            Close();
        }

        Widgets.DrawRectFast(bodyRect, BodyColor);
        if (drawBorder) {
            Widgets.DrawBoxSolid(new Rect(bodyRect.xMin, bodyRect.y, 1, bodyRect.height), BorderColor);
            Widgets.DrawBoxSolid(new Rect(bodyRect.xMax - 1, bodyRect.y, 1, bodyRect.height), BorderColor);
        }

        using (ScrollView sv = new ScrollView(bodyRect, scrollViewStatus)) {
            sv.height = DrawBody(sv.rect);
        }

        if (hasFooter) {
            if (drawBorder) Widgets.DrawBox(footerRect, 1, BorderTexture);
            Widgets.DrawRectFast(footerRect.ContractedBy(Margin), FooterColor);
            DrawFooter(footerRect);
        }
    }

    protected bool ButtonText(
        Listing_Standard listing,
        string label,
        string? highlightTag = null,
        float widthPct = 1f
    ) {
        Rect rect = listing.GetRect(footerButtonHeight, widthPct);
        bool flag = false;
        if (!listing.BoundingRectCached.HasValue || rect.Overlaps(listing.BoundingRectCached.Value)) {
            flag = Widgets.ButtonText(rect, label);
            if (highlightTag != null) {
                UIHighlighter.HighlightOpportunity(rect, highlightTag);
            }
        }

        listing.Gap(listing.verticalSpacing);

        return flag;
    }
}