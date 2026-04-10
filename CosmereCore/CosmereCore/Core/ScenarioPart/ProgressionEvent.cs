using Verse;

namespace Cosmere.Core.ScenarioPart;

public class ProgressionEvent {
    public string key = "";
    public List<ProgressionTrigger> triggers = [];
    public List<ProgressionAction> actions = [];
    public bool repeatable;
    public int repeatIntervalDays;
}
