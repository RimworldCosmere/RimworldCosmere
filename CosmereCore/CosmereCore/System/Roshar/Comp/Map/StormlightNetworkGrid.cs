using Cosmere.Core.Comp.Thing;
using Cosmere.System.Roshar.Comp.Thing;
using UnityEngine;
using Verse;

namespace Cosmere.System.Roshar.Comp.Map;

public class StormlightNetworkGrid {
    public List<StormlightBattery> batteries = [];
    public List<StormlightCharger> chargers = [];
    public HashSet<IntVec3> conduitCells = [];
    public List<StormlightReceiver> receivers = [];

    public float TotalStored {
        get {
            float total = 0f;
            for (int i = 0; i < receivers.Count; i++) {
                InvestitureHolder? holder = receivers[i].Investiture;
                if (holder != null) total += holder.currentInvestitureSelf;
            }

            for (int i = 0; i < batteries.Count; i++) {
                InvestitureHolder? holder = batteries[i].Investiture;
                if (holder != null) total += holder.currentInvestitureSelf;
            }

            return total;
        }
    }

    public float TotalCapacity {
        get {
            float total = 0f;
            for (int i = 0; i < receivers.Count; i++) {
                InvestitureHolder? holder = receivers[i].Investiture;
                if (holder != null) total += holder.maxInvestitureSelf;
            }

            for (int i = 0; i < batteries.Count; i++) {
                InvestitureHolder? holder = batteries[i].Investiture;
                if (holder != null) total += holder.maxInvestitureSelf;
            }

            return total;
        }
    }

    public float Draw(float amount) {
        float drawn = 0f;

        for (int i = 0; i < batteries.Count && drawn < amount; i++) {
            InvestitureHolder? holder = batteries[i].Investiture;
            if (holder == null) continue;
            float available = holder.currentInvestitureSelf;
            float toDraw = Mathf.Min(available, amount - drawn);
            if (toDraw <= 0f) continue;
            holder.currentInvestitureSelf -= toDraw;
            drawn += toDraw;
        }

        for (int i = 0; i < receivers.Count && drawn < amount; i++) {
            InvestitureHolder? holder = receivers[i].Investiture;
            if (holder == null) continue;
            float available = holder.currentInvestitureSelf;
            float toDraw = Mathf.Min(available, amount - drawn);
            if (toDraw <= 0f) continue;
            holder.currentInvestitureSelf -= toDraw;
            drawn += toDraw;
        }

        return drawn;
    }

    public void Distribute() {
        for (int i = 0; i < receivers.Count; i++) {
            InvestitureHolder? receiverHolder = receivers[i].Investiture;
            if (receiverHolder == null) continue;
            float excess = receiverHolder.currentInvestitureSelf;
            if (excess <= 0f) continue;

            for (int j = 0; j < batteries.Count; j++) {
                InvestitureHolder? batteryHolder = batteries[j].Investiture;
                if (batteryHolder == null || batteryHolder.isFull) continue;

                float space = batteryHolder.maxInvestitureSelf - batteryHolder.currentInvestitureSelf;
                float toTransfer = Mathf.Min(excess, space);
                if (toTransfer <= 0f) continue;

                batteryHolder.currentInvestitureSelf += toTransfer;
                receiverHolder.currentInvestitureSelf -= toTransfer;
                excess -= toTransfer;

                if (excess <= 0f) break;
            }
        }
    }
}