using System.Text;
using Cosmere.Framework.Thing;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.Core.Comp.Thing;

public class InvestitureHolderProperties : CompProperties {
    public float drainRate = 0f;
    public bool maxBasedOnInventory = false;
    public float maxInvestiture = Need.Investiture.MaxInvestiture;
    public bool showMax = true;

    public InvestitureHolderProperties() {
        compClass = typeof(InvestitureHolder);
    }
}

public class InvestitureHolder : ThingComp {
    public float currentInvestitureSelf;
    public float drainRate;

    public float maxInvestitureSelf;

    public float currentInvestiture => currentInvestitureSelf * parent.stackCount +
                                       children.Sum(x => x.TryGetComp<InvestitureHolder>().currentInvestiture);

    public float maxInvestiture => maxInvestitureSelf * parent.stackCount +
                                   children.Sum(x => x.TryGetComp<InvestitureHolder>().maxInvestiture);

    public List<Verse.Thing> children {
        get {
            List<Verse.Thing> things = [];
            switch (parent) {
                case ApparelWithStorage apparelWithStorage:
                    foreach (Verse.Thing? thing in apparelWithStorage.innerContainer ?? []) {
                        if (thing == parent || !thing.HasComp<InvestitureHolder>()) continue;
                        things.Add(thing);
                    }

                    break;
                case IStorageGroupMember storageGroupMember:
                    foreach (Verse.Thing thing in storageGroupMember.Group.HeldThings) {
                        if (thing == parent || !thing.HasComp<InvestitureHolder>()) continue;
                        things.Add(thing);
                    }

                    break;
                case Pawn pawn:
                    foreach (Verse.Thing thing in pawn.inventory?.innerContainer ?? []) {
                        if (thing == parent || !thing.HasComp<InvestitureHolder>()) continue;
                        things.Add(thing);
                    }

                    foreach (ThingWithComps thing in pawn.equipment?.AllEquipmentListForReading ?? []) {
                        if (thing == parent || !thing.HasComp<InvestitureHolder>()) continue;
                        things.Add(thing);
                    }

                    foreach (Apparel thing in pawn.apparel?.WornApparel ?? []) {
                        if (thing == parent || !thing.HasComp<InvestitureHolder>()) continue;
                        things.Add(thing);
                    }

                    break;
            }

            return things;
        }
    }

    private new InvestitureHolderProperties props => (InvestitureHolderProperties)base.props;
    public bool isFull => Mathf.Approximately(currentInvestiture, maxInvestiture);

    public override void Initialize(CompProperties props) {
        base.Initialize(props);
        drainRate = this.props.drainRate;
    }

    public bool ExudeInvestitureInto(Verse.Thing thing, float amountToDraw, out float amountDrawn) {
        InvestitureHolder? thingInvestiture = thing.TryGetComp<InvestitureHolder>();
        if (amountToDraw > currentInvestiture) {
            amountToDraw = currentInvestiture;
        }

        if (amountToDraw > thingInvestiture.maxInvestiture) {
            amountToDraw = thingInvestiture.maxInvestiture;
        }

        float amountToDrawFromEach = amountToDraw;
        List<Verse.Thing> allChildren = children.ToList();
        if (allChildren.Count > 0) {
            amountToDrawFromEach /= allChildren.Count + 1;
        }

        currentInvestitureSelf -= amountToDrawFromEach;
        amountDrawn = amountToDrawFromEach;
        foreach (Verse.Thing? child in allChildren) {
            child.TryGetComp<InvestitureHolder>()
                .ExudeInvestitureInto(child, amountToDrawFromEach, out float amountDrawnFromChild);
            amountDrawn += amountDrawnFromChild;
        }

        thingInvestiture.AddInvestiture(amountDrawn);

        return true;
    }

