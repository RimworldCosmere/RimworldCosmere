using System;
using System.Text;
using Cosmere.Core.Gene;
using Cosmere.Lightweave.Adapter;
using Cosmere.Lightweave.Runtime;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.Core.Gizmo;

public enum Status {
    Green,
    Red,
}

[StaticConstructorOnStartup]
public abstract class CosmereGeneCommand<TSubGizmo, TGene>(
    Gene_Resource gene,
    List<IGeneResourceDrain> drainGenes,
    Color barColor,
    Color barHighlightColor
) : GeneGizmo_Resource(gene, drainGenes, barColor, barHighlightColor)
    where TSubGizmo : SubGizmo, new() where TGene : Invested {
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
    protected NamedArgument coloredPawn;

    protected bool draggingBar;

    protected Rect? iconRect;
    protected bool initialized;
    protected Rect? labelRect;
    protected Rect? mainRect;
    protected Rect? outerRect;

    protected List<TSubGizmo> subgizmos = [];
    public float targetValuePercent;
    protected Rect? topBarRect;

    protected virtual bool useResourceLabelForTooltip => true;
    protected virtual bool useResourceLabelForTitle => true;

    protected override string Title {
        get {
            string title = (useResourceLabelForTitle ? gene.ResourceLabel : gene.Label).CapitalizeFirst();
            if (Find.Selector.SelectedPawns.Count != 1) {
                title = $"{title} ({gene.pawn.LabelShort})";
            }

            return title;
        }
    }

    protected override bool DraggingBar {
        get => draggingBar;
        set => draggingBar = value;
    }

    protected virtual float baseWidth => GetWidthForAbilityCount(2);
    protected virtual float abilityIconSize => Height / 2f;

    protected new TGene gene => (TGene)base.gene;
    protected Pawn pawn => gene.pawn;

    protected virtual int IncrementDivisor => 5;
    protected virtual bool IsPlayerOwned => gene.pawn.IsColonistPlayerControlled || gene.pawn.IsPrisonerOfColony;
    protected override bool IsDraggable => IsPlayerOwned;
    protected override string BarLabel => $"{gene.Value / gene.Max:P1}";
    protected override int Increments => gene.MaxForDisplay / IncrementDivisor;
    public override bool Visible => pawn.Faction.IsPlayer;

    protected bool shrunk => gene.gizmoShrunk;

    protected virtual bool shouldShowStatus => false;

    protected virtual Status status => Status.Red;

    protected override float Width {
        get {
            if (shrunk) {
                return Height + Padding.x / 2;
            }

            return Mathf.Max(baseWidth, GetWidthForAbilityCount(gene.abilities?.Count ?? 0));
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

    public void ClearCache() {
        initialized = false;
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
                ref targetValuePercent,
                GetBarThresholds(),
                Increments,
                DragRange.min,
                DragRange.max
            );
            targetValuePercent = Mathf.Clamp(targetValuePercent, DragRange.min, DragRange.max);
            Target = targetValuePercent;
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
        string? label = useResourceLabelForTooltip ? gene.ResourceLabel : gene.Label;

        return $"{label.CapitalizeFirst().Colorize(ColoredText.TipSectionTitleColor)}: {BarLabel}";
    }

    protected virtual string GetTooltipFooter() {
        return cachedTooltipFooter ??=
            "\n" +
            "Click icon to toggle visibility".Colorize(ColorLibrary.Grey) +
            "\n" +
            "Drag the bar to set your Investiture reserve limit".Colorize(ColorLibrary.Grey);
    }

    protected virtual string GetTooltipDescription() {
        if (cachedTooltipDescription != null) return cachedTooltipDescription;

        if (!gene.def.resourceDescription.NullOrEmpty()) {
            return cachedTooltipDescription =
                "\n" + gene.def.resourceDescription.Formatted(coloredPawn).Resolve();
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
        Util.UI.DrawIcon(
            rect,
            cachedIcon ??= GetIcon(),
            Command.BGTex,
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

    protected abstract IEnumerable<TSubGizmo> GetSubGizmos();

    protected virtual void Initialize() {
        if (initialized) return;

        initialized = true;
        targetValuePercent = Mathf.Clamp(Target, DragRange.min, DragRange.max);
        barTex = BarColor == new Color() ? BarTex : BarColor.ToSolidColorTexture();
        barHighlightTex = BarHighlightColor == new Color() ? BarHighlightTex : BarHighlightColor.ToSolidColorTexture();
        barDragTex = BarDragColor == new Color() ? DragBarTex : BarDragColor.ToSolidColorTexture();
        coloredPawn = pawn.NameShortColored.Named("PAWN");

        subgizmos.AddRange(GetSubGizmos());
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
        Rect rect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), Height);
        Guid id = AdapterStoreRegistry.GetOrCreate(pawn.thingIDNumber, AdapterKind.Gizmo, gene.def.shortHash);
        bool mouseOver = false;
        LightweaveRoot.Render(
            rect,
            id,
            () => {
                LightweaveNode node = new LightweaveNode { DebugName = "CosmereGeneCommand" };
                node.Paint = (paintRect, _) => mouseOver = PaintGene(paintRect);
                return node;
            }
        );
        return new GizmoResult(mouseOver ? GizmoState.Mouseover : GizmoState.Clear);
    }

    private bool PaintGene(Rect outerRectIn) {
        Initialize();
        bool mouseOver = false;

        using TextBlock textBlock = new TextBlock(GameFont.Tiny);
        outerRect = outerRectIn;
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

        return mouseOver;
    }
}