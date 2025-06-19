using System;
using System.Collections.Generic;
using CosmereScadrial.Comps.Things;
using CosmereScadrial.Defs;
using CosmereScadrial.Flags;
using CosmereScadrial.Utils;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Log = CosmereFramework.Log;

namespace CosmereScadrial.Abilities.Allomancy;

public abstract partial class AbstractAbility : Ability {
    private TargetFlags? cachedTargetFlags;
    protected int flareStartTick = -1;
    public GlobalTargetInfo globalTarget;
    public LocalTargetInfo localTarget;
    protected internal bool shouldFlare;
    public BurningStatus? status;

    protected AbstractAbility() { }

    protected AbstractAbility(Pawn pawn) : base(pawn) { }

    protected AbstractAbility(Pawn pawn, Precept sourcePrecept) : base(pawn, sourcePrecept) { }

    protected AbstractAbility(Pawn pawn, AbilityDef def) : base(pawn, def) {
        if (toggleable) {
            status = BurningStatus.Off;
        }
    }

    protected AbstractAbility(Pawn pawn, Precept sourcePrecept, AbilityDef def) : base(pawn, sourcePrecept, def) {
        if (toggleable) {
            status = BurningStatus.Off;
        }
    }

    protected TargetFlags targetFlags {
        get {
            cachedTargetFlags ??= TargetFlagsExtensions.FromAbilityDef(def);

            return (TargetFlags)cachedTargetFlags;
        }
    }

    public float flareDuration => flareStartTick < 0 ? 0 : Find.TickManager.TicksGame - flareStartTick;

    public new AllomanticAbilityDef def => (AllomanticAbilityDef)base.def;

    public bool atLeastPassive => status > BurningStatus.Passive;

    public MetallicArtsMetalDef metal => def.metal;

    protected virtual bool toggleable => def.toggleable;

    protected MetalBurning metalBurning => pawn.GetComp<MetalBurning>();

    public override AcceptanceReport CanCast => metalBurning.CanBurn(metal, def.beuPerTick);

    public TaggedString GetRightClickLabel(LocalTargetInfo targetInfo, BurningStatus burningStatus,
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
    }

    public float GetDesiredBurnRateForStatus() {
        return GetDesiredBurnRateForStatus(status ?? BurningStatus.Off);
    }

    public float GetDesiredBurnRateForStatus(BurningStatus burningStatus) {
        return burningStatus switch {
            BurningStatus.Off => 0,
            BurningStatus.Passive => def.beuPerTick / 2,
            BurningStatus.Burning => def.beuPerTick,
            BurningStatus.Flaring => def.beuPerTick * 2,
            BurningStatus.Duralumin => 0.00000001f,
            _ => throw new ArgumentOutOfRangeException(),
        };
    }

    public float GetStrength() {
        const float multiplier = 12f;
        float rawPower = pawn.GetStatValue(StatDefOf.Cosmere_Allomantic_Power);
        float statusValue = (status ?? BurningStatus.Burning) switch {
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

    public void UpdateStatus(BurningStatus newStatus) {
        if (status == newStatus) return;

        Log.Verbose($"Updating {pawn.NameFullColored}'s {def.defName} from {status} -> {newStatus}");
        BurningStatus? oldStatus = status;
        status = newStatus;
        metalBurning.UpdateBurnSource(metal, GetDesiredBurnRateForStatus(newStatus), def);

        switch (status) {
            // When Disabling (and possibly deflaring)
            case BurningStatus.Off when oldStatus > BurningStatus.Off:
                OnDisable();
                return;
            // When enabling 
            case > BurningStatus.Off when oldStatus == BurningStatus.Off:
                OnEnable();
                return;
            // When Flaring
            case BurningStatus.Flaring when oldStatus < BurningStatus.Flaring:
                OnFlare();
                return;
            // When de-flaring to burning
            case < BurningStatus.Burning when oldStatus == BurningStatus.Flaring:
                OnDeFlare();
                return;
        }
    }

    public override IEnumerable<Verse.Command> GetGizmos() {
        yield break;
    }
}