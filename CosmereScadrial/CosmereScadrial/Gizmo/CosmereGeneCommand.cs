using System.Collections.Generic;
using System.Linq;
using System.Text;
using CosmereFramework.Extension;
using CosmereFramework.Util;
using CosmereScadrial.Allomancy.Ability;
using CosmereScadrial.Def;
using CosmereScadrial.Gene;
using RimWorld;
using UnityEngine;
using Verse;

namespace CosmereScadrial.Gizmo;

public enum Status {
    Green,
    Red,
}

[StaticConstructorOnStartup]
public abstract class CosmereGeneCommand(
    Gene_Resource gene,
    List<IGeneResourceDrain> drainGenes,
    Color barColor,
    Color barHighlightColor
) : GeneGizmo_Resource(gene, drainGenes, barColor, barHighlightColor) {
    protected static readonly Vector2 Padding = new Vector2(2f, 4f);

    protected static readonly Texture2D BarTex = new Color(0.34f, 0.42f, 0.43f).ToSolidColorTexture();

    protected static readonly Texture2D BarHighlightTex =
        SolidColorMaterials.NewSolidColorTexture(new Color(0.43f, 0.54f, 0.55f));

    protected static readonly Texture2D EmptyBarTex = new Color(0.03f, 0.035f, 0.05f).ToSolidColorTexture();
    protected static readonly Texture2D DragBarTex = new Color(0.74f, 0.97f, 0.8f).ToSolidColorTexture();

    protected static readonly Texture2D StatusGreen = ContentFinder<Texture2D>.Get("UI/Widgets/StatusGreen");
    protected static readonly Texture2D StatusRed = ContentFinder<Texture2D>.Get("UI/Widgets/StatusGreen");

    private static readonly Texture2D PlusIcon = ContentFinder<Texture2D>.Get("UI/Buttons/Plus");
    private static readonly Texture2D MinusIcon = ContentFinder<Texture2D>.Get("UI/Buttons/Minus");

    protected Texture2D? barDragTex;
    protected Texture2D? barHighlightTex;
    protected Texture2D? barTex;

    // Things that get initialized
    protected Rect? bottomBarRect;

    protected Texture2D? cachedIcon;
    protected string? cachedTooltipDescription;

    protected string? cachedTooltipFooter;
    protected NamedArgument coloredMetal;
    protected NamedArgument coloredPawn;

    protected bool draggingBar;

    protected Rect? iconRect;
    protected bool initialized;
    protected Rect? labelRect;
    protected Rect? mainRect;
    protected Rect? outerRect;

    protected List<SubGizmo> subgizmos = [];
    internal float targetValuePct;
    protected Rect? topBarRect;

    protected override bool DraggingBar {
        get => draggingBar;
        set => draggingBar = value;
    }

    protected virtual float baseWidth => GetWidthForAbilityCount(2);
    protected virtual float abilityIconSize => Height / 2f;

    protected new Metalborn gene => (Metalborn)base.gene;
    internal MetallicArtsMetalDef metal => gene.metal;
    protected Pawn pawn => gene.pawn;

    protected virtual int IncrementDivisor => 5;
    protected virtual bool IsPlayerOwned => gene.pawn.IsColonistPlayerControlled || gene.pawn.IsPrisonerOfColony;
    protected override bool IsDraggable => IsPlayerOwned;
    protected override string BarLabel => $"{gene.Value / gene.Max:P1}";
    protected override int Increments => gene.MaxForDisplay / IncrementDivisor;
    public override bool Visible => pawn.Faction.IsPlayer;

    protected override string Title => metal.LabelCap;
    protected bool shrunk => gene.gizmoShrunk;

    protected virtual bool shouldShowStatus => false;

    protected virtual Status status => Status.Red;

    protected override float Width {
        get {
            if (shrunk) {
                return Height + Padding.x / 2;
            }

            return Mathf.Max(baseWidth, GetWidthForAbilityCount(gene.def.abilities?.Count ?? 0));
        }
    }

    protected float GetWidthForAbilityCount(int abilityCount) {
        return Height + Padding.x + (abilityIconSize + Padding.x) * abilityCount - Padding.x;
    }

    protected abstract Texture2D GetIcon();

    protected virtual void DrawTopBar(ref bool mouseOver) {
        if (!IsPlayerOwned) return;

        Rect abilityRect = new Rect(
            topBarRect!.Value.x,
            topBarRect.Value.y,
            topBarRect.Value.height,
            topBarRect.Value.height
        );
        foreach (SubGizmo subgizmo in subgizmos) {
            GizmoResult result = subgizmo.OnGUI(abilityRect);
            switch (result.State) {
                case GizmoState.Interacted:
                    subgizmo.ProcessInput(result.InteractEvent);
                    break;
                case GizmoState.Mouseover:
                    subgizmo.OnUpdate(abilityRect);
                    mouseOver = true;
                    break;
            }

            abilityRect.x += abilityIconSize + Padding.x;
        }
    }

    protected virtual void DrawBottomBar(ref bool mouseOver) {
        if (!IsDraggable) {
            Widgets.FillableBar(bottomBarRect!.Value, ValuePercent, barTex, EmptyBarTex, true);
            foreach (float barThreshold in GetBarThresholds()) {
                GUI.DrawTexture(
                    new Rect {
                        x = (float)(barRect.x + 3.0 + (barRect.width - 8.0) * barThreshold),
                        y = (float)(barRect.y + (double)barRect.height - 9.0),
                        width = 2f,
                        height = 6f,
                    },
                    ValuePercent < (double)barThreshold ? BaseContent.GreyTex : BaseContent.BlackTex
                );
            }
        } else {
            Widgets.DraggableBar(
                bottomBarRect!.Value,
                barTex,
                barHighlightTex,
                EmptyBarTex,
                barDragTex,
                ref draggingBar,
                ValuePercent,
                ref targetValuePct,
                GetBarThresholds(),
                Increments,
                DragRange.min,
                DragRange.max
            );
            targetValuePct = Mathf.Clamp(targetValuePct, DragRange.min, DragRange.max);
            Target = targetValuePct;
        }
    }

    protected override string GetTooltip() {
        StringBuilder tooltip = new StringBuilder();

        tooltip.AppendLine(GetTooltipHeader());
        tooltip.AppendLine(GetTooltipDescription());
        tooltip.AppendLine(GetTooltipFooter());

        return tooltip.ToString();
    }

    protected virtual string GetTooltipHeader() {
        return $"{gene.ResourceLabel.CapitalizeFirst().Colorize(ColoredText.TipSectionTitleColor)}: {BarLabel}";
    }

    protected virtual string GetTooltipFooter() {
        return cachedTooltipFooter ??= "";
    }

    protected virtual string GetTooltipDescription() {
        if (cachedTooltipDescription != null) return cachedTooltipDescription;

        if (!gene.def.resourceDescription.NullOrEmpty()) {
            return cachedTooltipDescription =
                "\n" + gene.def.resourceDescription.Formatted(coloredPawn, coloredMetal).Resolve();
        }

        return cachedTooltipDescription = "";
    }

    protected virtual void DrawIconBoxButtons(Rect rect, ref bool mouseOverElement) {
        const float statusRectSize = 14f;
        if (!shouldShowStatus) return;

        Rect statusRect = new Rect(
            rect.xMin,
            rect.yMin + 2,
            statusRectSize,
            statusRectSize
        );

        Widgets.DrawTextureFitted(statusRect, status == Status.Green ? StatusGreen : StatusRed, 1f);
    }

    protected virtual Rect DrawIconBox(ref bool mouseOverElement) {
        Rect rect = new Rect(mainRect!.Value.x, mainRect.Value.y, mainRect.Value.height, mainRect.Value.height);
        UIUtil.DrawIcon(
            rect,
            cachedIcon ??= GetIcon(),
            Verse.Command.BGTex,
            TexUI.GrayscaleGUI,
            offset: new Vector2(0, -4f),
            doBorder: false
        );

        if (!Title.NullOrEmpty()) {
            using (new TextBlock(GameFont.Tiny, TextAnchor.MiddleCenter)) {
                float textWidth = rect.width;
                float textHeight = Text.CalcHeight(Title, textWidth);
                labelRect = new Rect(rect.x, mainRect!.Value.yMax - textHeight, textWidth, textHeight);

                GUI.DrawTexture(labelRect.Value, TexUI.GrayTextBG);
                Widgets.Label(labelRect.Value, Title);
            }
        }

        DrawIconBoxButtons(rect, ref mouseOverElement);

        if (!mouseOverElement && Widgets.ButtonInvisible(rect)) {
            gene.gizmoShrunk = !shrunk;
        }

        return rect;
    }

    protected virtual void Initialize() {
        if (initialized) return;

        initialized = true;
        targetValuePct = Mathf.Clamp(Target, DragRange.min, DragRange.max);
        barTex = BarColor == new Color() ? BarTex : BarColor.ToSolidColorTexture();
        barHighlightTex = BarHighlightColor == new Color() ? BarHighlightTex : BarHighlightColor.ToSolidColorTexture();
        barDragTex = BarDragColor == new Color() ? DragBarTex : BarDragColor.ToSolidColorTexture();
        coloredPawn = pawn.NameShortColored.Named("PAWN");
        coloredMetal = metal.coloredLabel.Named("METAL");

        subgizmos.AddRange(
            gene.def.abilities?
                .OrderBy(x => x.uiOrder)
                .Select(x => pawn.abilities.GetAbility(x))
                .Cast<AbstractAbility>()
                .Where(x => x.GizmosVisible())
                .Select(x => new AbilitySubGizmo(this, gene, x))
                .ToList<SubGizmo>() ??
            []
        );
    }

    protected virtual Rect GetTopBarRect() {
        return new Rect(
            iconRect!.Value.x + iconRect.Value.width + Padding.x,
            iconRect.Value.y,
            mainRect!.Value.width - (iconRect.Value.width + Padding.x),
            abilityIconSize
        );
    }

    protected virtual Rect GetBottomBarRect() {
        return new Rect(
            topBarRect!.Value.x,
            topBarRect.Value.y + abilityIconSize + Padding.y,
            topBarRect.Value.width,
            Height - topBarRect.Value.height - Padding.y * 3
        );
    }

    public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms) {
        Initialize();
        bool mouseOver = false;

        using TextBlock textBlock = new TextBlock(GameFont.Tiny);
        outerRect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), Height);
        mainRect = outerRect.Value.ContractedBy(Padding.x * 2, Padding.y);

        Widgets.DrawWindowBackground(outerRect.Value);

        iconRect = DrawIconBox(ref mouseOver);

        if (!shrunk) {
            topBarRect = GetTopBarRect();
            bottomBarRect = GetBottomBarRect();

            DrawTopBar(ref mouseOver);
            DrawBottomBar(ref mouseOver);
        }

        if (Mouse.IsOver(mainRect.Value) && !mouseOver) {
            Widgets.DrawHighlight(mainRect.Value);
            TooltipHandler.TipRegion(mainRect.Value, GetTooltip, Gen.HashCombineInt(GetHashCode(), 8491284));
        }

        return new GizmoResult(mouseOver ? GizmoState.Mouseover : GizmoState.Clear);
    }
}