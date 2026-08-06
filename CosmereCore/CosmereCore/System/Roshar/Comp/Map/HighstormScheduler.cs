using Cosmere.Core.Util;
using Cosmere.System.Roshar.GameCondition;
using RimWorld;
using UnityEngine;
using Verse;
using Logger = Cosmere.Core.Logger;

namespace Cosmere.System.Roshar.Comp.Map;

public class HighstormScheduler(Verse.Map map) : MapComponent(map) {
    private const int WeepingStartDay = 46;
    private const int DaysPerYear = 60;

    private int lastHighstormTick = -1;

    private int nextHighstormTick = -1;
    private float seasonalIntensity = 1f;
    private bool stormActive;
    private bool warningShown;
    private bool weepingActive;

    private bool enabled {
        get {
            if (!Mod.enableHighstorms) return false;
            return FeatureUtility.IsActive(FeatureDefOf.Cosmere_Feature_Highstorms);
        }
    }

    /// <summary>
    ///     Whether storms are running at all on this map. False on a world with no Honor, or
    ///     with highstorms switched off in settings.
    /// </summary>
    public bool IsScheduling => enabled;

    /// <summary>
    ///     Never negative. With no storm scheduled - which is every map on a world without
    ///     Honor - this is "not any time soon" rather than a countdown that has already run out,
    ///     because a raw nextHighstormTick of -1 reads as overdue to anything comparing against
    ///     a warning threshold.
    /// </summary>
    public int TicksUntilNextStorm =>
        !enabled || nextHighstormTick < 0 ? int.MaxValue : nextHighstormTick - Find.TickManager.TicksGame;

    public bool IsStormActive => stormActive;

    public float SeasonalIntensity => seasonalIntensity;

    public override void FinalizeInit() {
        if (!enabled) return;

        base.FinalizeInit();

        if (stormActive) {
            Highstorm? activeCondition = null;
            List<RimWorld.GameCondition> conditions = map.gameConditionManager.ActiveConditions;
            for (int i = 0; i < conditions.Count; i++) {
                if (conditions[i] is Highstorm hs) {
                    activeCondition = hs;
                    break;
                }
            }

            if (activeCondition == null) {
                stormActive = false;
            }
        }

        if (nextHighstormTick < 0) {
            ScheduleNextHighstorm();
        }
    }

    public override void MapComponentTick() {
        if (!enabled) return;

        if (nextHighstormTick < 0) {
            ScheduleNextHighstorm();
        }

        int currentTick = Find.TickManager.TicksGame;

        if (stormActive) {
            List<RimWorld.GameCondition> conditions = map.gameConditionManager.ActiveConditions;
            bool found = false;
            for (int i = 0; i < conditions.Count; i++) {
                if (conditions[i] is Highstorm) {
                    found = true;
                    break;
                }
            }

            if (!found) {
                stormActive = false;
            }

            return;
        }

        if (Mod.enableWeeping) {
            bool currentlyWeeping = IsWeeping(map);
            if (currentlyWeeping && !weepingActive) {
                weepingActive = true;
                Find.LetterStack.ReceiveLetter(
                    "CR_Weeping_Start_Title".Translate(),
                    "CR_Weeping_Start_Message".Translate(),
                    RimWorld.LetterDefOf.NeutralEvent,
                    TargetInfo.Invalid
                );
            } else if (!currentlyWeeping && weepingActive) {
                weepingActive = false;
                Find.LetterStack.ReceiveLetter(
                    "CR_Weeping_End_Title".Translate(),
                    "CR_Weeping_End_Message".Translate(),
                    RimWorld.LetterDefOf.NeutralEvent,
                    TargetInfo.Invalid
                );
            }

            if (currentlyWeeping) return;
        }

        if (!warningShown && currentTick >= nextHighstormTick - GenDate.TicksPerDay) {
            SendWarning();
            warningShown = true;
        }

        if (currentTick >= nextHighstormTick) {
            TriggerHighstorm();
        }
    }

    private void ScheduleNextHighstorm() {
        int minDays = Mod.highstormMinIntervalDays;
        int maxDays = Mod.highstormMaxIntervalDays;
        int intervalTicks = Rand.Range(minDays, maxDays + 1) * GenDate.TicksPerDay;
        int ticksNow = Find.TickManager.TicksGame;

        if (lastHighstormTick < 0) {
            nextHighstormTick = ticksNow + maxDays * GenDate.TicksPerDay;
            lastHighstormTick = nextHighstormTick - intervalTicks;
            warningShown = false;
            seasonalIntensity = GetSeasonalIntensityMultiplier(map);
            Logger.Verbose(
                $"First storm scheduled at tick {nextHighstormTick} ({maxDays} days from now)"
            );
            return;
        }

        nextHighstormTick = lastHighstormTick + intervalTicks;

        if (Mod.enableWeeping && IsTickDuringWeeping(nextHighstormTick)) {
            int dayOfYear = GenLocalDate.DayOfYear(map);
            int daysUntilNewYear = DaysPerYear - dayOfYear;
            nextHighstormTick = ticksNow +
                                daysUntilNewYear * GenDate.TicksPerDay +
                                Rand.Range(0, 2) * GenDate.TicksPerDay;
        }

        warningShown = false;
        seasonalIntensity = GetSeasonalIntensityMultiplier(map);

        Logger.Verbose(
            $"Next scheduled at tick {nextHighstormTick} (interval {intervalTicks}, last at {lastHighstormTick})"
        );
    }

