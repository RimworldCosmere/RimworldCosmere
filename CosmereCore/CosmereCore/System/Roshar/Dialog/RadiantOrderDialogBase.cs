using System;
using Cosmere.Core.Listing;
using Cosmere.Core.UI;
using Cosmere.Core.Window;
using Cosmere.System.Roshar.Def;
using Cosmere.System.Roshar.Gene;
using Cosmere.System.Roshar.Settings;
using RimWorld;
using UnityEngine;
using Verse;
using Logger = Cosmere.Core.Logger;
using TraitRequirement = Verse.TraitRequirement;

namespace Cosmere.System.Roshar.Dialog;

public enum DialogTab {
    Overview,
    Ideals,
    Traits,
}

[StaticConstructorOnStartup]
public abstract class RadiantOrderDialogBase : BaseWindow {
    private const float DefaultSurgeHeight = 175f;
    private const float TabHeight = 35f;
    private const float TabUnderlineHeight = 3f;
    private const float IdealDotSize = 12f;

    private const float BannerSize = 160f;
    private const float BannerOverhang = 15f;

    private const float TimelineDotRadius = 9f;
    private const float TimelineLineWidth = 3f;
    private const float TimelineLeftMargin = 28f;
    private const float TimelineContentIndent = 56f;

    private static readonly Color AchievedColor = new Color(0.3f, 0.85f, 0.3f);
    private static readonly Color CurrentColor = new Color(0.95f, 0.85f, 0.2f);
    private static readonly Color FutureColor = new Color(0.5f, 0.5f, 0.5f);
    private static readonly Color BlockedColor = new Color(0.85f, 0.2f, 0.2f);

    private static readonly Texture2D CircleTex = CreateCircleTexture(32);
    protected readonly Color accentColor;
    protected readonly Pawn? pawn;
    protected readonly Surgebinder? surgebinder;

    private DialogTab currentTab;

    protected RadiantOrderDef order;
    private float? surgeHeight;

    protected RadiantOrderDialogBase(RadiantOrderDef order, Pawn? pawn = null, Surgebinder? surgebinder = null) {
        this.order = order;
        this.pawn = pawn;
        this.surgebinder = surgebinder;
        accentColor = order.color;
        currentTab = DialogTab.Overview;
    }

    protected override Vector2 initialWindowSize => new Vector2(
        Spacing.Get(65),
        Mathf.Max(Spacing.Get(30), Verse.UI.screenHeight - Spacing.Get(10))
    );

    protected override float headerHeight => Spacing.Get(16);

    protected override void DrawHeaderContent(FoundationListing listing, Rect innerRect) {
        listing.Gap(BannerSize - BannerOverhang + Spacing.Get(0.5f));

        using (new TextBlock(GameFont.Medium, TextAnchor.MiddleCenter, headerTextColor))
            listing.Label($"<b>{order.LabelCap}</b>");

        TaggedString title = GetTitle();
        if (title != null) {
            using (new TextBlock(GameFont.Small, TextAnchor.MiddleCenter, bodyTextColor))
                listing.Label(title);
        }
    }

    public override void DoWindowContents(Rect inRect) {
        base.DoWindowContents(inRect);

        float bannerX = inRect.x + (inRect.width - BannerSize) / 2f;
        float bannerY = inRect.y - BannerOverhang;
        Rect bannerRect = new Rect(bannerX, bannerY, BannerSize, BannerSize);
        GUI.DrawTexture(bannerRect, order.bannerIcon, ScaleMode.ScaleToFit);
    }

    protected override void DrawBodyContent(FoundationListing listing) {
        DrawTabBar(listing);
        listing.Gap();

        switch (currentTab) {
            case DialogTab.Overview:
                DrawOverviewTab(listing);
                break;
            case DialogTab.Ideals:
                DrawIdealsTab(listing);
                break;
            case DialogTab.Traits:
                DrawTraitsTab(listing);
                break;
        }
    }

    private void DrawTabBar(FoundationListing listing) {
        float bodyPad = bodyPadding;
        Rect tabRow = listing.GetRect(TabHeight);
        tabRow.x -= bodyPad;
        tabRow.width += bodyPad * 2;
        float tabWidth = tabRow.width / 3f;

        DrawTab(new Rect(tabRow.x, tabRow.y, tabWidth, TabHeight), "Overview", DialogTab.Overview);
        DrawTab(new Rect(tabRow.x + tabWidth, tabRow.y, tabWidth, TabHeight), "Ideals", DialogTab.Ideals);
        DrawTab(new Rect(tabRow.x + tabWidth * 2f, tabRow.y, tabWidth, TabHeight), "Traits", DialogTab.Traits);

        DrawBorder(
            tabRow.With(y: tabRow.yMax - 1, height: 1),
            1,
            bottom: true
        );
    }

