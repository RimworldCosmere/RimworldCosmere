using System.Text;
using Cosmere.Core.Gene;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.Core.Comp.Thing;

public class InvestitureHolderProperties : CompProperties {
    public float drainRate = 0f;
    public bool isInfinite = false;
    public float? maxInvestiture;
    public bool maxIsInfinity = false;
    public bool shareInvestitureByDefault = true;
    public bool showMax = true;
    public float startingInvestiture = 1;
    public bool valueBasedOnChildren = true;

    public InvestitureHolderProperties() {
        compClass = typeof(InvestitureHolder);
    }

    public override IEnumerable<string> ConfigErrors(ThingDef parentDef) {
        foreach (string configError in base.ConfigErrors(parentDef)) {
            yield return configError;
        }

        if (!maxIsInfinity) {
            if (!maxInvestiture.HasValue) {
                yield return "maxInvestiture is required";
            }

            if (maxInvestiture < 0) {
                yield return "maxInvestiture must be greater than or equal to 0.";
            }
        }
    }

    public override void PostLoadSpecial(ThingDef parent) {
        base.PostLoadSpecial(parent);
        if (maxIsInfinity) {
            showMax = false;
            maxInvestiture = float.PositiveInfinity;
        }

        if (Mathf.Approximately(maxInvestiture!.Value, 0)) {
            startingInvestiture = 0;
        }
    }
}

public class InvestitureHolder : ThingComp {
    private List<Verse.Thing>? cachedChildren;
    private int cachedChildrenTick = -1;
    private float currentInvestitureSelfInt;

    public float drainRate;

    public float maxInvestitureSelf;
    public bool sharingInvestiture;

    public float CurrentInvestitureSelf {
        get => Props.isInfinite ? float.PositiveInfinity : currentInvestitureSelfInt;
        set {
            currentInvestitureSelfInt = Mathf.Max(
                0,
                Mathf.Min(value, Props.isInfinite ? float.PositiveInfinity : maxInvestitureSelf)
            );
            parent.BroadcastCompSignal("Cosmere_Investiture_Changed");
        }
    }

    public float CurrentInvestitureSelfStack => CurrentInvestitureSelf * parent.stackCount;

    public float MaxInvestitureSelfStack => maxInvestitureSelf * parent.stackCount;

    public float CurrentInvestiture {
        get {
            float total = CurrentInvestitureSelf * parent.stackCount;
            if (!Props.valueBasedOnChildren) return total;
            List<Verse.Thing> childList = Children;
            for (int i = 0; i < childList.Count; i++) {
                InvestitureHolder? holder = childList[i].TryGetComp<InvestitureHolder>();
                if (holder != null) {
                    total += holder.CurrentInvestiture;
                }
            }

            return total;
        }
    }

    public float MaxInvestiture {
        get {
            float total = maxInvestitureSelf * parent.stackCount;
            if (!Props.valueBasedOnChildren) return total;
            List<Verse.Thing> childList = Children;
            for (int i = 0; i < childList.Count; i++) {
                InvestitureHolder? holder = childList[i].TryGetComp<InvestitureHolder>();
                if (holder != null) {
                    total += holder.MaxInvestiture;
                }
            }

            return total;
        }
    }

    public List<Verse.Thing> Children {
        get {
            int currentTick = Find.TickManager?.TicksGame ?? 0;
            if (cachedChildren != null && cachedChildrenTick == currentTick) {
                return cachedChildren;
            }

            List<Verse.Thing> things = [];

            if (parent.TryGetComp(out InnerStorage innerStorage)) {
                AddHolders(things, innerStorage.innerContainer);
            }

            switch (parent) {
                case ISlotGroupParent storageGroupParent:
                    AddSlotGroupHolders(things, storageGroupParent);
                    break;

                case Pawn pawn:
                    AddHolders(things, pawn.inventory?.innerContainer);
                    AddHolders(things, pawn.equipment?.AllEquipmentListForReading);
                    AddHolders(things, pawn.apparel?.WornApparel);
                    break;
            }

            cachedChildren = things;
            cachedChildrenTick = currentTick;
            return things;
        }
    }

