using System;
using CosmereResources;
using CosmereScadrial.Allomancy.Comp.Thing;
using CosmereScadrial.Def;
using CosmereScadrial.Extension;
using CosmereScadrial.Gene;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Logger = CosmereFramework.Logger;

namespace CosmereScadrial.Allomancy.Ability;

public abstract partial class AbstractAbility : RimWorld.Ability {
    protected Mote? burningMote;

    protected int flareStartTick = -1;
    protected Allomancer gene;
    public GlobalTargetInfo? globalTarget;
    public LocalTargetInfo? localTarget;
    public BurningStatus? status;
    public bool willBurnWhileDowned;

    protected AbstractAbility() { }

    protected AbstractAbility(Pawn pawn) : base(pawn) {
        gene = pawn.genes.GetAllomanticGeneForMetal(metal)!;
    }

    protected AbstractAbility(Pawn pawn, Precept sourcePrecept) : base(pawn, sourcePrecept) {
        gene = pawn.genes.GetAllomanticGeneForMetal(metal)!;
    }

    protected AbstractAbility(Pawn pawn, AbilityDef def) : base(pawn, def) {
        if (toggleable) {
            status = BurningStatus.Off;
        }

        gene = pawn.genes.GetAllomanticGeneForMetal(metal)!;
        Initialize();
    }

    protected AbstractAbility(Pawn pawn, Precept sourcePrecept, AbilityDef def) : base(pawn, sourcePrecept, def) {
        if (toggleable) {
            status = BurningStatus.Off;
        }

        gene = pawn.genes.GetAllomanticGeneForMetal(metal)!;
        Initialize();
    }

    public BurningStatus? nextStatus { get; private set; }

    public float flareDuration => flareStartTick < 0 ? 0 : Find.TickManager.TicksGame - flareStartTick;

    public new AllomanticAbilityDef def {
        get => (AllomanticAbilityDef)base.def;
        set => base.def = value;
    }

    public bool atLeastPassive => status >= BurningStatus.Passive;

    public MetallicArtsMetalDef metal => def.metal;

    protected virtual bool toggleable => def.toggleable;

    protected MetalBurning metalBurning => pawn.GetComp<MetalBurning>();

    public override AcceptanceReport CanCast => metalBurning.CanBurn(metal, def.beuPerTick);

    public new void Initialize() {
        if (def.comps.Any<AbilityCompProperties>()) {
            comps = [];
            for (int index = 0; index < def.comps.Count; ++index) {
                AbilityComp? abilityComp = null;
                try {
                    abilityComp = (AbilityComp)Activator.CreateInstance(def.comps[index].compClass);
                    abilityComp.parent = this;
                    comps.Add(abilityComp);
                    abilityComp.Initialize(def.comps[index]);
                } catch (Exception ex) {
                    Log.Error("Could not instantiate or initialize an AbilityComp: " + ex);
                    comps.Remove(abilityComp);
                }
            }
        }

        if (Id == -1)
            Id = Find.UniqueIDsManager.GetNextAbilityID();
        if (VerbTracker.PrimaryVerb is IAbilityVerb primaryVerb)
            primaryVerb.Ability = this;
        if (def.charges <= 0)
            return;
        maxCharges = def.charges;
        RemainingCharges = maxCharges;

        willBurnWhileDowned = def is { canBurnWhileDowned: true, autoBurnWhileDownedByDefault: true };
        gene ??= pawn.genes.GetAllomanticGeneForMetal(metal)!;
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
            BurningStatus.Duralumin => pawn.GetAllomanticReservePercent(MetalDefOf.Duralumin) * 10f,
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

    protected virtual void OnEnable() { }

    protected virtual void OnDisable() {
        nextStatus = null;
    }

    protected virtual void OnFlare() { }

    protected virtual void OnDeFlare() { }
}