    private void DrawTab(Rect rect, string label, DialogTab tab) {
        bool isActive = currentTab == tab;

        Color textColor = isActive ? headerTextColor : bodyTextColor;
        using (new TextBlock(GameFont.Medium, TextAnchor.MiddleCenter, textColor))
            Widgets.Label(rect, isActive ? $"<b>{label}</b>" : label);

        if (isActive) {
            Texture2D accentTexture = accentColor.ToSolidColorTexture();
            Rect underline = new Rect(rect.x, rect.yMax - TabUnderlineHeight, rect.width, TabUnderlineHeight);
            GUI.DrawTexture(underline, accentTexture);
        }

        if (Widgets.ButtonInvisible(rect)) {
            currentTab = tab;
        }
    }

    protected virtual void DrawOverviewTab(FoundationListing listing) {
        using (new TextBlock(GameFont.Small, TextAnchor.UpperLeft, bodyTextColor)) {
            float width = listing.ColumnWidth;
            float height = Text.CalcHeight(order.description, width);
            Rect rect = listing.GetRect(height);
            Widgets.Label(rect, order.description);
        }

        listing.Gap();

        int quoteIdealIndex = 1;
        if (surgebinder != null && surgebinder.currentIdeal < order.ideals.Count - 1) {
            quoteIdealIndex = surgebinder.currentIdeal + 1;
        }

        if (quoteIdealIndex < order.ideals.Count && order.ideals[quoteIdealIndex].quotes.Count > 0) {
            string quote = order.ideals[quoteIdealIndex].quotes[0];
            using (new TextBlock(GameFont.Small, TextAnchor.MiddleCenter, bodyTextColor))
                listing.Label($"<i>\"{quote}\"</i>");
            listing.Gap();
        }

        listing.GapLine(color: BorderColor);
        listing.Gap();

        using (new TextBlock(GameFont.Medium, TextAnchor.MiddleCenter, bodyTextColor)) {
            TaggedString str = "CRO_Bond_Choose_Surges".Translate(order.LabelCap.Named("ORDER"));
            Rect labelRect = listing.Label($"<b>{str}</b>");
            DrawBorder(
                labelRect.With(y: labelRect.yMax, height: 1),
                1,
                bottom: true
            );
        }

        listing.Gap();

        Rect surgeRow = listing.GetRect(surgeHeight ?? DefaultSurgeHeight);
        surgeRow.SplitVerticallyWithMargin(out Rect leftBox, out Rect rightBox, Spacing.Get());

        float leftHeight = DrawSurgeBox(leftBox, order.surges[0]);
        float rightHeight = DrawSurgeBox(rightBox, order.surges[1]);

        surgeHeight = Mathf.Max(DefaultSurgeHeight, leftHeight, rightHeight);
    }

    protected float DrawSurgeBox(Rect rect, SurgeDef surge) {
        float imageSize = Spacing.Get(5);
        DrawDropShadow(rect);
        GUI.DrawTexture(rect, HeaderBackground, ScaleMode.StretchToFill);

        Vector2 titleSize;
        using (new TextBlock(GameFont.Medium)) titleSize = Text.CalcSize(surge.LabelCap);

        Rect inner = rect.ContractedBy(16);
        inner.SplitVerticallyWithMargin(
            out Rect leftBox,
            out Rect rightBox,
            out float overflow,
            Spacing.Get(),
            Mathf.Max(imageSize, titleSize.x)
        );

        float descriptionHeight;
        using (new TextBlock(GameFont.Small, null, true))
            descriptionHeight = Text.CalcHeight(surge.description, rightBox.width);
        float currentSurgeHeight = surgeHeight ?? DefaultSurgeHeight;
        float leftPadding = currentSurgeHeight - imageSize - titleSize.y - Spacing.Get(2);
        float rightPadding = currentSurgeHeight - descriptionHeight - Spacing.Get(2);

        FoundationListing leftListing = new FoundationListing { maxOneColumn = true, verticalSpacing = 0 };
        leftListing.Begin(leftBox);
        leftListing.Gap(leftPadding / 2);

        Rect imageRect = leftListing.GetRect(imageSize);
        Rect centered = new Rect(
            imageRect.x + (imageRect.width - imageSize) / 2f,
            imageRect.y,
            imageSize,
            imageSize
        );
        GUI.DrawTexture(centered, surge.icon, ScaleMode.ScaleToFit);
        leftListing.Gap();

        using (new TextBlock(GameFont.Medium, TextAnchor.MiddleCenter, headerTextColor))
            leftListing.Label(surge.LabelCap);
        leftListing.End();

        FoundationListing rightListing = new FoundationListing { maxOneColumn = true, verticalSpacing = 0 };
        rightListing.Begin(rightBox);
        rightListing.Gap(rightPadding / 2);

        using (new TextBlock(GameFont.Small, TextAnchor.UpperLeft, headerTextColor))
            rightListing.Label(surge.description);

        rightListing.End();
        rightListing.Gap();

        return Mathf.Max(leftListing.CurHeight, rightListing.CurHeight);
    }

