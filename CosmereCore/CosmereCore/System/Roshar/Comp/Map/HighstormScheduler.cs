using Cosmere.Core.Util;
using Cosmere.System.Roshar.GameCondition;
using RimWorld;
using UnityEngine;
using Verse;
using Logger = Cosmere.Core.Logger;

namespace Cosmere.System.Roshar.Comp.Map;

public class HighstormScheduler(Verse.Map map) : MapComponent(map) {
    private const int WeepingStartDay = WeepingSchedule.StartDay;
    private const int DaysPerYear = WeepingSchedule.DaysPerYear;

    private static readonly Color WeepingBlue = new Color(0.5f, 0.7f, 0.9f);

    private int lastHighstormTick = -1;

    private int nextHighstormTick = -1;
    private bool pushedPastWeeping;
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

        // dated from the storm's own tick, not now, since it is being pushed past the Weeping it landed in
        pushedPastWeeping = Mod.enableWeeping && IsTickDuringWeeping(nextHighstormTick);
        if (pushedPastWeeping) {
            int daysLeftInYear = WeepingSchedule.DaysToClearWeeping(DayOfYearAt(nextHighstormTick));
            nextHighstormTick += daysLeftInYear * GenDate.TicksPerDay +
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
        Scribe_Values.Look(ref pushedPastWeeping, "pushedPastWeeping");
    }

    public static string? GetStatusText(Verse.Map map) {
        HighstormScheduler scheduler = map.GetComponent<HighstormScheduler>();
        if (scheduler == null || scheduler.nextHighstormTick < 0) return null;

        if (scheduler.stormActive) return "CRO_Highstorm_Readout_Storm".Translate().Colorize(ColorLibrary.RedReadable);
        if (Mod.enableWeeping && IsWeeping(map)) return "CRO_Highstorm_Readout_Weeping".Translate().Colorize(WeepingBlue);

        int ticksLeft = scheduler.TicksUntilNextStorm;
        if (ticksLeft <= 0) return "CRO_Highstorm_Readout_Imminent".Translate().Colorize(ColorLibrary.RedReadable);

        string span = DescribeSpan(ticksLeft);

        // countdown jumps by weeks here: a storm rolled into the Weeping gets pushed past it
        return scheduler.pushedPastWeeping
            ? "CRO_Highstorm_Readout_AfterWeeping".Translate(span)
            : "CRO_Highstorm_Readout_Countdown".Translate(span);
    }

    public static string? GetTooltipText(Verse.Map map) {
        HighstormScheduler scheduler = map.GetComponent<HighstormScheduler>();
        if (scheduler == null || scheduler.nextHighstormTick < 0) return null;

        if (scheduler.stormActive) return "CRO_Highstorm_Tip_Storm".Translate();
        if (Mod.enableWeeping && IsWeeping(map)) return "CRO_Highstorm_Tip_Weeping".Translate();

        int ticksLeft = scheduler.TicksUntilNextStorm;
        if (ticksLeft <= 0) return "CRO_Highstorm_Tip_Imminent".Translate();

        string span = DescribeSpan(ticksLeft);
        string intensity = scheduler.seasonalIntensity.ToStringPercent();

        return scheduler.pushedPastWeeping
            ? "CRO_Highstorm_Tip_AfterWeeping".Translate(span, intensity)
            : "CRO_Highstorm_Tip_Countdown".Translate(span, intensity);
    }

    private static string DescribeSpan(int ticks) {
        float days = ticks / (float)GenDate.TicksPerDay;
        if (days >= 1f) return "CRO_Highstorm_Span_Days".Translate(days.ToString("F1"));

        float hours = ticks / (float)GenDate.TicksPerHour;
        return "CRO_Highstorm_Span_Hours".Translate(hours.ToString("F1"));
    }

    public static bool IsWeeping(Verse.Map map) {
        return WeepingSchedule.IsWeepingDay(GenLocalDate.DayOfYear(map));
    }

    /// <summary>
    ///     The map's local day of year at a future game tick.
    /// </summary>
    /// <remarks>
    ///     Not <c>tick / TicksPerDay % DaysPerYear</c>. That reads days since the game started,
    ///     which is a different calendar from the one <see cref="IsWeeping" /> uses: local dates
    ///     run off <c>TicksAbs</c> and carry both the starting date and a longitude offset. The two
    ///     disagreed by however far into a year the colony landed, so the Weeping could be dodged
    ///     on a day nowhere near it and push the next storm most of a year out.
    /// </remarks>
    /// <param name="tick">A game tick, on the same clock as <c>TickManager.TicksGame</c>.</param>
    /// <returns>The day of year, 0 to <see cref="DaysPerYear" /> - 1.</returns>
    private int DayOfYearAt(int tick) {
        TickManager ticks = Find.TickManager;
        long absTick = tick + (ticks.TicksAbs - ticks.TicksGame);
        return GenDate.DayOfYear(absTick, Find.WorldGrid.LongLatOf(map.Tile).x);
    }

    private bool IsTickDuringWeeping(int tick) {
        return WeepingSchedule.IsWeepingDay(DayOfYearAt(tick));
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
