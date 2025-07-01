using System;
using System.Text;
using CosmereFramework.Extension;
using CosmereFramework.Util;
using CosmereScadrial.Allomancy.Ability;
using CosmereScadrial.Gene;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace CosmereScadrial.Gizmo;

[StaticConstructorOnStartup]
public class AbilitySubGizmo(Verse.Gizmo parent, Metalborn gene, AbstractAbility ability)
    : SubGizmo(parent) {
    private static readonly Texture2D CheckOff = ContentFinder<Texture2D>.Get("UI/Widgets/CheckOff");
    private static readonly Texture2D BgTexOff = GenColor.FromHex("000000").ToSolidColorTexture();
    private static readonly Texture2D BgTexBurning = ContentFinder<Texture2D>.Get("UI/Widgets/AbilityBurning");
    private static readonly Texture2D BgTexFlaring = ContentFinder<Texture2D>.Get("UI/Widgets/AbilityFlaring");
    private static readonly Texture2D Border = ColorLibrary.Grey.ToSolidColorTexture();
    private static readonly Texture2D AutoBurnBorder = ColorLibrary.LightBlue.ToSolidColorTexture();

    private AcceptanceReport _cachedReport;
    private bool disabled => !_cachedReport.Accepted;
    private string? disabledReason => _cachedReport.Reason;

    private Texture2D icon => disabled ? ability.def.disabledIcon : ability.def.uiIcon;

    private Texture2D background => ability.status switch {
        null => BgTexOff,
        BurningStatus.Off => BgTexOff,
        BurningStatus.Passive => BgTexBurning,
        BurningStatus.Burning => BgTexBurning,
        BurningStatus.Flaring => BgTexFlaring,
        BurningStatus.Duralumin => BgTexFlaring,
        _ => throw new ArgumentOutOfRangeException(),
    };

    private AcceptanceReport GizmoEnabled() {
        return ability.GizmoEnabled();
    }

    public override GizmoResult OnGUI(Rect rect) {
        _cachedReport = GizmoEnabled();

        GizmoRenderParms parms = new GizmoRenderParms { shrunk = true, lowLight = false, highLight = false };
        bool isMouseOver = false;
        bool isClicked = false;
        if (Mouse.IsOver(rect)) {
            isMouseOver = true;
        }

        MouseoverSounds.DoRegion(rect, SoundDefOf.Mouseover_Command);
        if (parms.highLight && !disabled) {
            Widgets.DrawStrongHighlight(rect.ExpandedBy(4f));
        }

        UIUtil.DrawIcon(
            rect,
            icon,
            background,
            !disabled ? null : TexUI.GrayscaleGUI,
            borderTexture: ability.willBurnWhileDowned ? AutoBurnBorder : Border
        );

        if (Widgets.ButtonInvisible(rect)) {
            isClicked = true;
        }

        if (isMouseOver) {
            StringBuilder desc = new StringBuilder(ability.Tooltip);

            TaggedString flareOrDeflare =
                (ability.status == BurningStatus.Flaring ? "CS_Deflare" : "CS_Flare").Translate();
            desc.AppendLine("\n");
            desc.AppendLine(
                "CS_PressToFlare"
                    .Translate(flareOrDeflare.Named("FLARE"), gene.metal.label.Named("METAL"))
                    .Colorize(ColoredText.GeneColor)
            );

            if (ability.def.canBurnWhileDowned) {
                desc.AppendLine("CS_ToggleAutomaticBurning".Translate()
                    .Colorize(ColoredText.GeneColor));
            }

            if (!disabledReason.NullOrEmpty()) {
                desc.AppendLine("\n");
                desc.AppendLine(
                    ("DisabledCommand".Translate() + ": " + disabledReason).Colorize(ColorLibrary.RedReadable));
            }

            TooltipHandler.TipRegion(rect, (TipSignal)desc.ToString());
        }

        if (!isClicked) {
            return isMouseOver ? new GizmoResult(GizmoState.Mouseover, null) : new GizmoResult(GizmoState.Clear, null);
        }


        if (!disabled || Event.current.shift) {
            return Event.current.button == 1
                ? new GizmoResult(GizmoState.OpenedFloatMenu, Event.current)
                : new GizmoResult(GizmoState.Interacted, Event.current);
        }

        Messages.Message((string)("DisabledCommand".Translate() + ": " + disabledReason),
            MessageTypeDefOf.RejectInput, false);
        return new GizmoResult(GizmoState.Mouseover, null);
    }

    public override void ProcessInput(Event ev) {
        _cachedReport = GizmoEnabled();
        SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();

        if (ev.shift) {
            ability.willBurnWhileDowned = !ability.willBurnWhileDowned;
            return;
        }

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