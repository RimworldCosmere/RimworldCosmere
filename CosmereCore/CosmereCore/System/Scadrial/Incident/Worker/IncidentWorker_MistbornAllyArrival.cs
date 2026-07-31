using Cosmere.Core.Incident.Worker;
using Cosmere.Core.Quest;
using Cosmere.System.Scadrial.Quest;
using RimWorld;

namespace Cosmere.System.Scadrial.Incident.Worker;

/// <summary>
///     Gates NamedPawnArrival on having given the Lerasium bead to an ally. Fires through the
///     ordinary storyteller cycle once Dialog_LerasiumChoice.MistbornAllyFlag is set, so the
///     helper "comes when you are pressed" instead of on a scheduled tick.
/// </summary>
public class IncidentWorker_MistbornAllyArrival : IncidentWorker_NamedPawnArrival {
    protected override bool CanFireNowSub(IncidentParms parms) {
        return QuestFlagStore.HasFlag(Dialog_LerasiumChoice.MistbornAllyFlag) && base.CanFireNowSub(parms);
    }
}
