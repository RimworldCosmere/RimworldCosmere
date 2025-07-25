using System.Text;
using Cosmere.Framework.Comp.Thing;
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
        }
    }
}

public class InvestitureHolder : ThingComp {
    private float currentInvestitureSelfInt;

    public float drainRate;

    public float maxInvestitureSelf;
    public bool sharingInvestiture;

    public float currentInvestitureSelf {
        get => props.isInfinite ? float.PositiveInfinity : currentInvestitureSelfInt;
        set => currentInvestitureSelfInt = Mathf.Max(0, Mathf.Min(value, maxInvestitureSelf));
    }

    public float currentInvestitureSelfStack => currentInvestitureSelf * parent.stackCount;
    public float maxInvestitureSelfStack => maxInvestitureSelf * parent.stackCount;

    public float currentInvestiture => currentInvestitureSelf * parent.stackCount +
                                       (props.valueBasedOnChildren
                                           ? children.Sum(x => x.TryGetComp<InvestitureHolder>().currentInvestiture)
                                           : 0);

    public float maxInvestiture => maxInvestitureSelf * parent.stackCount +
                                   (props.valueBasedOnChildren
                                       ? children.Sum(x => x.TryGetComp<InvestitureHolder>().maxInvestiture)
                                       : 0);

    public List<Verse.Thing> children {
        get {
            List<Verse.Thing> things = [];

            bool ThingWithInvestiture(Verse.Thing thing) {
                return thing != parent && thing.HasComp<InvestitureHolder>();
            }

            if (parent.TryGetComp(out InnerStorage innerStorage)) {
                things.AddRange(innerStorage.innerContainer.Where(ThingWithInvestiture));
            }

            switch (parent) {
                case ISlotGroupParent storageGroupParent:
                    things.AddRange(storageGroupParent.GetSlotGroup().HeldThings.Where(ThingWithInvestiture));

                    break;
                case Pawn pawn:
                    things.AddRange(pawn.inventory?.innerContainer?.Where(ThingWithInvestiture) ?? []);
                    things.AddRange(pawn.equipment?.AllEquipmentListForReading?.Where(ThingWithInvestiture) ?? []);
                    things.AddRange(pawn.apparel?.WornApparel?.Where(ThingWithInvestiture) ?? []);

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
                         ) *
                         thing.stackCount;
        if (amountAbsorbed > 0) {
            currentInvestitureSelf += amountAbsorbed;
            thingInvestiture.currentInvestitureSelf -= amountAbsorbed / thing.stackCount;
        }

        amountAbsorbed += thingInvestiture.children.Sum(child => AbsorbInvestitureFrom(child, amountToAbsorb));

        return true;
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
        maxInvestitureSelf = props.maxIsInfinity ? float.PositiveInfinity : props.maxInvestiture!.Value;
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
        Scribe_Values.Look(ref currentInvestitureSelfInt, "currentInvestitureSelfInt");
    }
}