    private void TriggerHighstorm() {
        IncidentParms parms = new IncidentParms {
            target = map,
            forced = true,
        };

        IncidentDef incidentDef = DefDatabase<IncidentDef>.GetNamed("Cosmere_Roshar_HighstormIncident");
        bool success = incidentDef.Worker.TryExecute(parms);

        if (success) {
            stormActive = true;
        } else {
            Logger.Warning("Incident failed to execute, will retry next schedule.");
        }

        lastHighstormTick = Find.TickManager.TicksGame;
        ScheduleNextHighstorm();
    }

    private void SendWarning() {
        Find.LetterStack.ReceiveLetter(
            "CR_Highstorm_Warning_Title".Translate(),
            "CR_Highstorm_Warning_Message".Translate(),
            RimWorld.LetterDefOf.ThreatSmall,
            TargetInfo.Invalid
        );
    }

    public override void ExposeData() {
        Scribe_Values.Look(ref nextHighstormTick, "nextHighstormTick", -1);
        Scribe_Values.Look(ref lastHighstormTick, "lastHighstormTick", -1);
        Scribe_Values.Look(ref stormActive, "stormActive");
        Scribe_Values.Look(ref warningShown, "warningShown");
        Scribe_Values.Look(ref seasonalIntensity, "seasonalIntensity", 1f);
        Scribe_Values.Look(ref weepingActive, "weepingActive");
    }

    public static string? GetStatusText(Verse.Map map) {
        HighstormScheduler scheduler = map.GetComponent<HighstormScheduler>();
        if (scheduler == null || scheduler.nextHighstormTick < 0) return null;

        if (scheduler.stormActive) return "Highstorm".Colorize(ColorLibrary.RedReadable);
        if (Mod.enableWeeping && IsWeeping(map)) return "The Weeping".Colorize(new Color(0.5f, 0.7f, 0.9f));

        int ticksLeft = scheduler.TicksUntilNextStorm;
        if (ticksLeft <= 0) return "Stormwall Imminent".Colorize(ColorLibrary.RedReadable);

        float days = ticksLeft / (float)GenDate.TicksPerDay;
        if (days >= 1f) return $"Highstorm: {days:F1} days";

        float hours = ticksLeft / (float)GenDate.TicksPerHour;
        return $"Highstorm: {hours:F1} hours";
    }

    public static string? GetTooltipText(Verse.Map map) {
        HighstormScheduler scheduler = map.GetComponent<HighstormScheduler>();
        if (scheduler == null || scheduler.nextHighstormTick < 0) return null;

        if (scheduler.stormActive) {
            return
                "A highstorm rages across the land. The winds carry stones and debris from the east, scouring everything unsheltered.";
        }

        if (Mod.enableWeeping && IsWeeping(map)) {
            return
                "The Weeping has settled over the land. Constant light rain falls, but no highstorms will come until it passes.";
        }

        int ticksLeft = scheduler.TicksUntilNextStorm;
        if (ticksLeft <= 0) return "The stormwall draws near. Those caught in the open will not survive.";

        float days = ticksLeft / (float)GenDate.TicksPerDay;
        float hours = ticksLeft / (float)GenDate.TicksPerHour;
        string timeStr = days >= 1f ? $"{days:F1} days" : $"{hours:F1} hours";
        return
            $"The next highstorm will arrive in approximately {timeStr}.\nHighstorms sweep from east to west, carrying debris with enough force to shatter bone. Seek shelter behind solid eastern walls.\nSeasonal intensity: {scheduler.seasonalIntensity:P0}";
    }

    public static bool IsWeeping(Verse.Map map) {
        return GenLocalDate.DayOfYear(map) >= WeepingStartDay;
    }

    private bool IsTickDuringWeeping(int tick) {
        long absTick = tick;
        int dayOfYear = (int)(absTick / GenDate.TicksPerDay % DaysPerYear);
        return dayOfYear >= WeepingStartDay;
    }

    private static float GetSeasonalIntensityMultiplier(Verse.Map map) {
        Season season = GenLocalDate.Season(map);
        return season switch {
            Season.Spring => 1.0f,
            Season.Summer => 1.3f,
            Season.Fall => 0.8f,
            Season.Winter => 0.5f,
            _ => 1.0f,
        };
    }
}
