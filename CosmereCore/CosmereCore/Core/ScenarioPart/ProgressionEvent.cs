using Cosmere.Core.ScenarioPart.Action;
using Cosmere.Core.ScenarioPart.Trigger;

namespace Cosmere.Core.ScenarioPart;

public class ProgressionEvent {
    public List<ProgressionAction> actions = [];
    public string key = "";
    public bool repeatable;
    public int repeatIntervalDays;
    public List<ProgressionTrigger> triggers = [];
}