    private static Texture2D CreateCircleTexture(int size) {
        Texture2D tex = new Texture2D(size, size, TextureFormat.ARGB32, false);
        float radius = size / 2f;
        float center = size / 2f;
        for (int y = 0; y < size; y++) {
            for (int x = 0; x < size; x++) {
                float dist = Mathf.Sqrt((x - center) * (x - center) + (y - center) * (y - center));
                tex.SetPixel(x, y, dist <= radius - 0.5f ? Color.white : Color.clear);
            }
        }

        tex.Apply();
        return tex;
    }

    protected virtual void DrawIdealsTab(FoundationListing listing) {
        RosharModSettings settings = Core.Mod.GetModSettings<RosharModSettings>();
        int currentIdealLevel = surgebinder?.currentIdeal ?? -1;
        bool hasPendingOath = surgebinder?.PendingOath ?? false;

        listing.Gap(Spacing.Get());

        for (int i = 0; i < order.ideals.Count; i++) {
            Ideal ideal = order.ideals[i];
            IdealStatus status = GetIdealStatus(i, currentIdealLevel, hasPendingOath);

            float rowStartY = listing.CurHeight;
            DrawIdealRow(listing, ideal, i, status, settings.showIdealRequirements);
            float rowEndY = listing.CurHeight;

            float dotCenterX = listing.ListingRect.x + TimelineLeftMargin;
            float dotCenterY = rowStartY + TimelineDotRadius + 4f;
            float dotDiameter = TimelineDotRadius * 2f;

            if (i < order.ideals.Count - 1) {
                Color lineColor = status == IdealStatus.Achieved
                    ? new Color(AchievedColor.r, AchievedColor.g, AchievedColor.b, 0.6f)
                    : new Color(0.25f, 0.25f, 0.25f);
                Rect lineRect = new Rect(
                    dotCenterX - TimelineLineWidth / 2f,
                    dotCenterY + TimelineDotRadius + 2f,
                    TimelineLineWidth,
                    rowEndY - dotCenterY - TimelineDotRadius + Spacing.Get()
                );
                Widgets.DrawBoxSolid(lineRect, lineColor);
            }

            Color statusColor = GetStatusColor(status);

            if (status == IdealStatus.Current) {
                float glowSize = dotDiameter + 8f;
                Rect glowRect = new Rect(dotCenterX - glowSize / 2f, dotCenterY - glowSize / 2f, glowSize, glowSize);
                Color glowColor = new Color(accentColor.r, accentColor.g, accentColor.b, 0.25f);
                GUI.color = glowColor;
                GUI.DrawTexture(glowRect, CircleTex);
                GUI.color = Color.white;
            }

            Rect dotRect = new Rect(
                dotCenterX - TimelineDotRadius,
                dotCenterY - TimelineDotRadius,
                dotDiameter,
                dotDiameter
            );
            GUI.color = statusColor;
            GUI.DrawTexture(dotRect, CircleTex);
            GUI.color = Color.white;

            listing.Gap(Spacing.Get(1.5f));
        }
    }

    private Color GetStatusColor(IdealStatus status) {
        return status switch {
            IdealStatus.Achieved => AchievedColor,
            IdealStatus.Current => CurrentColor,
            IdealStatus.Blocked => BlockedColor,
            _ => FutureColor,
        };
    }

    private IdealStatus GetIdealStatus(int idealIndex, int currentIdeal, bool hasPendingOath) {
        if (surgebinder == null) return IdealStatus.Future;
        if (idealIndex <= currentIdeal) return IdealStatus.Achieved;

        if (idealIndex == currentIdeal + 1) {
            if (idealIndex >= 2 &&
                order.idealChecker != null &&
                pawn != null &&
                order.idealChecker.HasIncompatibleTrait(pawn, idealIndex)) {
                return IdealStatus.Blocked;
            }

            return hasPendingOath ? IdealStatus.Current : IdealStatus.Current;
        }

        return IdealStatus.Future;
    }