    private void AddHolders<T>(List<Verse.Thing> things, IList<T>? source)
        where T : Verse.Thing {
        if (source == null) return;

        for (int i = 0; i < source.Count; i++) {
            if (source[i] != parent && source[i].HasComp<InvestitureHolder>()) {
                things.Add(source[i]);
            }
        }
    }

    private void AddSlotGroupHolders(List<Verse.Thing> things, ISlotGroupParent slotGroupParent) {
        SlotGroup? slotGroup = slotGroupParent.GetSlotGroup();
        if (slotGroup == null) return;

        foreach (Verse.Thing thing in slotGroup.HeldThings) {
            if (thing != parent && thing.HasComp<InvestitureHolder>()) {
                things.Add(thing);
            }
        }
    }

    private InvestitureHolderProperties Props => (InvestitureHolderProperties)props;

    public bool IsFull => Mathf.Approximately(CurrentInvestiture, MaxInvestiture);

    public override void Initialize(CompProperties props) {
        base.Initialize(props);
        currentInvestitureSelfInt = this.Props.startingInvestiture;
        drainRate = this.Props.drainRate;
        sharingInvestiture = this.Props.shareInvestitureByDefault;
    }

    public float ExudeInvestitureInto(Verse.Thing thing, float amountToDraw) {
        return !ExudeInvestitureInto(thing, amountToDraw, out float amountDrawn) ? 0f : amountDrawn;
    }

    public bool ExudeInvestitureInto(Verse.Thing thing, float amountToDraw, out float amountDrawn) {
        amountDrawn = 0;
        return thing.TryGetComp(out InvestitureHolder investitureHolder) &&
               investitureHolder.AbsorbInvestitureFrom(parent, amountToDraw, out amountDrawn);
    }

    /// <summary>
    ///     Gets the Investiture received per unit drawn. Only a pawn carries the stat, anything else trades one for one.
    /// </summary>
    private float AbsorptionEfficiency {
        get {
            if (parent is not Pawn pawn) return 1f;

            return Mathf.Max(0.01f, pawn.GetStatValue(StatDefOf.Cosmere_InvestitureAbsorption));
        }
    }

    public float AbsorbInvestitureFrom(Verse.Thing thing, float amountToAbsorb) {
        return !AbsorbInvestitureFrom(thing, amountToAbsorb, out float amountAbsorbed) ? 0f : amountAbsorbed;
    }

    public bool AbsorbInvestitureFrom(Verse.Thing thing, float amountToAbsorb, out float amountAbsorbed) {
        InvestitureHolder? thingInvestiture = thing.TryGetComp<InvestitureHolder>();
        if (thingInvestiture == null) {
            amountAbsorbed = 0f;
            return false;
        }

        if (amountToAbsorb > MaxInvestiture) {
            amountToAbsorb = MaxInvestiture;
        }

        if (amountToAbsorb > thingInvestiture.CurrentInvestiture) {
            amountToAbsorb = thingInvestiture.CurrentInvestiture;
        }

        // capped by space/efficiency, so the multiplied gain fills the space without overflowing
        float efficiency = AbsorptionEfficiency;
        float drawn = Mathf.Min(
            amountToAbsorb,
            Mathf.Min(
                (MaxInvestitureSelfStack - CurrentInvestitureSelfStack) / efficiency,
                thingInvestiture.CurrentInvestitureSelfStack
            )
        );

        amountAbsorbed = drawn;
        if (drawn > 0) {
            CurrentInvestitureSelf += drawn * efficiency / parent.stackCount;
            thingInvestiture.CurrentInvestitureSelf -= drawn / thing.stackCount;
        }

        List<Verse.Thing> thingChildren = thingInvestiture.Children;
        for (int i = 0; i < thingChildren.Count; i++) {
            amountAbsorbed += AbsorbInvestitureFrom(thingChildren[i], amountToAbsorb);
        }

        return true;
    }

