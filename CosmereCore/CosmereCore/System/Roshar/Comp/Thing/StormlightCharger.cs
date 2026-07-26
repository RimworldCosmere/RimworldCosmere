using Cosmere.Core.Comp.Thing;
using Cosmere.System.Roshar.Comp.Map;
using UnityEngine;
using Verse;

namespace Cosmere.System.Roshar.Comp.Thing;

public class StormlightChargerProperties : CompProperties {
    public float chargeRate = 2.0f;
    public int connectRange;

    public StormlightChargerProperties() {
        compClass = typeof(StormlightCharger);
    }
}

public class StormlightCharger : StormlightNode {
    private const int TickInterval = 60;

    private StormlightChargerProperties Props => (StormlightChargerProperties)props;

    public override void CompTick() {
        base.CompTick();
        if (!GenTicks.IsTickInterval(TickInterval)) return;
        if (Network == null) return;

        ChargeContainedSpheres();
    }

    private void ChargeContainedSpheres() {
        InnerStorage? storage = parent.GetComp<InnerStorage>();
        if (storage == null) return;

        StormlightNetworkGrid network = Network!;
        ThingOwner<Verse.Thing> items = storage.innerContainer;
        float budget = Props.chargeRate * TickInterval;

        for (int i = 0; i < items.Count; i++) {
            if (budget <= 0f) break;

            InvestitureHolder? sphereHolder = items[i].TryGetComp<InvestitureHolder>();
            if (sphereHolder == null || sphereHolder.isFull) continue;

            float space = sphereHolder.maxInvestitureSelf - sphereHolder.currentInvestitureSelf;
            float toCharge = Mathf.Min(space, budget);
            float drawn = network.Draw(toCharge);

            if (drawn > 0f) {
                sphereHolder.currentInvestitureSelf += drawn;
                budget -= drawn;
            }
        }
    }
}