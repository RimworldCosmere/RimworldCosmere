using Cosmere.Foundation.UI;
using UnityEngine;
using Verse;

namespace Cosmere.Foundation.Window;

[StaticConstructorOnStartup]
public abstract class BaseWindow : Verse.Window {
    protected static readonly Color BodyTextColor = new Color(.80f, .80f, .80f);
    protected static readonly Color HeaderTextColor = new Color(.95f, .95f, .95f);
    protected static readonly Texture2D Border = ContentFinder<Texture2D>.Get("Window/Border");
    protected static readonly Texture2D Background = ContentFinder<Texture2D>.Get("Window/BackgroundWide");
    protected static readonly Texture2D DarkBackground = ContentFinder<Texture2D>.Get("Window/BackgroundDarker");
    protected static readonly Texture2D UniformBackground = ContentFinder<Texture2D>.Get("Window/BackgroundTall");
    protected static readonly Texture2D HeaderBar = ContentFinder<Texture2D>.Get("Window/HeaderLine");
    protected static readonly Texture2D HeaderBackground = Widgets.MenuSectionBGFillColor.ToSolidColorTexture();
    protected static readonly Texture2D BodyBackground = DarkBackground;
    protected static readonly Texture2D FooterBackground = HeaderBackground;

    private static readonly Texture2D
        InvertedDropShadow = ContentFinder<Texture2D>.Get("UI/Widgets/InvertedDropShadow");

    protected static readonly Color BodyColor = Widgets.WindowBGFillColor;
    protected static readonly Color FooterColor = Widgets.MenuSectionBGFillColor;
    protected static readonly Color BorderColor = new Color(.79f, .65f, .37f);
    protected static readonly Texture2D BorderTexture = BorderColor.ToSolidColorTexture();
    protected static readonly Texture2D CloseButton = ContentFinder<Texture2D>.Get("UI/Buttons/Abandon");
    protected static float DropShadowContract = -7f;
    protected static float DropShadowX = 1f;
    protected static float DropShadowY = 1f;

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

    protected virtual bool hasFooter { get; set; } = false;
    protected virtual bool drawBorder { get; set; } = true;
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
    protected virtual Color bodyTextColor => BodyTextColor;
    protected virtual GameFont titleFont => GameFont.Medium;
    protected virtual GameFont subtitleFont => GameFont.Small;
    protected virtual GameFont bodyFont => GameFont.Small;
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

        Widgets.DrawBox(new Rect(rect.x, rect.yMax - 3, rect.width, 3), 3, BorderTexture);
    }

    protected virtual void DrawHeaderContent(Listing_Standard listing, Rect innerRect) {
        using (new TextBlock(titleFont)) listing.Label($"<b>{GetTitle()}</b>");

        TaggedString? subtitle = GetSubtitle();
        if (subtitle == null) return;

        using (new TextBlock(subtitleFont)) listing.Label(subtitle);
    }

    protected virtual float DrawBody(Rect rect) {
        //rect = rect.ContractedBy(bodyPadding, 0);
        if (scrollViewStatus.scrollVisibile) {
            rect = new Rect(rect.x + Spacing.Get(1.25), rect.y, rect.width - Spacing.Get(1.25), rect.height);
        }

        Listing_Standard listing = new Listing_Standard { maxOneColumn = true, verticalSpacing = 0 };

        listing.Begin(rect);
        float originalWidth = listing.ColumnWidth;
        listing.ColumnWidth -= bodyPadding * 2;
        listing.Indent(bodyPadding);
        DrawBodyContent(listing);
        listing.Outdent(bodyPadding);
        listing.ColumnWidth = originalWidth;
        listing.End();
        listing.Gap(Spacing.Get());

        Widgets.DrawBox(new Rect(rect.x, rect.yMax - 3, rect.width, 3), 3, BorderTexture);

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
        if (drawBorder) Widgets.DrawBox(inRect, (int)Margin, BorderTexture);
        GUI.DrawTexture(inRect.ContractedBy(Margin), BodyBackground, ScaleMode.ScaleAndCrop);

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

        //Widgets.DrawRectFast(bodyRect, BodyColor);
        if (drawBorder) {
            Widgets.DrawBoxSolid(new Rect(bodyRect.xMin, bodyRect.y, 1, bodyRect.height), BorderColor);
            Widgets.DrawBoxSolid(new Rect(bodyRect.xMax - 1, bodyRect.y, 1, bodyRect.height), BorderColor);
        }

        using (ScrollView sv = new ScrollView(bodyRect, scrollViewStatus)) {
            using (new TextBlock(bodyFont)) sv.height = DrawBody(sv.rect);
        }

        if (hasFooter) {
            if (drawBorder) Widgets.DrawBox(footerRect, 1, BorderTexture);
            GUI.DrawTexture(footerRect.ContractedBy(Margin), FooterBackground);
            DrawFooter(footerRect);
        }
    }

    protected void DrawDropShadow(Rect rect) {
        Rect rect1 = rect.ContractedBy(DropShadowContract);
        rect1.x += DropShadowX;
        rect1.y += DropShadowY;
        Widgets.DrawAtlas(rect1, InvertedDropShadow);
    }
}