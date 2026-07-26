using System.Text;
using Cosmere.Core.Comp.Thing;
using Cosmere.System.Roshar.Comp.Map;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.System.Roshar.Comp.Thing;

public class StormlightNodeProperties : CompProperties {
    public int connectRange;

    public StormlightNodeProperties() {
        compClass = typeof(StormlightNode);
    }
}

public class StormlightNode : ThingComp {
    public static Material MatConnectorAnticipated => PowerOverlayMats.MatConnectorAnticipated;

    public StormlightNetworkGrid? Network { get; set; }

    public bool IsConnected => Network != null;

    public InvestitureHolder? Investiture => parent.GetComp<InvestitureHolder>();

    public override void PostSpawnSetup(bool respawningAfterLoad) {
        base.PostSpawnSetup(respawningAfterLoad);
        StormlightNetwork? network = parent.Map?.GetComponent<StormlightNetwork>();
        network?.MarkDirty();
    }

    public override void PostDestroy(DestroyMode mode, Verse.Map previousMap) {
        base.PostDestroy(mode, previousMap);
        Network = null;
        StormlightNetwork? network = previousMap?.GetComponent<StormlightNetwork>();
        network?.MarkDirty();
    }

    public override string? CompInspectStringExtra() {
        if (this is StormlightConduit) return null;

        StringBuilder sb = new StringBuilder();
        if (Network != null) {
            sb.AppendFormat("Network: {0:F0} / {1:F0} Stormlight", Network.TotalStored, Network.TotalCapacity);
        }
        else {
            sb.Append("Not connected to stormlight network");
        }

        return sb.ToString();
    }
}