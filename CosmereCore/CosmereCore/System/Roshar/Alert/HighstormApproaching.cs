using Cosmere.System.Roshar.Comp.Map;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Alert;

public class HighstormApproaching : Alert_Critical {
    private const int WarningThresholdTicks = 60000;

    public override string GetLabel() {
        int ticksRemaining = GetTicksUntilNextStorm();
        if (ticksRemaining > 0) {
            float hoursRemaining = ticksRemaining / (float)GenDate.TicksPerHour;
            return "CR_Highstorm_Alert_Label".Translate() + $" ({hoursRemaining:F1}h)";
        }

        return "CR_Highstorm_Alert_Label".Translate();
    }

    public override AlertReport GetReport() {
        List<Map> maps = Find.Maps;
        for (int i = 0; i < maps.Count; i++) {
            HighstormScheduler scheduler = maps[i].GetComponent<HighstormScheduler>();
            if (scheduler == null) continue;
            if (scheduler.IsStormActive) continue;
            if (scheduler.TicksUntilNextStorm <= WarningThresholdTicks) {
                return AlertReport.Active;
            }
        }

        return AlertReport.Inactive;
    }

    public override TaggedString GetExplanation() {
        return "CR_Highstorm_Alert_Explanation".Translate();
    }

    private int GetTicksUntilNextStorm() {
        List<Map> maps = Find.Maps;
        for (int i = 0; i < maps.Count; i++) {
            HighstormScheduler scheduler = maps[i].GetComponent<HighstormScheduler>();
            if (scheduler == null) continue;
            if (scheduler.IsStormActive) continue;
            int ticks = scheduler.TicksUntilNextStorm;
            if (ticks <= WarningThresholdTicks) return ticks;
        }

        return -1;
    }
}