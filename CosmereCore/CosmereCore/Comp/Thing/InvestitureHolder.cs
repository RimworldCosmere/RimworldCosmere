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

    public float maxInvestitureSelf;

    public float currentInvestiture => currentInvestitureSelf +
                                       children.Sum(x => x.TryGetComp<InvestitureHolder>().currentInvestiture);

    public float maxInvestiture => maxInvestitureSelf +
                                   children.Sum(x => x.TryGetComp<InvestitureHolder>().maxInvestiture);

    private IEnumerable<Verse.Thing> children {
        get {
            if (parent is not IHaulDestination) yield break;

            switch (parent) {
                case ApparelWithStorage apparelWithStorage:
                    foreach (Verse.Thing? thing in apparelWithStorage.inventory.innerContainer) {
                        if (thing == parent || !thing.HasComp<InvestitureHolder>()) continue;
                        yield return thing;
                    }

                    break;
                case IStorageGroupMember storageGroupMember:
                    foreach (Verse.Thing thing in storageGroupMember.Group.HeldThings) {
                        if (thing == parent || !thing.HasComp<InvestitureHolder>()) continue;
                        yield return thing;
                    }

                    break;
                case Pawn pawn:
                    foreach (Verse.Thing thing in pawn.inventory.innerContainer) {
                        if (thing == parent || !thing.HasComp<InvestitureHolder>()) continue;
                        yield return thing;
                    }

                    foreach (ThingWithComps? thing in pawn.equipment.AllEquipmentListForReading) {
                        if (thing == parent || !thing.HasComp<InvestitureHolder>()) continue;
                        yield return thing;
                    }

                    foreach (Verse.Thing? thing in pawn.apparel.GetDirectlyHeldThings()) {
                        if (thing == parent || !thing.HasComp<InvestitureHolder>()) continue;
                        yield return thing;
                    }

                    break;
            }
        }
    }

    private new InvestitureHolderProperties props => (InvestitureHolderProperties)base.props;

    public override void PostPostMake() {
        base.PostPostMake();
        maxInvestitureSelf = props.maxInvestiture;
    }

    public override string CompInspectStringExtra() {
        StringBuilder sb = new StringBuilder();
        sb.Append("CC_Stored_Investiture".Translate());
        sb.AppendFormat(": {0:F0}", currentInvestitureSelf);
        if (props.showMax) {
            sb.AppendFormat(" / {0:F0}", maxInvestiture);
        }

        return sb.ToString();
    }

    public override string CompTipStringExtra() {
        return CompInspectStringExtra();
    }

    public override void CompTickInterval(int delta) {
        base.CompTickInterval(delta);

        if (Mathf.Approximately(props.drainRate, 0)) return;
        if (!GenTicks.IsTickIntervalDelta(GenTicks.TicksPerRealSecond, delta)) return;

        currentInvestitureSelf -= props.drainRate;
    }

    public override void PostExposeData() {
        base.PostExposeData();
        Scribe_Values.Look(ref currentInvestitureSelf, "currentInvestitureSelf", 0f, true);
    }
}