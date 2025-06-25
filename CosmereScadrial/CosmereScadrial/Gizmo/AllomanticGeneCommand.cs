using System.Collections.Generic;
using System.Linq;
using System.Text;
using CosmereFramework.Extension;
using CosmereFramework.Util;
using CosmereScadrial.Ability.Allomancy;
using CosmereScadrial.Comp.Thing;
using CosmereScadrial.Def;
using CosmereScadrial.Gene;
using RimWorld;
using UnityEngine;
using Verse;

namespace CosmereScadrial.Gizmo;

[StaticConstructorOnStartup]
public class AllomanticGeneCommand(
    Gene_Resource gene,
    List<IGeneResourceDrain> drainGenes,
    Color barColor,
    Color barHighlightColor) : GeneGizmo_Resource(gene, [], barColor, barHighlightColor) {
    private const float AbilityIconSize = Height / 2f;
    private const float BaseWidth = 185;
    private static readonly Vector2 Padding = new Vector2(2f, 4f);

    private static readonly Texture2D VialIcon =
        ContentFinder<Texture2D>.Get("Things/Item/AllomanticVial/AllomanticVial_c");

    private static readonly Texture2D BarTex = new Color(0.34f, 0.42f, 0.43f).ToSolidColorTexture();

    private static readonly Texture2D BarHighlightTex =
        SolidColorMaterials.NewSolidColorTexture(new Color(0.43f, 0.54f, 0.55f));

    private static readonly Texture2D EmptyBarTex = new Color(0.03f, 0.035f, 0.05f).ToSolidColorTexture();
    private static readonly Texture2D DragBarTex = new Color(0.74f, 0.97f, 0.8f).ToSolidColorTexture();
    private static bool DraggingBar;
    private Texture2D barDragTex;
    private Texture2D barHighlightTex;
    private Texture2D barTex;

    // Things to initialize
    private bool initialized;
    private IEnumerable<SubGizmo> subgizmos;
    private float targetValuePct;

    private new Allomancer gene => (Allomancer)base.gene;
    private MetallicArtsMetalDef metal => gene.metal;
    private Pawn pawn => gene.pawn;
    private MetalBurning burning => pawn.GetComp<MetalBurning>();

    protected override bool IsDraggable => gene.pawn.IsColonistPlayerControlled || gene.pawn.IsPrisonerOfColony;
    protected override string BarLabel => $"{gene.Value / gene.Max:P1}";
    protected override int Increments => gene.MaxForDisplay / 5;
    public override bool Visible => pawn.Faction.IsPlayer;

    protected override string Title => metal.LabelCap;

    protected override float Width => Mathf.Max(BaseWidth,
        Height + Padding.x + (AbilityIconSize + Padding.x) * gene.def.abilities.Count);

    protected override string GetTooltip() {
        float rate = burning.GetTotalBurnRate(metal) * GenTicks.TicksPerRealSecond;

        NamedArgument coloredPawn = pawn.NameShortColored.Named("PAWN");
        NamedArgument coloredCount =
            gene.RequestedVialStock.ToString().Colorize(ColoredText.FactionColor_Ally).Named("COUNT");

        StringBuilder tooltip = new StringBuilder();

        tooltip.AppendLine(
            $"{gene.ResourceLabel.CapitalizeFirst().Colorize(ColoredText.TipSectionTitleColor)}: {BarLabel}");
        if (burning.IsBurning(metal)) {
            tooltip.AppendLine("CS_BurnRate".Translate($"{rate:0.000}".Named("RATE"))
                .Colorize(ColoredText.FactionColor_Hostile));
        }

        if (IsDraggable) {
            tooltip.AppendLine("CS_CurrentVialStock".Translate(coloredCount));
            if (gene.targetValue <= 0.0) {
                tooltip.AppendLine("CS_NeverConsumeVial".Translate());
            } else {
                tooltip.AppendLine("CS_ConsumeVialBelow".Translate() + $": {gene.PostProcessValue(gene.targetValue)}%");
            }
        }

        if (!gene.def.resourceDescription.NullOrEmpty()) {
            tooltip.AppendLine("\n" + gene.def.resourceDescription.Formatted(coloredPawn).Resolve());
        }

        if (IsDraggable) tooltip.Append("\n" + "CS_ChangeVialStock".Translate());

        return tooltip.ToString();
    }

    private void DrawTopBar(Rect topBarRect, ref bool mouseOverElement) {
        if (!IsDraggable) return;

        Rect abilityRect = new Rect(topBarRect.x, topBarRect.y, topBarRect.height, topBarRect.height);
        foreach (SubGizmo subgizmo in subgizmos) {
            GizmoResult result = subgizmo.OnGUI(abilityRect);
            if (result.State == GizmoState.Interacted) {
                subgizmo.ProcessInput(result.InteractEvent);
            }

            if (result.State == GizmoState.Mouseover) {
                subgizmo.OnUpdate(abilityRect);
                mouseOverElement = true;
            }

            abilityRect.x += AbilityIconSize + Padding.x;
        }
    }

    private void DrawBottomBar(Rect bottomBarRect, ref bool mouseOverElement) {
        Texture thresholdColor =
            metal.Equals(MetallicArtsMetalDefOf.Atium) ? BaseContent.WhiteTex : BaseContent.BlackTex;
        if (!IsDraggable) {
            Widgets.FillableBar(bottomBarRect, ValuePercent, barTex, EmptyBarTex, true);
            foreach (float barThreshold in GetBarThresholds()) {
                GUI.DrawTexture(new Rect {
                        x = (float)(barRect.x + 3.0 + (barRect.width - 8.0) * barThreshold),
                        y = (float)(barRect.y + (double)barRect.height - 9.0),
                        width = 2f,
                        height = 6f,
                    },
                    ValuePercent < (double)barThreshold ? BaseContent.GreyTex : thresholdColor);
            }
        } else {
            Widgets.DraggableBar(bottomBarRect, barTex, barHighlightTex, EmptyBarTex, barDragTex,
                ref DraggingBar, ValuePercent, ref targetValuePct, GetBarThresholds(), Increments,
                DragRange.min, DragRange.max);
            targetValuePct = Mathf.Clamp(targetValuePct, DragRange.min, DragRange.max);
            Target = targetValuePct;
        }

        using (new TextBlock(GameFont.Small, TextAnchor.MiddleCenter)) Widgets.Label(bottomBarRect, BarLabel);

        TooltipHandler.TipRegionByKey(bottomBarRect, "CS_SetVialLevelTooltip", pawn.Named("PAWN"),
            metal.Named("METAL"));
        if (Mouse.IsOver(bottomBarRect)) {
            mouseOverElement = true;
        }
    }

    private Rect DrawIconBox(Rect iconBoxRect, ref bool mouseOverElement) {
        const float configureButtonSize = 20f;
        NamedArgument coloredPawn = pawn.NameShortColored.Named("PAWN");
        NamedArgument coloredMetal = metal.coloredLabel.Named("METAL");

        Rect iconRect = new Rect(iconBoxRect.x, iconBoxRect.y, iconBoxRect.height, iconBoxRect.height);
        UIUtil.DrawIcon(
            iconRect,
            metal.invertedIcon,
            Verse.Command.BGTex,
            TexUI.GrayscaleGUI,
            offset: new Vector2(0, -4f),
            doBorder: false
        );

        Rect configureRect = new Rect(iconRect.xMax - configureButtonSize, iconRect.yMin + 2,
            configureButtonSize, configureButtonSize);
        TooltipHandler.TipRegion(configureRect,
            (TaggedString)"CS_SetVialCountTooltip".Translate(coloredPawn, coloredMetal, pawn.Named("pawn")).Resolve());
        if (Mouse.IsOver(configureRect)) {
            mouseOverElement = true;
        }

        if (Widgets.ButtonImageFitted(configureRect, VialIcon)) {
            const int min = 0;
            const int max = 20;
            Dialog_Slider slider = new Dialog_Slider(
                amount => "CS_SetVialCountSlider"
                    .Translate(amount.Named("COUNT"), coloredPawn, coloredMetal, pawn.Named("pawn")).Resolve(),
                min,
                max,
                value => gene.RequestedVialStock = value,
                gene.RequestedVialStock
            );
            Find.WindowStack.Add(slider);
        }

        return iconRect;
    }

    private void Initialize() {
        if (initialized) {
            return;
        }

        subgizmos = gene.def.abilities
            .OrderBy(x => x.uiOrder)
            .Select(x => pawn.abilities.GetAbility(x))
            .Cast<AbstractAbility>()
            .Select(x => new AbilitySubGizmo(this, gene, x));

        initialized = true;
        targetValuePct = Mathf.Clamp(Target, DragRange.min, DragRange.max);
        barTex = BarColor == new Color() ? BarTex : BarColor.ToSolidColorTexture();
        barHighlightTex = BarHighlightColor == new Color() ? BarHighlightTex : BarHighlightColor.ToSolidColorTexture();
        barDragTex = BarDragColor == new Color() ? DragBarTex : BarDragColor.ToSolidColorTexture();
    }

    public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms) {
        Initialize();

        bool mouseOver = false;
        using TextBlock textBlock = new TextBlock(GameFont.Tiny);
        Rect rect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), Height);
        Widgets.DrawWindowBackground(rect);
        rect = rect.ContractedBy(Padding.x * 2, Padding.y);

        Rect iconRect = DrawIconBox(rect, ref mouseOver);
        Rect topBarRect = new Rect(iconRect.x + iconRect.width + Padding.x, iconRect.y,
            rect.width - (rect.height + Padding.y), AbilityIconSize);
        DrawTopBar(topBarRect, ref mouseOver);
        Rect bottomBarRect = new Rect(topBarRect.x, topBarRect.y + AbilityIconSize + Padding.y / 2, topBarRect.width,
            Height - AbilityIconSize - Padding.y * 3);
        DrawBottomBar(bottomBarRect, ref mouseOver);

        if (!Title.NullOrEmpty()) {
            using (new TextBlock(GameFont.Tiny)) {
                float textWidth = iconRect.width;
                float textHeight = Text.CalcHeight(Title, textWidth);
                Rect labelRect = new Rect(iconRect.x, rect.yMax - textHeight, textWidth, textHeight);
                GUI.DrawTexture(labelRect, TexUI.GrayTextBG);
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(labelRect, Title);
            }
        }

        // @todo Maybe implement float menu stuff?

        if (Mouse.IsOver(rect) && !mouseOver) {
            Widgets.DrawHighlight(rect);
            TooltipHandler.TipRegion(rect, GetTooltip, Gen.HashCombineInt(GetHashCode(), 8491284));
        }

        return new GizmoResult(mouseOver ? GizmoState.Mouseover : GizmoState.Clear);
    }
}