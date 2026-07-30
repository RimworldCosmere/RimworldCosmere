using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Logger = Cosmere.Core.Logger;

namespace Cosmere.Core.Quest.Objective;

/// <summary>
///     Places a site on the world map and completes when the player reaches it. The first
///     stage of every travel quest in the Scadrial arc.
/// </summary>
public class TravelToSiteObjective : QuestObjective {
    public int maxTiles = 27;
    public int minTiles = 7;
    public SitePartDef? sitePart;
    public float threatPoints = 300f;

    public override void AddParts(RimWorld.Quest quest, string inSignal, string outSignal, QuestBuildContext ctx) {
        if (!TryFindTile(ctx, out PlanetTile tile)) {
            Logger.Error($"TravelToSiteObjective on {ctx.def?.defName} found no tile. Quest aborted.");
            throw new QuestBuildFailure("no site tile available");
        }

        Faction? faction = ctx.def.targetFaction == null
            ? null
            : Find.FactionManager.FirstFactionOfDef(ctx.def.targetFaction);

        Site site = SiteMaker.MakeSite(
            sitePart,
            tile,
            faction,
            true,
            threatPoints,
            WorldObjectDefOf.Site
        );

        if (site == null) {
            throw new QuestBuildFailure("SiteMaker returned null");
        }

        Find.WorldObjects.Add(site);

        QuestPart_ArrivedAtSite arrived = new QuestPart_ArrivedAtSite {
            quest = quest,
            site = site,
            inSignalEnable = inSignal,
            outSignalsCompleted = new List<string> { outSignal },
        };
        quest.AddPart(arrived);
    }

    private bool TryFindTile(QuestBuildContext ctx, out PlanetTile tile) {
        return TileFinder.TryFindNewSiteTile(out tile, minTiles, maxTiles);
    }

    public override string? ConfigError() {
        if (sitePart == null) return "TravelToSiteObjective has no sitePart.";
        if (threatPoints < 0f) return "TravelToSiteObjective threatPoints cannot be negative.";
        if (minTiles < 0) return "TravelToSiteObjective minTiles cannot be negative.";
        if (maxTiles < minTiles) return "TravelToSiteObjective maxTiles is below minTiles.";
        return null;
    }
}
