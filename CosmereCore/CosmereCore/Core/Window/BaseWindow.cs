using Cosmere.Core.Listing;
using Cosmere.Core.UI;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Cosmere.Core.Window;

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

    public static Texture2D CTAButtonBGAtlas = ContentFinder<Texture2D>.Get("UI/Widgets/CTAButtonBG");
    public static Texture2D CTAButtonBGAtlasMouseover = ContentFinder<Texture2D>.Get("UI/Widgets/CTAButtonBGMouseover");
    public static Texture2D CTAButtonBGAtlasClick = ContentFinder<Texture2D>.Get("UI/Widgets/CTAButtonBGClick");

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

    protected virtual bool drawBorder { get; set; } = false;

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
        FoundationListing listing = new FoundationListing { maxOneColumn = true };
        listing.Begin(innerRect);

        using (new TextBlock(headerAlignment, headerTextColor)) {
            DrawHeaderContent(listing, innerRect);
        }

        listing.End();
    }

    protected virtual void DrawHeaderContent(FoundationListing listing, Rect innerRect) {
        using (new TextBlock(titleFont)) listing.Label($"<b>{GetTitle()}</b>");

        TaggedString? subtitle = GetSubtitle();
        if (subtitle == null) return;

        using (new TextBlock(subtitleFont)) listing.Label(subtitle);
    }

    protected virtual float DrawBody(Rect rect) {
        if (scrollViewStatus.scrollVisibile) {
            rect = new Rect(rect.x + Spacing.Get(1.25), rect.y, rect.width - Spacing.Get(1.25), rect.height);
        }

        FoundationListing listing = new FoundationListing { maxOneColumn = true, verticalSpacing = 0 };

        listing.Begin(rect);
        float originalWidth = listing.ColumnWidth;
        listing.ColumnWidth -= bodyPadding * 2;
        listing.Indent(bodyPadding);
        DrawBodyContent(listing);
        listing.Outdent(bodyPadding);
        listing.ColumnWidth = originalWidth;
        listing.End();
        listing.Gap();

        return listing.CurHeight;
    }

    protected abstract void DrawBodyContent(FoundationListing listing);

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

        GUI.DrawTexture(headerRect.ContractedBy(Margin), HeaderBackground);
        DrawHeader(headerRect);
        DrawBorder(headerRect, 3, bottom: true);

        if (CloseButtonFor(inRect.AtZero())) {
            Close();
        }

        using (ScrollView sv = new ScrollView(bodyRect, scrollViewStatus)) {
            using (new TextBlock(bodyFont)) sv.height = DrawBody(sv.rect);
        }

        if (hasFooter) {
            GUI.DrawTexture(footerRect.ContractedBy(Margin), FooterBackground);
            DrawFooter(footerRect);
            DrawBorder(footerRect.ContractedBy(Margin), 3, top: true);
        }
    }

    protected virtual void DrawBorder(
        Rect rect,
        int thickness,
        Texture2D? borderTexture = null,
        bool top = false,
        bool right = false,
        bool bottom = false,
        bool left = false
    ) {
        if (thickness <= 0) return;
        Texture2D texture = borderTexture ?? BorderTexture;

        if (top) Widgets.DrawBox(new Rect(rect.xMin, rect.yMin, rect.width, thickness), thickness, texture);
        if (right) {
            Widgets.DrawBox(new Rect(rect.xMax - thickness, rect.yMin, thickness, rect.height), thickness, texture);
        }

        if (bottom) {
            Widgets.DrawBox(new Rect(rect.xMin, rect.yMax - thickness, rect.width, thickness), thickness, texture);
        }

        if (left) Widgets.DrawBox(new Rect(rect.xMin, rect.yMin, thickness, rect.height), thickness, texture);
    }

    protected void DrawDropShadow(Rect rect) {
        Rect rect1 = rect.ContractedBy(DropShadowContract);
        rect1.x += DropShadowX;
        rect1.y += DropShadowY;
        Widgets.DrawAtlas(rect1, InvertedDropShadow);
    }

    public virtual bool CTAButtonText(Rect rect, string label) {
        Texture2D atlas = CTAButtonBGAtlas;
        if (Mouse.IsOver(rect)) {
            atlas = CTAButtonBGAtlasMouseover;
            if (Input.GetMouseButton(0)) {
                atlas = CTAButtonBGAtlasClick;
            }
        }

        Widgets.DrawAtlas(rect, atlas);
        MouseoverSounds.DoRegion(rect);

        using (new TextBlock(null, TextAnchor.MiddleCenter, rect.height >= Text.LineHeight * 2f, Color.white)) {
            Widgets.Label(rect, label);
        }

        return Widgets.ButtonInvisible(rect, false);
    }
}