    private void DrawIdealRow(
        FoundationListing listing,
        Ideal ideal,
        int index,
        IdealStatus status,
        bool showRequirements
    ) {
        Color statusColor = GetStatusColor(status);

        listing.Indent(TimelineContentIndent);
        float originalWidth = listing.ColumnWidth;
        listing.ColumnWidth -= TimelineContentIndent;

        string statusSuffix = status switch {
            IdealStatus.Achieved => " ✓",
            IdealStatus.Current => "",
            IdealStatus.Blocked => " (Blocked)",
            _ => "",
        };

        using (new TextBlock(GameFont.Medium, TextAnchor.UpperLeft, statusColor))
            listing.Label($"<b>{ideal.label.CapitalizeFirst()}{statusSuffix}</b>");

        listing.Gap(Spacing.Get(0.25f));

        if (ideal.quotes.Count > 0) {
            using (new TextBlock(GameFont.Small, TextAnchor.UpperLeft, new Color(0.65f, 0.65f, 0.65f)))
                listing.Label($"<i>\"{ideal.quotes[0]}\"</i>");
            listing.Gap(Spacing.Get(0.25f));
        }

        if (showRequirements && status != IdealStatus.Achieved) {
            string? requirements = GetIdealRequirements(index);
            if (requirements != null) {
                listing.Gap(Spacing.Get(0.15f));
                using (new TextBlock(GameFont.Small, TextAnchor.UpperLeft, new Color(0.5f, 0.5f, 0.5f)))
                    listing.Label(requirements);
            }
        }

        List<AbilityDef> abilitiesToShow = [];
        if (index == 0) {
            abilitiesToShow.AddRange(order.abilities);
        }

        for (int s = 0; s < order.surges.Count; s++) {
            for (int a = 0; a < order.surges[s].abilities.Count; a++) {
                AbilityDef abilityDef = order.surges[s].abilities[a];
                int minIdeal = abilityDef is SurgebindingAbilityDef surgeDef
                    ? surgeDef.GetMinIdealForOrder(order.defName)
                    : 0;
                if (minIdeal == index) {
                    abilitiesToShow.Add(abilityDef);
                }
            }
        }

        if (ideal.abilities != null) {
            abilitiesToShow.AddRange(ideal.abilities);
        }

        if (abilitiesToShow.Count > 0) {
            string abilityNames = GetAbilityNames(abilitiesToShow);
            using (new TextBlock(GameFont.Tiny, TextAnchor.UpperLeft, accentColor))
                listing.Label($"Unlocks: {abilityNames}");
        }

        listing.ColumnWidth = originalWidth;
        listing.Outdent(TimelineContentIndent);
    }

    private static string GetAbilityNames(List<AbilityDef> abilities) {
        string result = "";
        for (int i = 0; i < abilities.Count; i++) {
            if (i > 0) result += ", ";
            result += abilities[i].LabelCap;
        }

        return result;
    }

    protected virtual string? GetIdealRequirements(int idealIndex) {
        if (idealIndex < 0 || idealIndex >= order.ideals.Count) return null;

        string? description = order.ideals[idealIndex].description;
        string? requirements = order.idealChecker?.GetRequirementsText(idealIndex);

        if (requirements != null && description != null) return $"{description}\n{requirements}";
        return requirements ?? description;
    }

    protected virtual void DrawTraitsTab(FoundationListing listing) {
        using (new TextBlock(GameFont.Medium, TextAnchor.UpperLeft, headerTextColor))
            listing.Label("<b>Favorable Traits</b>");

        using (new TextBlock(GameFont.Tiny, TextAnchor.UpperLeft, bodyTextColor))
            listing.Label("1.25x progression speed");

        listing.Gap(Spacing.Get(0.5f));

        if (order.favorableTraits != null && order.favorableTraits.Count > 0) {
            DrawTraitPills(listing, order.favorableTraits, AchievedColor);
        } else {
            using (new TextBlock(GameFont.Small, TextAnchor.UpperLeft, FutureColor))
                listing.Label("None");
        }

        listing.Gap();
        listing.GapLine(color: BorderColor);
        listing.Gap();

        using (new TextBlock(GameFont.Medium, TextAnchor.UpperLeft, headerTextColor))
            listing.Label("<b>Incompatible Traits</b>");

        using (new TextBlock(GameFont.Tiny, TextAnchor.UpperLeft, bodyTextColor))
            listing.Label("Blocks progression at 3rd Ideal");

        listing.Gap(Spacing.Get(0.5f));

        if (order.incompatibleTraits != null && order.incompatibleTraits.Count > 0) {
            DrawTraitPills(listing, order.incompatibleTraits, BlockedColor);
        } else {
            using (new TextBlock(GameFont.Small, TextAnchor.UpperLeft, FutureColor))
                listing.Label("None");
        }
    }