    public void AddInvestiture(float amount, bool addToChildren = true) {
        if (!addToChildren) {
            currentInvestitureSelf += amount;
            return;
        }

        float amountToAddToEach = amount;
        List<Verse.Thing> allChildren = children.ToList();
        if (allChildren.Count > 0) {
            amountToAddToEach /= allChildren.Count + 1;
        }

        currentInvestitureSelf += amountToAddToEach;
        foreach (Verse.Thing? child in allChildren) {
            child.TryGetComp<InvestitureHolder>().AddInvestiture(amount, addToChildren);
        }
    }

    public bool AbsorbInvestitureFrom(Verse.Thing thing, float amountToAbsorb, out float amountAbsorbed) {
        InvestitureHolder? thingInvestiture = thing.TryGetComp<InvestitureHolder>();
        if (amountToAbsorb > maxInvestiture) {
            amountToAbsorb = maxInvestiture;
        }

        if (amountToAbsorb > thingInvestiture.currentInvestiture) {
            amountToAbsorb = thingInvestiture.currentInvestiture;
        }

        float amountToAbsorbFromEach = amountToAbsorb;
        List<Verse.Thing> allChildren = children.ToList();
        if (allChildren.Count > 0) {
            amountToAbsorbFromEach /= allChildren.Count + 1;
        }

        currentInvestitureSelf -= amountToAbsorbFromEach;
        amountAbsorbed = amountToAbsorbFromEach;
        foreach (Verse.Thing? child in allChildren) {
            child.TryGetComp<InvestitureHolder>()
                .ExudeInvestitureInto(child, amountToAbsorbFromEach, out float amountAbsorbedFromChild);
            amountAbsorbed += amountAbsorbedFromChild;
        }

        thingInvestiture.RemoveInvestiture(amountAbsorbed);

        return true;
    }

    public void RemoveInvestiture(float amount, bool addToChildren = true) {
        if (!addToChildren) {
            currentInvestitureSelf -= amount;
            return;
        }

        float amountToRemoveFromEach = amount;
        List<Verse.Thing> allChildren = children.ToList();
        if (allChildren.Count > 0) {
            amountToRemoveFromEach /= allChildren.Count + 1;
        }

        currentInvestitureSelf -= amountToRemoveFromEach;
        foreach (Verse.Thing? child in allChildren) {
            child.TryGetComp<InvestitureHolder>().RemoveInvestiture(amount, addToChildren);
        }
    }

    public override bool AllowStackWith(Verse.Thing other) {
        return Mathf.Approximately(
            other.TryGetComp<InvestitureHolder>().currentInvestitureSelf,
            currentInvestitureSelf
        );
    }

    public override void PostPostMake() {
        base.PostPostMake();
        // TickerType has to be normal. Otherwise, CompTickInterval never ticks for things inside.
        parent.def.tickerType = TickerType.Normal;
        maxInvestitureSelf = props.maxInvestiture;
    }

    public override string CompInspectStringExtra() {
        StringBuilder sb = new StringBuilder();
        sb.Append("CC_Stored_Investiture".Translate());
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

        currentInvestitureSelf -= drainRate;
        foreach (Verse.Thing child in children ?? []) {
            child.TryGetComp<InvestitureHolder>().CompTickInterval(delta);
        }
    }

    public override void PreAbsorbStack(Verse.Thing otherStack, int count) {
        // currentInvestitureSelf += GetCurrentInvestiture(otherStack) * count;
    }

    public override void PostSplitOff(Verse.Thing piece) {
        piece.TryGetComp<InvestitureHolder>().currentInvestitureSelf = currentInvestitureSelf;
    }

    public static float GetCurrentInvestiture(Verse.Thing thing, bool self = true) {
        if (!thing.TryGetComp(out InvestitureHolder investiture)) return 0f;

        return self ? investiture.currentInvestitureSelf : investiture.currentInvestiture;
    }

    public override void PostExposeData() {
        base.PostExposeData();
        Scribe_Values.Look(ref currentInvestitureSelf, "currentInvestitureSelf");
    }
}