using System;
using System.Collections.Generic;
using CosmereScadrial.Allomancy.Comp.Thing;
using CosmereScadrial.Def;
using CosmereScadrial.Util;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Logger = CosmereFramework.Logger;

namespace CosmereScadrial.Allomancy.Ability;

public abstract partial class AbstractAbility : RimWorld.Ability {
    protected Mote? burningMote;

    protected int flareStartTick = -1;
    public GlobalTargetInfo? globalTarget;
    public LocalTargetInfo? localTarget;
    public BurningStatus? status;
    public bool willBurnWhileDowned;

    protected AbstractAbility() { }

    protected AbstractAbility(Pawn pawn) : base(pawn) { }

    protected AbstractAbility(Pawn pawn, Precept sourcePrecept) : base(pawn, sourcePrecept) { }

    protected AbstractAbility(Pawn pawn, AbilityDef def) : base(pawn, def) {
        if (toggleable) {
            status = BurningStatus.Off;
        }

        Initialize();
    }

    protected AbstractAbility(Pawn pawn, Precept sourcePrecept, AbilityDef def) : base(pawn, sourcePrecept, def) {
        if (toggleable) {
            status = BurningStatus.Off;
        }

        Initialize();
    }

    public BurningStatus? nextStatus { get; private set; }

    public float flareDuration => flareStartTick < 0 ? 0 : Find.TickManager.TicksGame - flareStartTick;

    public new AllomanticAbilityDef def => (AllomanticAbilityDef)base.def;

    public bool atLeastPassive => status >= BurningStatus.Passive;

    public MetallicArtsMetalDef metal => def.metal;

    protected virtual bool toggleable => def.toggleable;

    protected MetalBurning metalBurning => pawn.GetComp<MetalBurning>();

    public override AcceptanceReport CanCast => metalBurning.CanBurn(metal, def.beuPerTick);

    public new void Initialize() {
        base.Initialize();
        willBurnWhileDowned = def.canBurnWhileDowned;
    }

    public event Action<AbstractAbility, BurningStatus, BurningStatus>? OnStatusChanged;

    public override void AbilityTick() {
        base.AbilityTick();

        burningMote?.Maintain();
    }

    protected virtual float GetMoteScale() {
        return status switch {
            BurningStatus.Off => 0f,
            BurningStatus.Passive => 0.5f,
            BurningStatus.Burning => 1f,
            BurningStatus.Flaring => 2f,
            BurningStatus.Duralumin => 10f,
            _ => 0,
        };
    }

    /*public TaggedString GetRightClickLabel(LocalTargetInfo targetInfo, BurningStatus burningStatus,
        string disableReason = null) {
        bool hasDisableReason = !string.IsNullOrEmpty(disableReason);

        TaggedString label = def.LabelCap.Replace("Target",
            targetInfo.Pawn != null ? targetInfo.Pawn.LabelShort : targetInfo.Thing.LabelNoParenthesisCap);
        if (burningStatus.Equals(BurningStatus.Off)) {
            return string.Equals(def.label, metal.label, StringComparison.CurrentCultureIgnoreCase)
                ? $"Stop Burning {metal.LabelCap}"
                : $"Stop {label}";
        }

        if (string.Equals(def.label, metal.label, StringComparison.CurrentCultureIgnoreCase)) {
            return burningStatus.Equals(BurningStatus.Burning)
                ? $"Burn {metal.LabelCap}{(hasDisableReason ? $" ({disableReason.Trim()})" : "")}"
                : $"Flare {metal.LabelCap}{(hasDisableReason ? $" ({disableReason.Trim()})" : "")}";
        }

        if (status == BurningStatus.Flaring && burningStatus.Equals(BurningStatus.Burning)) {
            label = $"De-Flare: {label}";
        }

        return burningStatus.Equals(BurningStatus.Burning)
            ? $"{label}{(hasDisableReason ? $" ({disableReason.Trim()})" : "")}"
            : $"Flare {label}{(hasDisableReason ? $" ({disableReason.Trim()})" : "")}";
    }*/

    public float GetDesiredBurnRateForStatus() {
        return GetDesiredBurnRateForStatus(status ?? BurningStatus.Off);
    }

    public float GetDesiredBurnRateForStatus(BurningStatus? burningStatus) {
        return (burningStatus ?? status ?? BurningStatus.Off) switch {
            BurningStatus.Off => 0,
            BurningStatus.Passive => def.beuPerTick / 2,
            BurningStatus.Burning => def.beuPerTick,
            BurningStatus.Flaring => def.beuPerTick * 2,
            BurningStatus.Duralumin => 0.00000001f,
            _ => throw new ArgumentOutOfRangeException(),
        };
    }

    public float GetStrength(BurningStatus? burningStatus = null) {
        const float multiplier = 10f;
        float rawPower = pawn.GetStatValue(StatDefOf.Cosmere_Scadrial_Stat_AllomanticPower);
        float statusValue = (burningStatus ?? status ?? BurningStatus.Off) switch {
            BurningStatus.Off => 0f,
            BurningStatus.Passive => 0.5f,
            BurningStatus.Burning => 1f,
            BurningStatus.Flaring => 2f,
            BurningStatus.Duralumin => AllomancyUtility.GetReservePercent(pawn, MetallicArtsMetalDefOf.Duralumin) *
                                       10f,
            _ => 0f,
        };

        return multiplier * def.hediffSeverityFactor * rawPower * statusValue;
    }

    public void UpdateStatus(BurningStatus? newStatus = null) {
        if (newStatus == null) {
            if (nextStatus == null) {
                Logger.Error("Call to UpdateStatus with no status, and no next status");
                return;
            }

            newStatus = nextStatus;
        }

        if (status == newStatus) return;

        Logger.Verbose($"Updating {pawn.NameFullColored}'s {def.defName} from {status} -> {newStatus}");
        BurningStatus? oldStatus = status;
        status = newStatus;

        if (oldStatus == BurningStatus.Off && def.burningMote != null) {
            burningMote ??= MoteMaker.MakeAttachedOverlay(pawn, def.burningMote, Vector3.zero, GetMoteScale());
        } else if (newStatus == BurningStatus.Off) {
            burningMote?.Destroy();
            burningMote = null;
        }

        if (burningMote != null) burningMote.Scale = GetMoteScale();

        metalBurning.UpdateBurnSource(metal, GetDesiredBurnRateForStatus(newStatus), def);

        // OnStatusChanged needs to be called in these orders so that the SeverityCalculator can be updated properly

        // When Disabling (and possibly deflaring)
        if (status == BurningStatus.Off && oldStatus > BurningStatus.Off) {
            OnDisable();
        }

        // When enabling 
        if (status > BurningStatus.Off && oldStatus == BurningStatus.Off) {
            OnEnable();
        }

        // When Flaring
        if (status == BurningStatus.Flaring && oldStatus < BurningStatus.Flaring) {
            OnFlare();
        }

        // When de-flaring to burning
        if (status < BurningStatus.Burning && oldStatus == BurningStatus.Flaring) {
            OnDeFlare();
        }

        OnStatusChanged?.Invoke(this, oldStatus!.Value, newStatus.Value);

        nextStatus = null;
    }

    public override IEnumerable<Verse.Command> GetGizmos() {
        yield break;
    }

    public override bool GizmoDisabled(out string reason) {
        if (!atLeastPassive) return base.GizmoDisabled(out reason);

        reason = "";
        return false;
    }

    protected virtual void OnEnable() { }

    protected virtual void OnDisable() {
        nextStatus = null;
    }

    protected virtual void OnFlare() { }

    protected virtual void OnDeFlare() { }
}