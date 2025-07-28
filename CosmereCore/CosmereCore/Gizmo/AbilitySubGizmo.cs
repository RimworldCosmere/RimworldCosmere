using System.Text;
using Cosmere.Core.Ability;
using Cosmere.Core.Gene;
using Cosmere.Core.Hediff;
using Cosmere.Framework.Util;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Cosmere.Core.Gizmo;

[StaticConstructorOnStartup]
public class AbilitySubGizmo<TGene, THediff> : SubGizmo where TGene : Invested where THediff : AbstractHediff<TGene> {
    protected static readonly Texture2D BgTexOff = GenColor.FromHex("000000").ToSolidColorTexture();
    protected static readonly Texture2D BgTexBurning = ContentFinder<Texture2D>.Get("UI/Widgets/AbilityBurning");
    protected static readonly Texture2D BgTexFlaring = ContentFinder<Texture2D>.Get("UI/Widgets/AbilityFlaring");
    protected static readonly Texture2D Border = ColorLibrary.Grey.ToSolidColorTexture();

    protected static readonly List<Texture2D> AutoBurnBorders = [
        ContentFinder<Texture2D>.Get("UI/Widgets/Borders/B1"),
        ContentFinder<Texture2D>.Get("UI/Widgets/Borders/B2"),
        ContentFinder<Texture2D>.Get("UI/Widgets/Borders/B3"),
        ContentFinder<Texture2D>.Get("UI/Widgets/Borders/B4"),
        ContentFinder<Texture2D>.Get("UI/Widgets/Borders/B5"),
        ContentFinder<Texture2D>.Get("UI/Widgets/Borders/B6"),
        ContentFinder<Texture2D>.Get("UI/Widgets/Borders/B7"),
        ContentFinder<Texture2D>.Get("UI/Widgets/Borders/B8"),
    ];

    protected readonly AbstractAbility<TGene, THediff> ability;
    protected readonly TGene gene;

    protected AcceptanceReport cachedReport;
    protected int currentAutoBorderIndex;

    protected ulong iteration;

    public AbilitySubGizmo() { }

    public AbilitySubGizmo(Verse.Gizmo parent) : base(parent) { }

    public AbilitySubGizmo(Verse.Gizmo parent, TGene gene, AbstractAbility<TGene, THediff> ability) : base(parent) {
        this.gene = gene;
        this.ability = ability;
    }

    protected bool disabled => !cachedReport.Accepted;
    protected string? disabledReason => cachedReport.Reason;

    private Texture2D icon => disabled ? ability.def.disabledIcon :
        ability.paused ? ability.def.pausedIcon : ability.def.uiIcon;

    private Texture2D background => ability.status.power switch {
        0 => BgTexOff,
        1 => BgTexBurning,
        _ => BgTexFlaring,
    };

    private AcceptanceReport GizmoEnabled() {
        return ability.GizmoEnabled();
    }

    private Texture2D GetBorder() {
        if (!ability.willUseWhileDowned || !ability.willUseWhileInjured || AutoBurnBorders.NullOrEmpty()) {
            return Border;
        }

        // Change frame every 10 ticks
        if (iteration++ % 10 == 0) {
            currentAutoBorderIndex = (currentAutoBorderIndex + 1) % AutoBurnBorders.Count;
        }

        return AutoBurnBorders[currentAutoBorderIndex];
    }

    protected virtual string GetTooltip() {
        StringBuilder desc = new StringBuilder(ability.Tooltip);

        if (ability.def.maxPower > 1) {
            desc.AppendLine("\n");
            desc.AppendLine(GetPowerUpDisplay().Resolve());
        }

        if (ability.def.autoUseWhileDowned || ability.def.autoUseWhileInjured) {
            string key = ability.def.isAutocast
                ? "CC_Gizmo_ToggleAutomaticCast_Click"
                : "CC_Gizmo_ToggleAutomaticCast_ShiftClick";
            desc.AppendLine("\n");
            desc.AppendLine(
                key.Translate()
                    .Colorize(ColoredText.GeneColor)
            );
        }

        if (!disabledReason.NullOrEmpty()) {
            desc.AppendLine("\n");
            desc.AppendLine(
                ("DisabledCommand".Translate() + ": " + disabledReason).Colorize(ColorLibrary.RedReadable)
            );
        }

        return desc.ToString();
    }

    protected virtual TaggedString GetPowerUpDisplay() {
        return ability.status.power > 1
            ? "CC_Gizmo_PressToPowerDown".Translate()
            : "CC_Gizmo_PressToPowerUp".Translate();
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
            TooltipHandler.TipRegion(rect, GetTooltip, Gen.HashCombineInt(GetHashCode(), 749141947));
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

        if (ev.shift || ability.def.isAutocast) {
            if (ability.def.canUseWhileDowned) ability.willUseWhileDowned = !ability.willUseWhileDowned;
            if (ability.def.autoUseWhileInjured) ability.willUseWhileInjured = !ability.willUseWhileInjured;
            if (ability.def.isAutocast && ability.status.isActive) {
                ability.UpdateStatus(Active.Off);
            }

            return;
        }

        if (ability.status.isActive) {
            if (ev.control) {
                if (ability.status.power == 1) {
                    ability.SetNextStatus(Ability.Status.PowerTwo);
                } else if (ability.status.isPoweredUp) {
                    ability.SetNextStatus(Ability.Status.PowerOne);
                }
            } else {
                ability.SetNextStatus(Active.Off);
            }

            ability.UpdateStatus(ability.nextStatus!.Value);

            return;
        }

        if (ability.def.targetRequired) {
            Find.DesignatorManager.Deselect();
            ability.SetNextStatus(ev.control ? Ability.Status.PowerTwo : Ability.Status.PowerOne, true);
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

                        ability.QueueCastingJob(t, ev.control ? Ability.Status.PowerTwo : Ability.Status.PowerOne);
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
                ev.control ? Ability.Status.PowerTwo : Ability.Status.PowerOne
            );
        }
    }
}