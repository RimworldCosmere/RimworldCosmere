using System;
using System.Text;
using Cosmere.Core.Comp.Thing;
using Cosmere.Core.Def;
using Cosmere.Core.DefModExtension;
using UnityEngine;
using Verse;
using Logger = Cosmere.Core.Logger;

namespace Cosmere.System.Scadrial.Feruchemy.Comp.Thing;

public class MetalmindProperties : CompProperties {
    public float maxAmount;

    public MetalmindProperties() {
        compClass = typeof(Metalmind);
    }
}

public class Metalmind : ThingComp, IMetalmindSource {
    private MetalDef? cachedMetal;

    private bool equippedInt = true;
    public bool equipped {
        get => equippedInt;
        set => equippedInt = value;
    }

    private float storedAmountInt;
    public Pawn? owner { get; private set; }
    private new MetalmindProperties props => (MetalmindProperties)base.props;
    public float maxAmount => props.maxAmount;
    public bool canStore => equipped && storedAmount < maxAmount;
    public bool canTap => equipped && storedAmount > 0;
    private InvestitureHolder investitureHolder => parent.GetComp<InvestitureHolder>();

    public float storedAmount {
        get => storedAmountInt;
        private set {
            storedAmountInt = value;
            investitureHolder.currentInvestitureSelf = value * Constants.BreathEquivalentUnitsPerMetalUnit;
        }
    }

    public MetalDef? metal {
        get {
            if (cachedMetal != null) return cachedMetal;
            if (parent.def.GetModExtension<MetalsLinked>() is { } metalsLinked) {
                cachedMetal = metalsLinked.Metals.FirstOrDefault();
            }

            if (parent.Stuff is { } stuff) {
                cachedMetal = DefDatabase<MetalDef>.GetNamed(stuff.defName);
            }

            if (cachedMetal != null) return cachedMetal;

            Logger.Error("Metalmind doesn't have a metal");
            return null;
        }
    }

    public override void PostPostMake() {
        investitureHolder.maxInvestitureSelf = maxAmount * Constants.BreathEquivalentUnitsPerMetalUnit;
    }

    public void AddStored(float amount) {
        if (!canStore) return;
        if (!ValidateOwner()) return;

        storedAmount = Mathf.Clamp(storedAmount + amount, 0, maxAmount);
    }

    public void ConsumeStored(float amount) {
        if (!canTap) return;
        if (!ValidateOwner()) return;

        storedAmount = Mathf.Clamp(storedAmount - amount, 0, maxAmount);
    }

    private bool ValidateOwner() {
        IThingHolder? currentOwner = parent.holdingOwner?.Owner;
        if (owner == null) {
            owner = currentOwner as Pawn;
            return true;
        }

        return currentOwner == null || owner.Equals(currentOwner);
    }

    public override void PostExposeData() {
        base.PostExposeData();

        Scribe_Values.Look(ref storedAmountInt, "storedAmount");
        Scribe_Values.Look(ref equippedInt, "equipped");

        // Optionally restore cachedMetal if needed (but can be recomputed)
        if (Scribe.mode == LoadSaveMode.PostLoadInit) {
            cachedMetal = null;
            if (parent?.Stuff != null) {
                cachedMetal = DefDatabase<MetalDef>.GetNamedSilentFail(parent.Stuff.defName);
            }
            owner = parent?.holdingOwner?.Owner as Pawn;
        }
    }

    public override string CompInspectStringExtra() {
        StringBuilder sb = new StringBuilder();
        TaggedString coloredOwner = owner?.NameFullColored ?? "None".Colorize(ColoredText.DateTimeColor);
        sb.AppendLine("CS_MetalmindOwner".Translate() + ": " + coloredOwner);
        NamedArgument coloredMetal = metal?.coloredLabel.Named("METAL") ?? "unknown".Named("METAL");
        sb.Append("CS_MetalmindStored".Translate(coloredMetal) + $": {storedAmountInt:F1} / {maxAmount}");

        return sb.ToString();
    }
}