using System;
using System.Text;
using Cosmere.Core.Gizmo;
using Cosmere.Framework.Util;
using Cosmere.Scadrial.Allomancy.Ability;
using Cosmere.Scadrial.Gene;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Cosmere.Scadrial.Gizmo;

[StaticConstructorOnStartup]
public class AbilitySubGizmo : SubGizmo {
    private static readonly Texture2D BgTexOff = GenColor.FromHex("000000").ToSolidColorTexture();
    private static readonly Texture2D BgTexBurning = ContentFinder<Texture2D>.Get("UI/Widgets/AbilityBurning");
    private static readonly Texture2D BgTexFlaring = ContentFinder<Texture2D>.Get("UI/Widgets/AbilityFlaring");
    private static readonly Texture2D Border = ColorLibrary.Grey.ToSolidColorTexture();

    private static readonly List<Texture2D> AutoBurnBorders = [
        ContentFinder<Texture2D>.Get("UI/Widgets/Borders/B1"),
        ContentFinder<Texture2D>.Get("UI/Widgets/Borders/B2"),
        ContentFinder<Texture2D>.Get("UI/Widgets/Borders/B3"),
        ContentFinder<Texture2D>.Get("UI/Widgets/Borders/B4"),
        ContentFinder<Texture2D>.Get("UI/Widgets/Borders/B5"),
        ContentFinder<Texture2D>.Get("UI/Widgets/Borders/B6"),
        ContentFinder<Texture2D>.Get("UI/Widgets/Borders/B7"),
        ContentFinder<Texture2D>.Get("UI/Widgets/Borders/B8"),
    ];

    private readonly AllomancyAbility ability;
    private readonly Metalborn gene;

    private AcceptanceReport cachedReport;
    private int currentAutoBorderIndex;

    private ulong iteration;

    public AbilitySubGizmo(Verse.Gizmo parent) : base(parent) { }

    public AbilitySubGizmo() { }

    public AbilitySubGizmo(Verse.Gizmo parent, Metalborn gene, AllomancyAbility ability) : base(parent) {
        this.gene = gene;
        this.ability = ability;
    }

    private bool disabled => !cachedReport.Accepted;
    private string? disabledReason => cachedReport.Reason;

    private Texture2D icon => disabled ? ability.def.disabledIcon :
        ability.paused ? ability.def.pausedIcon : ability.def.uiIcon;

    private Texture2D background => ability.status.power switch {
        BurningStatus.Off => BgTexOff,
        BurningStatus.Burning => BgTexBurning,
        BurningStatus.Flaring => BgTexFlaring,
        BurningStatus.Duralumin => BgTexFlaring,
        _ => throw new ArgumentOutOfRangeException(),
    };

    private AcceptanceReport GizmoEnabled() {
        return ability.GizmoEnabled();
    }

    private Texture2D GetBorder() {
        if (!ability.willUseWhileDowned || AutoBurnBorders.NullOrEmpty()) {
            return Border;
        }

        // Change frame every 10 ticks
        if (iteration++ % 10 == 0) {
            currentAutoBorderIndex = (currentAutoBorderIndex + 1) % AutoBurnBorders.Count;
        }

        return AutoBurnBorders[currentAutoBorderIndex];
    }

    public override GizmoResult OnGUI(Rect rect) {
        cachedReport = GizmoEnabled();

        GizmoRenderParms parms = new GizmoRenderParms { shrunk = true, lowLight = false, highLight = false };
        bool isMouseOver = false;
        bool isClicked = false;
        if (Mouse.IsOver(rect)) isMouseOver = true;

        MouseoverSounds.DoRegion(rect, SoundDefOf.Mouseover_Command);
        if (parms.highLight && !disabled) Widgets.DrawStrongHighlight(rect.ExpandedBy(4f));

        UIUtil.DrawIcon(
            rect,
            icon,
            background,
            !disabled ? null : TexUI.GrayscaleGUI,
            borderTexture: GetBorder()
        );

        if (Widgets.ButtonInvisible(rect)) isClicked = true;

        if (isMouseOver) {
            StringBuilder desc = new StringBuilder(ability.Tooltip);

            TaggedString flareOrDeflare =
                (ability.status == BurningStatus.Flaring ? "CS_Deflare" : "CS_Flare").Translate();
            desc.AppendLine("\n");
            if (ability.def.maxPower > 1) {
                desc.AppendLine(
                    "CS_PressToFlare"
                        .Translate(flareOrDeflare.Named("FLARE"), gene.metal.label.Named("METAL"))
                        .Colorize(ColoredText.GeneColor)
                );
            }

            if (ability.def.canUseWhileDowned) {
                desc.AppendLine(
                    "CS_ToggleAutomaticBurning".Translate()
                        .Colorize(ColoredText.GeneColor)
                );
            }

            if (!disabledReason.NullOrEmpty()) {
                desc.AppendLine("\n");
                desc.AppendLine(
                    ("DisabledCommand".Translate() + ": " + disabledReason).Colorize(ColorLibrary.RedReadable)
                );
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

        Messages.Message(
            (string)("DisabledCommand".Translate() + ": " + disabledReason),
            MessageTypeDefOf.RejectInput,
            false
        );
        return new GizmoResult(GizmoState.Mouseover, null);
    }

    public override void ProcessInput(Event ev) {
        cachedReport = GizmoEnabled();
        SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();

        if (ev.shift) {
            ability.willUseWhileDowned = !ability.willUseWhileDowned;
            return;
        }

        if (ability.atLeastBurning) {
            bool isFlaring = ability.status.power == BurningStatus.Flaring;
            bool isBurning = ability.status.power is BurningStatus.Burning;

            if (ev.control) {
                if (isBurning) {
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
                Find.Targeter.BeginTargeting(
                    ability.verb,
                    actionWhenFinished: () => ability.verb.verbProps.range = originalRange
                );
            } else {
                CameraJumper.TryJump(CameraJumper.GetWorldTarget(ability.pawn));
                Find.WorldTargeter.BeginTargeting(
                    t => {
                        if (!ability.ValidateGlobalTarget(t)) return false;

                        ability.QueueCastingJob(t, ev.control ? BurningStatus.Flaring : BurningStatus.Burning);
                        return true;
                    },
                    true,
                    ability.def.uiIcon,
                    !ability.pawn.IsCaravanMember(),
                    extraLabelGetter: ability.WorldMapExtraLabel,
                    canSelectTarget: ability.ValidateGlobalTarget
                );
            }
        } else {
            ability.QueueCastingJob(
                ability.pawn,
                LocalTargetInfo.Invalid,
                ev.control ? BurningStatus.Flaring : BurningStatus.Burning
            );
        }
    }
}