    private void DrawTraitPills(FoundationListing listing, List<TraitRequirement> traits, Color pillColor) {
        float pillHeight = Spacing.Get(1.75f);
        float pillPadding = Spacing.Get(0.5f);
        float x = listing.CurrentX;
        float maxWidth = listing.ColumnWidth;
        float currentX = 0f;

        Rect rowRect = listing.GetRect(pillHeight);
        for (int i = 0; i < traits.Count; i++) {
            TraitRequirement trait = traits[i];
            int degree = trait.degree ?? 0;
            TraitDegreeData? degreeData = null;
            try {
                degreeData = trait.def?.DataAtDegree(degree);
            } catch (Exception ex) {
                Logger.Verbose(
                    $"RadiantOrderDialogBase: DataAtDegree({degree}) failed for trait '{trait.def?.defName}': {ex.Message}"
                );
            }

            string traitLabel = degreeData?.label ?? trait.def?.label ?? trait.def?.defName ?? "Unknown";
            traitLabel = traitLabel.CapitalizeFirst();

            bool pawnHasTrait = pawn != null && trait.HasTrait(pawn);

            Vector2 textSize;
            using (new TextBlock(GameFont.Small)) textSize = Text.CalcSize(traitLabel);

            float pillWidth = textSize.x + Spacing.Get(1.5f);

            if (currentX + pillWidth > maxWidth && currentX > 0f) {
                currentX = 0f;
                rowRect = listing.GetRect(pillHeight);
            }

            Rect pillRect = new Rect(rowRect.x + currentX, rowRect.y, pillWidth, pillHeight);

            Color bgColor;
            Color borderCol;
            Color textColor;
            if (pawnHasTrait) {
                bgColor = new Color(pillColor.r, pillColor.g, pillColor.b, 0.4f);
                borderCol = pillColor;
                textColor = Color.white;
            } else {
                bgColor = new Color(0.3f, 0.3f, 0.3f, 0.2f);
                borderCol = new Color(0.4f, 0.4f, 0.4f, 0.5f);
                textColor = new Color(0.55f, 0.55f, 0.55f);
            }

            Widgets.DrawBoxSolid(pillRect, bgColor);
            Widgets.DrawBox(pillRect, 1, borderCol.ToSolidColorTexture());

            using (new TextBlock(GameFont.Small, TextAnchor.MiddleCenter, textColor)) {
                if (pawnHasTrait) {
                    Widgets.Label(pillRect, $"<b>{traitLabel}</b>");
                } else {
                    Widgets.Label(pillRect, traitLabel);
                }
            }

            string? tooltipText = null;
            TraitDegreeData? tipData = trait.degree.HasValue
                ? trait.def?.DataAtDegree(trait.degree.Value)
                : trait.def?.degreeDatas is { Count: > 0 }
                    ? trait.def.DataAtDegree(0)
                    : null;

            if (tipData?.description != null) {
                tooltipText = tipData.description
                    .Replace("{PAWN_nameDef}", pawn?.Name?.ToStringShort ?? "pawn")
                    .Replace("{PAWN_pronoun}", pawn?.gender == Gender.Female ? "she" : "he")
                    .Replace("{PAWN_possessive}", pawn?.gender == Gender.Female ? "her" : "his")
                    .Replace("{PAWN_objective}", pawn?.gender == Gender.Female ? "her" : "him");
            }

            if (pawn != null) {
                string traitStatus = pawnHasTrait
                    ? $"{pawn.LabelShortCap} has this trait!"
                    : $"{pawn.LabelShortCap} doesn't have this trait";
                tooltipText = tooltipText != null ? $"{traitStatus}\n\n{tooltipText}" : traitStatus;
            }

            if (tooltipText != null) {
                TooltipHandler.TipRegion(pillRect, tooltipText);
            }

            currentX += pillWidth + pillPadding;
        }
    }

    private enum IdealStatus {
        Achieved,
        Current,
        Future,
        Blocked,
    }
}