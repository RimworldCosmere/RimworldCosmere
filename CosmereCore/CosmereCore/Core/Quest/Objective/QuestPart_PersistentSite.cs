using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Logger = Cosmere.Core.Logger;

namespace Cosmere.Core.Quest.Objective;

/// <summary>
///     Owns a persistent quest site: cleans it up when the quest ends, and keeps the enemy
///     pressure on it. PersistentSiteMapPatch stops the map being torn down, so the site's
///     defenders are not regenerated either - this posts fresh ones when the player returns and
///     keeps sending them while the player works.
/// </summary>
public class QuestPart_PersistentSite : QuestPart_CosmereActivable {
    /// <summary>Ticks between assaults while the player is working the site. 60000 is one day.</summary>
    public int raidIntervalTicks = 60000;

    public FactionDef? factionDef;
    public int lastRaidTick = -1;
    public bool playerWasPresent;
    public Site? site;
    public float threatPoints;

    /// <summary>Never satisfied on purpose: it runs until the quest ends and Cleanup tidies up.</summary>
    protected override bool IsSatisfied() {
        Site? current = site;
        Verse.Map? map = current == null || current.Destroyed ? null : current.Map;

        if (map == null) {
            playerWasPresent = false;
            return false;
        }

        bool present = map.mapPawns.FreeColonistsSpawnedCount > 0;

        // Arriving starts the clock, not an assault: it gives no time to unload before anyone notices.
        if (present && !playerWasPresent) {
            if (lastRaidTick < 0) lastRaidTick = Find.TickManager.TicksGame;
        } else if (present && Find.TickManager.TicksGame - lastRaidTick > raidIntervalTicks) {
            Assault(map, "is working");
        }

        playerWasPresent = present;
        return false;
    }

    private void Assault(Verse.Map map, string why) {
        IncidentParms parms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatBig, map);
        parms.forced = true;
        parms.points = threatPoints;

        Faction? faction = factionDef == null ? null : Find.FactionManager.FirstFactionOfDef(factionDef);
        if (faction != null && faction.HostileTo(Faction.OfPlayer)) parms.faction = faction;

        if (!IncidentDefOf.RaidEnemy.Worker.CanFireNow(parms)) return;

        IncidentDefOf.RaidEnemy.Worker.TryExecute(parms);
        lastRaidTick = Find.TickManager.TicksGame;
        Logger.Verbose($"QuestPart_PersistentSite: player {why} the site, sent {parms.points} points of raid.");
    }

    public override IEnumerable<GlobalTargetInfo> QuestLookTargets {
        get {
            foreach (GlobalTargetInfo target in base.QuestLookTargets) {
                yield return target;
            }

            if (site != null && !site.Destroyed) yield return site;
        }
    }

    /// <summary>
    ///     The patch stops the site tidying itself away, so the quest has to. Without this a
    ///     finished dig would sit on the world map with a live map behind it forever.
    /// </summary>
    public override void Cleanup() {
        base.Cleanup();
        if (site != null && !site.Destroyed) site.Destroy();
    }

    public override void ExposeData() {
        base.ExposeData();
        Scribe_References.Look(ref site, "site");
        Scribe_Defs.Look(ref factionDef, "factionDef");
        Scribe_Values.Look(ref threatPoints, "threatPoints");
        Scribe_Values.Look(ref raidIntervalTicks, "raidIntervalTicks", 90000);
        Scribe_Values.Look(ref lastRaidTick, "lastRaidTick", -1);
        Scribe_Values.Look(ref playerWasPresent, "playerWasPresent");
    }
}