    public override bool AllowStackWith(Verse.Thing other) {
        InvestitureHolder? otherHolder = other.TryGetComp<InvestitureHolder>();
        if (otherHolder == null) return false;

        return Mathf.Approximately(otherHolder.CurrentInvestitureSelf, CurrentInvestitureSelf);
    }

    public override void PostPostMake() {
        base.PostPostMake();
        parent.def.tickerType = TickerType.Normal;

        maxInvestitureSelf = Props.maxIsInfinity ? float.PositiveInfinity : Props.maxInvestiture!.Value;
    }

    private string GetInvestitureLabel() {
        if (parent is not Pawn pawn || pawn.genes == null) {
            return "CC_Stored_Investiture".Translate();
        }

        Invested? investedGene = pawn.genes.GetFirstGeneOfType<Invested>();
        if (investedGene != null && !string.IsNullOrEmpty(investedGene.InvestitureLabel)) {
            return investedGene.InvestitureLabel;
        }

        return "CC_Stored_Investiture".Translate();
    }

    public override string CompInspectStringExtra() {
        StringBuilder sb = new StringBuilder();
        sb.Append(GetInvestitureLabel());
        sb.AppendFormat(": {0:F0}", CurrentInvestiture);
        if (Props.showMax) {
            sb.AppendFormat(" / {0:F0}", MaxInvestiture);
        }

        if (drainRate > 0.0 && CurrentInvestiture > 0.0) {
            sb.AppendFormat(" (-{0:F4}/sec)", drainRate);
        }

        return sb.ToString();
    }

    public override string CompTipStringExtra() {
        return CompInspectStringExtra();
    }

    public override void CompTickInterval(int delta) {
        base.CompTickInterval(delta);

        if (!GenTicks.IsTickIntervalDelta(GenTicks.TickRareInterval, delta)) return;
        if (Mathf.Approximately(drainRate, 0) || Mathf.Approximately(MaxInvestiture, 0)) return;

        if (parent is Pawn && CurrentInvestitureSelf <= 1) return;
        CurrentInvestitureSelf -= drainRate;
    }

    public override void PreAbsorbStack(Verse.Thing otherStack, int count) {
        float otherInvestiture = GetCurrentInvestiture(otherStack);
        if (otherInvestiture > 0) {
            CurrentInvestitureSelf += otherInvestiture * count;
        }
    }

    public override void PostSplitOff(Verse.Thing piece) {
        InvestitureHolder? pieceHolder = piece.TryGetComp<InvestitureHolder>();
        if (pieceHolder != null) {
            pieceHolder.CurrentInvestitureSelf = CurrentInvestitureSelf;
        }
    }

    public static float GetCurrentInvestiture(Verse.Thing thing, bool self = true) {
        if (!thing.TryGetComp(out InvestitureHolder investiture)) return 0f;

        return self ? investiture.CurrentInvestitureSelf : investiture.CurrentInvestiture;
    }

    public void FillInvestiture() {
        CurrentInvestitureSelf = maxInvestitureSelf;
        foreach (Verse.Thing child in Children) {
            child.TryGetComp<InvestitureHolder>()?.FillInvestiture();
        }
    }

    public void WipeInvestiture() {
        CurrentInvestitureSelf = 0;
        foreach (Verse.Thing child in Children) {
            child.TryGetComp<InvestitureHolder>()?.WipeInvestiture();
        }
    }

    public override void PostExposeData() {
        base.PostExposeData();
        Scribe_Values.Look(ref currentInvestitureSelfInt, "currentInvestitureSelf", Props.startingInvestiture);
        Scribe_Values.Look(ref drainRate, "drainRate", Props.drainRate);
        Scribe_Values.Look(
            ref maxInvestitureSelf,
            "maxInvestitureSelf",
            Props.maxIsInfinity ? float.PositiveInfinity : Props.maxInvestiture!.Value
        );
        Scribe_Values.Look(ref sharingInvestiture, "sharingInvestiture");
    }
}
