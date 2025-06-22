using System;
using System.Collections.Generic;
using CosmereScadrial.Abilities.Allomancy;
using CosmereScadrial.Comps.Things;
using CosmereScadrial.Defs;
using CosmereScadrial.Genes;
using RimWorld;
using RimWorld.Planet;
using TextureTools;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace CosmereScadrial.Gizmo;

[StaticConstructorOnStartup]
public class AllomanticGeneCommand(
    Gene_Resource gene,
    List<IGeneResourceDrain> drainGenes,
    Color barColor,
    Color barHighlightColor) : GeneGizmo_Resource(gene, [], barColor, barHighlightColor) {
    private const float AbilityIconSize = Height / 2f;
    private const float BaseWidth = 185;
    private const float IconBorderSize = 1;
    private static readonly Vector2 Padding = new Vector2(2f, 4f);

    public static readonly Texture2D CheckOff = ContentFinder<Texture2D>.Get("UI/Widgets/CheckOff");

    public static readonly Texture2D
        BgTexOff = ContentFinder<Texture2D>.Get(
            "UI/Widgets/AbilityOff"); //SolidColorMaterials.NewSolidColorTexture(new Color(0f, 0f, 0f, 0f));

    public static readonly Texture2D BgTexBurning = ContentFinder<Texture2D>.Get("UI/Widgets/AbilityBurning");
    public static readonly Texture2D BgTexFlaring = ContentFinder<Texture2D>.Get("UI/Widgets/AbilityFlaring");
    private static readonly Texture2D BarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.34f, 0.42f, 0.43f));

    private static readonly Texture2D EmptyBarTex =
        SolidColorMaterials.NewSolidColorTexture(new Color(0.03f, 0.035f, 0.05f));

    private static readonly Texture2D DragBarTex =
        SolidColorMaterials.NewSolidColorTexture(new Color(0.74f, 0.97f, 0.8f));

    public static readonly Texture2D BgTexOffDisabled =
        ContentFinder<Texture2D>.Get("UI/Widgets/AbilityOff").Overlay(CheckOff);

    private static bool DraggingBar;
    private Texture2D barDragTex;
    private Texture2D barHighlightTex;
    private Texture2D barTex;

    private bool initialized;
    private float targetValuePct;

    protected MetallicArtsMetalDef metal => ((Allomancer)gene).metal;
    protected Pawn pawn => gene.pawn;
    protected MetalBurning burning => pawn.GetComp<MetalBurning>();

    protected override bool IsDraggable => gene.pawn.IsColonistPlayerControlled || gene.pawn.IsPrisonerOfColony;
    protected override string BarLabel => $"{gene.Value / gene.Max:P1}";
    protected override int Increments => gene.MaxForDisplay / 5;
    public override bool Visible => pawn.Faction.IsPlayer;

    protected override string Title => metal.LabelCap;

    protected override float Width => Mathf.Max(BaseWidth,
        Height + Padding.x + (AbilityIconSize + Padding.x) * gene.def.abilities.Count);

    protected override string GetTooltip() {
        float rate = burning.GetTotalBurnRate(metal) * GenTicks.TicksPerRealSecond;

        string tooltip =
            $"{gene.ResourceLabel.CapitalizeFirst().Colorize(ColoredText.TipSectionTitleColor)}: {BarLabel}\n";
        if (burning.IsBurning(metal)) {
            tooltip +=
                "CS_BurnRate".Translate($"{rate:0.000}".Named("RATE")).Colorize(ColoredText.FactionColor_Hostile) +
                "\n";
        }

        if (IsDraggable) {
            if (gene.targetValue <= 0.0) {
                tooltip += "CS_NeverConsumeVial"
                    .Translate(metal.label.Colorize(ColoredText.DateTimeColor).Named("METAL")).Resolve();
            } else {
                tooltip += ("CS_ConsumeVialBelow".Translate(metal.label.Colorize(ColoredText.DateTimeColor)
                               .Named("METAL")) + ": ").Resolve() +
                           gene.PostProcessValue(gene.targetValue) + '%';
            }
        }

        if (!gene.def.resourceDescription.NullOrEmpty()) {
            string resourceDescription =
                gene.def.resourceDescription.Formatted(pawn.NameShortColored.Named("PAWN")).Resolve();
            tooltip += $"\n\n{resourceDescription}";
        }

        return tooltip;
    }

    protected virtual GizmoResult AbilityOnGUI(Rect rect, AbstractAbility ability) {
        //GeneGizmo_ResourceHemogen
        GizmoRenderParms parms = new GizmoRenderParms { shrunk = true, lowLight = false, highLight = false };
        Color color = Color.white;
        bool isMouseOver = false;
        bool isClicked = false;
        bool disabled = ability.GizmoDisabled(out string disabledReason);
        if (Mouse.IsOver(rect)) {
            isMouseOver = true;
            if (!disabled) {
                color = GenUI.MouseoverColor;
            }
        }

        MouseoverSounds.DoRegion(rect, SoundDefOf.Mouseover_Command);
        if (parms.highLight && !disabled) {
            Widgets.DrawStrongHighlight(rect.ExpandedBy(4f));
        }

        if (disabled) {
            parms.lowLight = true;
        }

        Material? grayscaleGui = parms.lowLight ? TexUI.GrayscaleGUI : null;
        Texture2D background = ability.status switch {
            null => BgTexOff,
            BurningStatus.Off => BgTexOff,
            BurningStatus.Passive => BgTexBurning,
            BurningStatus.Burning => BgTexBurning,
            BurningStatus.Flaring => BgTexFlaring,
            BurningStatus.Duralumin => BgTexFlaring,
            _ => throw new ArgumentOutOfRangeException(),
        };
        GUI.color = parms.lowLight ? Verse.Command.LowLightBgColor : color;
        Texture2D? icon = disabled ? BgTexOffDisabled : ability.def.uiIcon;
        DrawIcon(rect, icon, background, grayscaleGui, parms.lowLight ? Color.gray.ToTransparent(.6f) : null);

        if (Widgets.ButtonInvisible(rect)) {
            isClicked = true;
        }

        if (isMouseOver) {
            TipSignal desc = ability.Tooltip;

            TaggedString flareOrDeflare =
                (ability.status == BurningStatus.Flaring ? "CS_Deflare" : "CS_Flare").Translate();
            desc.text += "\n\n" + "CS_PressToFlare"
                .Translate(flareOrDeflare.Named("FLARE"), metal.label.Named("METAL"))
                .Colorize(ColoredText.GeneColor);

            if (disabled && !disabledReason.NullOrEmpty()) {
                desc.text +=
                    ("\n\n" + "DisabledCommand".Translate() + ": " + disabledReason).Colorize(ColorLibrary.RedReadable);
            }

            TooltipHandler.TipRegion(rect, desc);
        }

        if (!isClicked) {
            return isMouseOver ? new GizmoResult(GizmoState.Mouseover, null) : new GizmoResult(GizmoState.Clear, null);
        }


        if (!disabled || disabledReason.NullOrEmpty()) {
            return Event.current.button == 1
                ? new GizmoResult(GizmoState.OpenedFloatMenu, Event.current)
                : new GizmoResult(GizmoState.Interacted, Event.current);
        }

        Messages.Message((string)("DisabledCommand".Translate() + ": " + disabledReason),
            MessageTypeDefOf.RejectInput, false);
        return new GizmoResult(GizmoState.Mouseover, null);
    }

    protected virtual void ProcessInput(AbstractAbility ability, Event ev) {
        SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();

        if (ability.atLeastPassive) {
            bool isFlaring = ability.status == BurningStatus.Flaring;
            bool isBurningOrPassive = ability.status is BurningStatus.Burning or BurningStatus.Passive;

            if (ev.control) {
                if (isBurningOrPassive) {
                    ability.SetNextStatus(BurningStatus.Flaring);
                } else if (isFlaring) {
                    ability.SetNextStatus(BurningStatus.Burning);
                }
            } else {
                ability.SetNextStatus(BurningStatus.Off);
            }

            ability.UpdateStatus(ability.nextStatus!.Value);

            return;
        }

        if (ability.def.targetRequired) {
            Find.DesignatorManager.Deselect();
            ability.SetNextStatus(ev.control ? BurningStatus.Flaring : BurningStatus.Burning, true);
            if (!ability.def.targetWorldCell) {
                float originalRange = ability.verb.verbProps.range;
                ability.verb.verbProps.range = originalRange * ability.GetStrength(ability.nextStatus);
                Find.Targeter.BeginTargeting(ability.verb,
                    actionWhenFinished: () => ability.verb.verbProps.range = originalRange);
            } else {
                CameraJumper.TryJump(CameraJumper.GetWorldTarget((GlobalTargetInfo)(Thing)ability.pawn));
                Find.WorldTargeter.BeginTargeting(t => {
                        if (!ability.ValidateGlobalTarget(t)) {
                            return false;
                        }

                        ability.QueueCastingJob(t, ev.control);
                        return true;
                    }, true, ability.def.uiIcon, !ability.pawn.IsCaravanMember(),
                    extraLabelGetter: ability.WorldMapExtraLabel, canSelectTarget: ability.ValidateGlobalTarget);
            }
        } else {
            ability.QueueCastingJob(ability.pawn, LocalTargetInfo.Invalid, ev.control);
        }
    }

    private void DrawTopBar(Rect topBarRect, ref bool mouseOverElement) {
        if (!IsDraggable) return;
        if (gene is not Allomancer metalborn) return;

        metalborn.def.abilities.SortBy(x => x.uiOrder);
        Rect abilityRect = new Rect(topBarRect.x, topBarRect.y, topBarRect.height, topBarRect.height);
        foreach (AbilityDef abilityDef in metalborn.def.abilities) {
            if (pawn.abilities.GetAbility(abilityDef) is not AbstractAbility ability) continue;

            GizmoResult result = AbilityOnGUI(abilityRect, ability);

            switch (result.State) {
                case GizmoState.Interacted:
                    ProcessInput(ability, result.InteractEvent);
                    break;
                case GizmoState.Mouseover:
                    if (ability.verb is Verb_CastAbility verb) {
                        // @TODO This doesn't work very well. Gotta figure that out. Its like its not happening every tick?
                        // verb.verbProps.DrawRadiusRing(verb.caster.Position, verb);
                    }

                    ability.OnGizmoUpdate();
                    Widgets.DrawHighlight(abilityRect);
                    mouseOverElement = true;
                    break;
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

    private void DrawIcon(Rect rect, Texture2D icon, Texture? background = null, Material? material = null,
        Color? color = null, float? scale = null, Vector2? offset = null, bool doBorder = true) {
        if (doBorder) {
            GUI.DrawTexture(rect, BaseContent.BlackTex);
            rect = rect.ContractedBy(IconBorderSize);
        }

        GUI.color = color ?? Color.white;
        if (background != null) GenUI.DrawTextureWithMaterial(rect, background, material);

        Rect iconRect = offset == null
            ? rect
            : new Rect(rect.x + offset.Value.x, rect.y + offset.Value.y, rect.width, rect.height);
        Widgets.DrawTextureFitted(iconRect, icon, scale ?? 1f);
        GUI.color = Color.white;
    }


    private void Initialize() {
        if (initialized) {
            return;
        }

        initialized = true;
        targetValuePct = Mathf.Clamp(Target, DragRange.min, DragRange.max);
        barTex = BarColor == new Color() ? BarTex : SolidColorMaterials.NewSolidColorTexture(BarColor);
        barHighlightTex = SolidColorMaterials.NewSolidColorTexture(BarColor.SaturationChanged(50f));
        barDragTex = BarDragColor == new Color() ? DragBarTex : SolidColorMaterials.NewSolidColorTexture(BarDragColor);
    }

    public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms) {
        Initialize();

        bool mouseOver = false;
        using TextBlock textBlock = new TextBlock(GameFont.Tiny);
        Rect rect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), Height);
        Widgets.DrawWindowBackground(rect);
        rect = rect.ContractedBy(Padding.x * 2, Padding.y);

        Rect iconRect = new Rect(rect.x, rect.y, rect.height, rect.height);
        DrawIcon(iconRect, metal.invertedIcon, Verse.Command.BGTex, TexUI.GrayscaleGUI, offset: new Vector2(0, -4f),
            doBorder: false);
        Text.Font = GameFont.Small;
        float barWidth = rect.width - (rect.height + Padding.y);
        Rect topBarRect = new Rect(iconRect.x + iconRect.width + Padding.x, iconRect.y, barWidth, AbilityIconSize);
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

        if (Mouse.IsOver(rect) && !mouseOver) {
            Widgets.DrawHighlight(rect);
            TooltipHandler.TipRegion(rect, GetTooltip, Gen.HashCombineInt(GetHashCode(), 8491284));
        }

        return new GizmoResult(mouseOver ? GizmoState.Mouseover : GizmoState.Clear, null);
    }

    public override void GizmoUpdateOnMouseover() {
        base.GizmoUpdateOnMouseover();
    }
}