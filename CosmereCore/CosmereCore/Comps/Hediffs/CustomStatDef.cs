using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Log = CosmereFramework.Log;

namespace CosmereCore.Comps.Hediffs {
    public class CustomStatDef : HediffComp {
        private List<StatDef> customStatDefsCache;

        protected virtual List<StatDef> customStatDefs => customStatDefsCache ??= DefDatabase<StatDef>.AllDefsListForReading.Where(x => x.defName.StartsWith("CosmereCore_")).ToList();

        public override void CompPostTick(ref float severityAdjustment) {
            base.CompPostTick(ref severityAdjustment);

            if (!Pawn.IsHashIntervalTick(GenTicks.TicksPerRealSecond)) return;

            foreach (var stat in customStatDefs) {
                var methodName = $"Handle{stat.defName.Replace("CosmereCore_", "")}";
                var method = GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);

                if (method != null) {
                    var value = Pawn.GetStatValue(stat);
                    method.Invoke(this, [value]);
                } else {
                    Log.Warning($"No handler for stat {stat.defName} defined in {GetType().Name}");
                }
            }
        }

        protected virtual void HandleMentalBreakAddFactor(float mentalBreakAddFactor) {
            if (parent.pawn.InMentalState) return;
            if (Rand.Value > mentalBreakAddFactor) return;

            var breakDef = MentalBreakDefOf.BerserkShort;
            if (!breakDef.Worker.BreakCanOccur(parent.pawn)) return;
            parent.pawn.jobs?.EndCurrentJob(JobCondition.InterruptForced);
            breakDef.Worker.TryStart(parent.pawn, "StatTrigger", true);

            if (LoadedModManager.RunningMods.Any(x => x.PackageId.Equals("cryptiklemur.cosmere.scadrial"))) {
                Pawn.skills.Learn(DefDatabase<SkillDef>.GetNamed("Cosmere_Allomantic_Power"), 100f);
            }
        }

        protected virtual void HandleMentalBreakRemoveFactor(float mentalBreakRemoveFactor) {
            if (!parent.pawn.InMentalState) return;
            if (Rand.Value > mentalBreakRemoveFactor) return;

            parent.pawn.mindState.mentalStateHandler.Reset();
            if (parent.pawn.InMentalState) return;
            parent.pawn.jobs?.EndCurrentJob(JobCondition.InterruptForced);
            MoteMaker.ThrowText(parent.pawn.DrawPos, parent.pawn.Map, "Calmed", Color.green);

            if (LoadedModManager.RunningMods.Any(x => x.PackageId.Equals("cryptiklemur.cosmere.scadrial"))) {
                Pawn.skills.Learn(DefDatabase<SkillDef>.GetNamed("Cosmere_Allomantic_Power"), 100f);
            }
        }
    }
}