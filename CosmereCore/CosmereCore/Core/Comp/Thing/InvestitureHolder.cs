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

    public float currentInvestitureSelf {
        get => props.isInfinite ? float.PositiveInfinity : currentInvestitureSelfInt;
        set {
            currentInvestitureSelfInt = Mathf.Max(
                0,
                Mathf.Min(value, props.isInfinite ? float.PositiveInfinity : maxInvestitureSelf)
            );
            parent.BroadcastCompSignal("Cosmere_Investiture_Changed");
        }
    }

    public float currentInvestitureSelfStack => currentInvestitureSelf * parent.stackCount;

    public float maxInvestitureSelfStack => maxInvestitureSelf * parent.stackCount;

    public float currentInvestiture {
        get {
            float total = currentInvestitureSelf * parent.stackCount;
            if (!props.valueBasedOnChildren) return total;
            List<Verse.Thing> childList = children;
            for (int i = 0; i < childList.Count; i++) {
                InvestitureHolder? holder = childList[i].TryGetComp<InvestitureHolder>();
                if (holder != null) {
                    total += holder.currentInvestiture;
                }
            }

            return total;
        }
    }

    public float maxInvestiture {
        get {
            float total = maxInvestitureSelf * parent.stackCount;
            if (!props.valueBasedOnChildren) return total;
            List<Verse.Thing> childList = children;
            for (int i = 0; i < childList.Count; i++) {
                InvestitureHolder? holder = childList[i].TryGetComp<InvestitureHolder>();
                if (holder != null) {
                    total += holder.maxInvestiture;
                }
            }

            return total;
        }
    }

    public List<Verse.Thing> children {
        get {
            int currentTick = Find.TickManager?.TicksGame ?? 0;
            if (cachedChildren != null && cachedChildrenTick == currentTick) {
                return cachedChildren;
            }

            List<Verse.Thing> things = [];

            if (parent.TryGetComp(out InnerStorage innerStorage)) {
                ThingOwner container = innerStorage.innerContainer;
                for (int i = 0; i < container.Count; i++) {
                    if (container[i] != parent && container[i].HasComp<InvestitureHolder>()) {
                        things.Add(container[i]);
                    }
                }
            }

            switch (parent) {
                case ISlotGroupParent storageGroupParent: {
                    SlotGroup? slotGroup = storageGroupParent.GetSlotGroup();
                    if (slotGroup != null) {
                        foreach (Verse.Thing thing in slotGroup.HeldThings) {
                            if (thing != parent && thing.HasComp<InvestitureHolder>()) {
                                things.Add(thing);
                            }
                        }
                    }

                    break;
                }
                case Pawn pawn:
                    if (pawn.inventory?.innerContainer != null) {
                        ThingOwner invContainer = pawn.inventory.innerContainer;
                        for (int i = 0; i < invContainer.Count; i++) {
                            if (invContainer[i] != parent && invContainer[i].HasComp<InvestitureHolder>()) {
                                things.Add(invContainer[i]);
                            }
                        }
                    }

                    if (pawn.equipment?.AllEquipmentListForReading != null) {
                        List<ThingWithComps> equipment = pawn.equipment.AllEquipmentListForReading;
                        for (int i = 0; i < equipment.Count; i++) {
                            if (equipment[i] != parent && equipment[i].HasComp<InvestitureHolder>()) {
                                things.Add(equipment[i]);
                            }
                        }
                    }

                    if (pawn.apparel?.WornApparel != null) {
                        List<Apparel> apparel = pawn.apparel.WornApparel;
                        for (int i = 0; i < apparel.Count; i++) {
                            if (apparel[i] != parent && apparel[i].HasComp<InvestitureHolder>()) {
                                things.Add(apparel[i]);
                            }
                        }
                    }

                    break;
            }

            cachedChildren = things;
            cachedChildrenTick = currentTick;
            return things;
        }
    }

    private new InvestitureHolderProperties props => (InvestitureHolderProperties)base.props;

    public bool isFull => Mathf.Approximately(currentInvestiture, maxInvestiture);

    public override void Initialize(CompProperties props) {
        base.Initialize(props);
        currentInvestitureSelfInt = this.props.startingInvestiture;
        drainRate = this.props.drainRate;
        sharingInvestiture = this.props.shareInvestitureByDefault;
    }

    public float ExudeInvestitureInto(Verse.Thing thing, float amountToDraw) {
        return !ExudeInvestitureInto(thing, amountToDraw, out float amountDrawn) ? 0f : amountDrawn;
    }

    public bool ExudeInvestitureInto(Verse.Thing thing, float amountToDraw, out float amountDrawn) {
        amountDrawn = 0;
        return thing.TryGetComp(out InvestitureHolder investitureHolder) &&
               investitureHolder.AbsorbInvestitureFrom(parent, amountToDraw, out amountDrawn);
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

        if (amountToAbsorb > maxInvestiture) {
            amountToAbsorb = maxInvestiture;
        }

        if (amountToAbsorb > thingInvestiture.currentInvestiture) {
            amountToAbsorb = thingInvestiture.currentInvestiture;
        }

        amountAbsorbed = Mathf.Min(
            amountToAbsorb,
            Mathf.Min(
                maxInvestitureSelfStack - currentInvestitureSelfStack,
                thingInvestiture.currentInvestitureSelfStack
            )
        );
        if (amountAbsorbed > 0) {
            currentInvestitureSelf += amountAbsorbed / parent.stackCount;
            thingInvestiture.currentInvestitureSelf -= amountAbsorbed / thing.stackCount;
        }

        List<Verse.Thing> thingChildren = thingInvestiture.children;
        for (int i = 0; i < thingChildren.Count; i++) {
            amountAbsorbed += AbsorbInvestitureFrom(thingChildren[i], amountToAbsorb);
        }

        return true;
    }

    public override bool AllowStackWith(Verse.Thing other) {
        InvestitureHolder? otherHolder = other.TryGetComp<InvestitureHolder>();
        if (otherHolder == null) return false;

        return Mathf.Approximately(otherHolder.currentInvestitureSelf, currentInvestitureSelf);
    }

    public override void PostPostMake() {
        base.PostPostMake();
        if (parent.def.tickerType != TickerType.Normal) {
            parent.def.tickerType = TickerType.Normal;
        }

        maxInvestitureSelf = props.maxIsInfinity ? float.PositiveInfinity : props.maxInvestiture!.Value;
    }

    private string GetInvestitureLabel() {
        if (parent is not Pawn pawn || pawn.genes == null) {
            return "CC_Stored_Investiture".Translate();
        }

        Invested? investedGene = pawn.genes.GetFirstGeneOfType<Invested>();
        if (investedGene != null && !string.IsNullOrEmpty(investedGene.investitureLabel)) {
            return investedGene.investitureLabel;
        }

        return "CC_Stored_Investiture".Translate();
    }

    public override string CompInspectStringExtra() {
        StringBuilder sb = new StringBuilder();
        sb.Append(GetInvestitureLabel());
        sb.AppendFormat(": {0:F0}", currentInvestiture);
        if (props.showMax) {
            sb.AppendFormat(" / {0:F0}", maxInvestiture);
        }

        if (drainRate > 0.0 && currentInvestiture > 0.0) {
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
        if (Mathf.Approximately(drainRate, 0) || Mathf.Approximately(maxInvestiture, 0)) return;

        if (parent is Pawn && currentInvestitureSelf <= 1) return;
        currentInvestitureSelf -= drainRate;
    }

    public override void PreAbsorbStack(Verse.Thing otherStack, int count) {
        float otherInvestiture = GetCurrentInvestiture(otherStack);
        if (otherInvestiture > 0) {
            currentInvestitureSelf += otherInvestiture * count;
        }
    }

    public override void PostSplitOff(Verse.Thing piece) {
        InvestitureHolder? pieceHolder = piece.TryGetComp<InvestitureHolder>();
        if (pieceHolder != null) {
            pieceHolder.currentInvestitureSelf = currentInvestitureSelf;
        }
    }

    public static float GetCurrentInvestiture(Verse.Thing thing, bool self = true) {
        if (!thing.TryGetComp(out InvestitureHolder investiture)) return 0f;

        return self ? investiture.currentInvestitureSelf : investiture.currentInvestiture;
    }

    public void FillInvestiture() {
        currentInvestitureSelf = maxInvestitureSelf;
        foreach (Verse.Thing child in children ?? []) {
            child.TryGetComp<InvestitureHolder>()?.FillInvestiture();
        }
    }

    public void WipeInvestiture() {
        currentInvestitureSelf = 0;
        foreach (Verse.Thing child in children ?? []) {
            child.TryGetComp<InvestitureHolder>()?.WipeInvestiture();
        }
    }

    public override void PostExposeData() {
        base.PostExposeData();
        Scribe_Values.Look(ref currentInvestitureSelfInt, "currentInvestitureSelf", props.startingInvestiture);
        Scribe_Values.Look(ref drainRate, "drainRate", props.drainRate);
        Scribe_Values.Look(
            ref maxInvestitureSelf,
            "maxInvestitureSelf",
            props.maxIsInfinity ? float.PositiveInfinity : props.maxInvestiture!.Value
        );
        Scribe_Values.Look(ref sharingInvestiture, "sharingInvestiture");
    }
}