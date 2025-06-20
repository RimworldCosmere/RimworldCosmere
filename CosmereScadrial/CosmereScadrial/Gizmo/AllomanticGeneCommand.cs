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
    private const float ABILITY_ICON_SIZE = 24;
    private const float ABILITY_ICON_PADDING = 4;
    public static readonly Texture2D CheckOff = ContentFinder<Texture2D>.Get("UI/Widgets/CheckOff");
    public static readonly Texture2D BGTexOff = ContentFinder<Texture2D>.Get("UI/Widgets/AbilityOff");
    public static readonly Texture2D BGTexBurning = ContentFinder<Texture2D>.Get("UI/Widgets/AbilityBurning");
    public static readonly Texture2D BGTexFlaring = ContentFinder<Texture2D>.Get("UI/Widgets/AbilityFlaring");
    public static readonly Texture2D BGTexOffDisabled = BGTexOff.Overlay(CheckOff);

    protected MetallicArtsMetalDef metal => ((Allomancer)gene).metal;
    protected Pawn pawn => gene.pawn;
    protected MetalBurning burning => pawn.GetComp<MetalBurning>();

    protected override bool IsDraggable => gene.pawn.IsColonistPlayerControlled || gene.pawn.IsPrisonerOfColony;
    protected override string BarLabel => $"{gene.Value / gene.Max:P1}";
    protected override int Increments => gene.MaxForDisplay / 5;
    public override bool Visible => pawn.Faction.IsPlayer;

    protected override string Title => metal.LabelCap;

    private float TitleWidth => Text.CalcSize(Title).x;

    protected override float Width => Mathf.Max(165f,
        TitleWidth + 30f + (ABILITY_ICON_SIZE + ABILITY_ICON_PADDING) * (gene.def.abilities.Count + 1));

    protected override string GetTooltip() {
        float rate = burning.GetTotalBurnRate(metal) * GenTicks.TicksPerRealSecond;

        string tooltip =
            $"{gene.ResourceLabel.CapitalizeFirst().Colorize(ColoredText.TipSectionTitleColor)}: {BarLabel}\n";
        if (burning.IsBurning(metal)) {
            tooltip += "BurnRate".Translate().Colorize(ColoredText.FactionColor_Hostile) + $": {rate:0.000}/sec\n";
        }

        if (IsDraggable) {
            if (gene.targetValue <= 0.0) {
                tooltip += "NeverConsumeVial".Translate(metal.LabelCap).ToString();
            } else {
                tooltip += (string)("ConsumeVialBelow".Translate(metal.LabelCap) + ": ") +
                           gene.PostProcessValue(gene.targetValue) + '%';
            }
        }

        if (!gene.def.resourceDescription.NullOrEmpty()) {
            tooltip += $"\n\n{gene.def.resourceDescription.Formatted(gene.pawn.Named("PAWN"))}";
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
            null => BGTexOff,
            BurningStatus.Off => BGTexOff,
            BurningStatus.Passive => BGTexBurning,
            BurningStatus.Burning => BGTexBurning,
            BurningStatus.Flaring => BGTexFlaring,
            BurningStatus.Duralumin => BGTexFlaring,
            _ => throw new ArgumentOutOfRangeException(),
        };
        GUI.color = parms.lowLight ? Verse.Command.LowLightBgColor : color;
        Texture2D? icon = disabled ? BGTexOffDisabled : ability.def.uiIcon;
        DrawIcon(rect, icon, background, grayscaleGui, parms.lowLight ? Color.gray.ToTransparent(.6f) : null);

        if (Widgets.ButtonInvisible(rect)) {
            isClicked = true;
        }

        if (isMouseOver) {
            TipSignal desc = ability.Tooltip;

            desc.text += "\n\n" + "PressToFlare".Translate(metal.LabelCap).Colorize(ColoredText.GeneColor);

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

        if (ability.def.targetRequired) {
            Find.DesignatorManager.Deselect();
            if (!ability.def.targetWorldCell) {
                Find.Targeter.BeginTargeting(ability.verb);
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
            ability.QueueCastingJob(
                ability.pawn,
                LocalTargetInfo.Invalid,
                ev.control
            );
        }
    }

    protected override void DrawHeader(Rect headerRect, ref bool mouseOverElement) {
        if (IsDraggable) {
            if (gene is Allomancer metalborn) {
                headerRect.xMax -= (ABILITY_ICON_SIZE + ABILITY_ICON_PADDING) * metalborn.def.abilities.Count;
                int i = 0;
                metalborn.def.abilities.SortBy(x => x.uiOrder);
                foreach (AbilityDef abilityDef in metalborn.def.abilities) {
                    if (pawn.abilities.GetAbility(abilityDef) is not AbstractAbility ability) continue;

                    Rect rect = new Rect(
                        headerRect.xMax + i * (ABILITY_ICON_SIZE + ABILITY_ICON_PADDING),
                        headerRect.y,
                        ABILITY_ICON_SIZE,
                        ABILITY_ICON_SIZE
                    );


                    GizmoResult result = AbilityOnGUI(rect, ability);

                    switch (result.State) {
                        case GizmoState.Interacted:
                            ProcessInput(ability, result.InteractEvent);
                            break;
                        case GizmoState.Mouseover:
                            TooltipHandler.ClearTooltipsFrom(headerRect);
                            if (ability.verb is Verb_CastAbility verb) {
                                verb.verbProps.DrawRadiusRing(verb.caster.Position, verb);
                            }

                            ability.OnGizmoUpdate();
                            Widgets.DrawHighlight(rect);
                            mouseOverElement = true;
                            break;
                    }

                    i++;
                }
            }
        }

        if (metal.invertedIcon) {
            Rect icon = new Rect(headerRect.xMin, headerRect.yMin, ABILITY_ICON_SIZE, ABILITY_ICON_SIZE);

            DrawIcon(icon, metal.invertedIcon, scale: 1.5f);
            headerRect.xMin += ABILITY_ICON_SIZE + ABILITY_ICON_PADDING;
        }

        base.DrawHeader(headerRect, ref mouseOverElement);
    }

    private void DrawIcon(Rect rect, Texture2D icon, Texture? background = null, Material? material = null,
        Color? color = null, float? scale = null, Vector2? offset = null) {
        GUI.color = color ?? Color.white;
        if (background != null) GenUI.DrawTextureWithMaterial(rect, background, material);

        Rect iconRect = offset == null
            ? rect
            : new Rect(rect.x + offset.Value.x, rect.y + offset.Value.y, rect.width, rect.height);
        Widgets.DrawTextureFitted(iconRect, icon, scale ?? 1f);
        GUI.color = Color.white;
    }
}