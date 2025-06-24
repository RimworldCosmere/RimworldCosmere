using System;
using CosmereFramework.Extensions;
using CosmereFramework.Utils;
using CosmereScadrial.Abilities.Allomancy;
using CosmereScadrial.Genes;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace CosmereScadrial.Gizmo;

public class AbilitySubGizmo(Verse.Gizmo parent, Metalborn gene, AbstractAbility ability) : SubGizmo(parent) {
    private static readonly Texture2D CheckOff = ContentFinder<Texture2D>.Get("UI/Widgets/CheckOff");
    private static readonly Texture2D BgTexOff = ContentFinder<Texture2D>.Get("UI/Widgets/AbilityOff");
    private static readonly Texture2D BgTexBurning = ContentFinder<Texture2D>.Get("UI/Widgets/AbilityBurning");
    private static readonly Texture2D BgTexOffDisabled = BgTexOff.Overlay(CheckOff);
    private static readonly Texture2D BgTexFlaring = ContentFinder<Texture2D>.Get("UI/Widgets/AbilityFlaring");

    public readonly AbstractAbility ability = ability;
    public readonly Metalborn gene = gene;

    public override GizmoResult OnGUI(Rect rect) {
        //GeneGizmo_ResourceHemogen
        GizmoRenderParms parms = new GizmoRenderParms { shrunk = true, lowLight = false, highLight = false };
        Color color = Color.white;
        bool isMouseOver = false;
        bool isClicked = false;
        AcceptanceReport disabled = ability.GizmoDisabled();
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
        UIUtil.DrawIcon(rect, icon, background, grayscaleGui, parms.lowLight ? Color.gray.ToTransparent(.6f) : null);

        if (Widgets.ButtonInvisible(rect)) {
            isClicked = true;
        }

        if (isMouseOver) {
            TipSignal desc = ability.Tooltip;

            TaggedString flareOrDeflare =
                (ability.status == BurningStatus.Flaring ? "CS_Deflare" : "CS_Flare").Translate();
            desc.text += "\n\n" + "CS_PressToFlare"
                .Translate(flareOrDeflare.Named("FLARE"), gene.metal.label.Named("METAL"))
                .Colorize(ColoredText.GeneColor);

            if (disabled && !disabled.Reason.NullOrEmpty()) {
                desc.text +=
                    ("\n\n" + "DisabledCommand".Translate() + ": " + disabled.Reason)
                    .Colorize(ColorLibrary.RedReadable);
            }

            TooltipHandler.TipRegion(rect, desc);
        }

        if (!isClicked) {
            return isMouseOver ? new GizmoResult(GizmoState.Mouseover, null) : new GizmoResult(GizmoState.Clear, null);
        }


        if (!disabled || disabled.Reason.NullOrEmpty()) {
            return Event.current.button == 1
                ? new GizmoResult(GizmoState.OpenedFloatMenu, Event.current)
                : new GizmoResult(GizmoState.Interacted, Event.current);
        }

        Messages.Message((string)("DisabledCommand".Translate() + ": " + disabled.Reason),
            MessageTypeDefOf.RejectInput, false);
        return new GizmoResult(GizmoState.Mouseover, null);
    }

    public override void ProcessInput(Event ev) {
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
                CameraJumper.TryJump(CameraJumper.GetWorldTarget(ability.pawn));
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
}