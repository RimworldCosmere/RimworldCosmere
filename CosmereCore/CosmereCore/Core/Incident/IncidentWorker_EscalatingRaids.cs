using RimWorld;
using Verse;

namespace Cosmere.Core.Incident;

public class IncidentWorker_EscalatingRaids : IncidentWorker {
    protected override bool TryExecuteWorker(IncidentParms parms) {
        Map? map = parms.target as Map ?? Find.AnyPlayerHomeMap;
        if (map == null) return false;

        DefModExtension.EscalatingRaidConfig? config =
            def.GetModExtension<DefModExtension.EscalatingRaidConfig>();
        float multiplier = config?.pointMultiplier ?? 1.5f;

        IncidentDef raidDef = IncidentDefOf.RaidEnemy;
        IncidentParms raidParms = StorytellerUtility.DefaultParmsNow(raidDef.category, map);
        raidParms.points *= multiplier;

        if (config?.factionDef != null) {
            FactionDef? facDef = DefDatabase<FactionDef>.GetNamedSilentFail(config.factionDef);
            if (facDef != null) {
                Faction? faction = Find.FactionManager.FirstFactionOfDef(facDef);
                if (faction != null) raidParms.faction = faction;
            }
        }

        bool result = raidDef.Worker.TryExecute(raidParms);

        if (result) {
            SendStandardLetter(
                def.letterLabel,
                def.letterText,
                def.letterDef ?? LetterDefOf.ThreatBig,
                parms,
                null
            );
        }

        return result;
    }
}
