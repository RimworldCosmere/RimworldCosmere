using System;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Cosmere.Core.Comp.Hediff;

public class MentalBreakHandlerProperties : HediffCompProperties {
    public MentalBreakHandlerProperties() {
        compClass = typeof(MentalBreakHandler);
    }
}

public class MentalBreakHandler : HediffComp {
    private const string StatPrefix = "Cosmere_Mental_Break_";
    private static List<StatDef>? customStatDefsCache;
    private static bool cacheInitialized;

    private static readonly Dictionary<string, Action<MentalBreakHandler, float>> Handlers =
        new Dictionary<string, Action<MentalBreakHandler, float>> {
            ["Add_Factor"] = (h, v) => h.HandleMentalBreakAddFactor(v),
            ["Remove_Factor"] = (h, v) => h.HandleMentalBreakRemoveFactor(v),
        };

    private static List<StatDef> customStatDefs {
        get {
            if (!cacheInitialized) {
                customStatDefsCache = [];
                List<StatDef> allStats = DefDatabase<StatDef>.AllDefsListForReading;
                for (int i = 0; i < allStats.Count; i++) {
                    if (allStats[i].defName.StartsWith(StatPrefix)) {
                        customStatDefsCache.Add(allStats[i]);
                    }
                }

                cacheInitialized = true;
            }

            return customStatDefsCache!;
        }
    }

    public override void CompPostTick(ref float severityAdjustment) {
        base.CompPostTick(ref severityAdjustment);

        if (customStatDefs.Count == 0) return;
        if (!Pawn.IsHashIntervalTick(GenTicks.TicksPerRealSecond)) return;

        for (int i = 0; i < customStatDefs.Count; i++) {
            StatDef stat = customStatDefs[i];
            string handlerName = stat.defName.Replace(StatPrefix, string.Empty);
            if (Handlers.TryGetValue(handlerName, out Action<MentalBreakHandler, float>? handler)) {
                float value = Pawn.GetStatValue(stat);
                handler(this, value);
            }
        }
    }

    protected virtual void HandleMentalBreakAddFactor(float mentalBreakAddFactor) {
        if (parent.pawn.InMentalState) return;
        if (Rand.Value > mentalBreakAddFactor) return;

        MentalBreakDef? breakDef = MentalBreakDefOf.Berserk;
        if (!breakDef.Worker.BreakCanOccur(parent.pawn)) return;
        parent.pawn.jobs?.EndCurrentJob(JobCondition.InterruptForced);
        breakDef.Worker.TryStart(parent.pawn, "StatTrigger", true);
    }

    protected virtual void HandleMentalBreakRemoveFactor(float mentalBreakRemoveFactor) {
        if (!parent.pawn.InMentalState) return;
        if (Rand.Value > mentalBreakRemoveFactor) return;

        parent.pawn.mindState.mentalStateHandler.Reset();
        if (parent.pawn.InMentalState) return;
        parent.pawn.jobs?.EndCurrentJob(JobCondition.InterruptForced);
        MoteMaker.ThrowText(parent.pawn.DrawPos, parent.pawn.Map, "Calmed", Color.green);
    }
}
