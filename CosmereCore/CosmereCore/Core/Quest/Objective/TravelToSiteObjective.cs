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
    // Additional parts layered onto the site. A resource lump spawns no defenders on its own,
    // so garrisoning it means pairing it with something that wants threat points.
    public List<SitePartDef>? extraSiteParts;

    public int maxTiles = 27;
    public int minTiles = 7;

    // Must be a mineable rock def, not the resource item: GenStep_PreciousLump reads
    // .building.mineableThing off it to size the lump.
    public ThingDef? preciousLumpResources;

    // Site.Label falls back to MainSitePartDef.label, so without this the world map reads
    // "manhunter pack" instead of the place the quest is actually about.
    public string? siteLabelKey;
    public SitePartDef? sitePart;

    /// <summary>
    ///     Keeps the site on the world map for the life of the quest, re-posting it whenever
    ///     vanilla tears it down on departure. For objectives the player is meant to return to.
    /// </summary>
    public bool persistent;

    public float threatPoints = 300f;

    public override void AddParts(RimWorld.Quest quest, string inSignal, string outSignal, QuestBuildContext ctx) {
        if (!TryFindTile(ctx, out PlanetTile tile)) {
            Logger.Error($"TravelToSiteObjective on {ctx.def?.defName} found no tile. Quest aborted.");
            throw new QuestBuildFailure("no site tile available");
        }

        Faction? faction = ctx.def.targetFaction == null
            ? null
            : Find.FactionManager.FirstFactionOfDef(ctx.def.targetFaction);

        List<SitePartDef> siteParts = new List<SitePartDef>();
        if (sitePart != null) siteParts.Add(sitePart);

        // Added before the extras so PersistentSiteMapPatch sees it regardless of what else the
        // quest layers on.
        if (persistent) siteParts.Add(SitePartDefOf.Cosmere_SitePart_Persistent);
        if (extraSiteParts != null) {
            for (int i = 0; i < extraSiteParts.Count; i++) {
                SitePartDef? extra = extraSiteParts[i];
                if (extra == null) continue;

                // Outpost and friends build their base from map.ParentFaction. With no faction
                // the settlement resolver throws and the site generates empty, so skip rather
                // than ship a garrison with nobody in it.
                if (extra.requiresFaction && faction == null) {
                    Logger.Warning(
                        $"{ctx.def?.defName}: skipping site part '{extra.defName}' - it requires a " +
                        "faction and the quest's targetFaction is not present in this world."
                    );
                    continue;
                }

                siteParts.Add(extra);
            }
        }

        Site site = SiteMaker.MakeSite(
            siteParts,
            tile,
            faction,
            true,
            threatPoints,
            WorldObjectDefOf.Site
        );

        if (site == null) {
            throw new QuestBuildFailure("SiteMaker returned null");
        }

        // Site.MainSitePart is private, so the parms go through the public parts list.
        if (preciousLumpResources != null && site.parts != null && site.parts.Count > 0) {
            site.parts[0].parms.preciousLumpResources = preciousLumpResources;
        }

        if (siteLabelKey != null && siteLabelKey.Length > 0) {
            site.customLabel = siteLabelKey.Translate().Resolve();
        }

        // Quest.Notify_SignalReceived drops any tag that does not start with "Quest{id}.", and
        // outSignal already carries that prefix, so deriving the tag from it keeps the signal
        // routable and unique per stage.
        string siteTag = outSignal + ".Site";
        QuestUtility.AddQuestTag(ref site.questTags, siteTag);

        Find.WorldObjects.Add(site);

        QuestPart_ArrivedAtSite arrived = new QuestPart_ArrivedAtSite {
            quest = quest,
            site = site,
            arrivalSignal = siteTag + "." + QuestUtility.QuestTargetSignalPart_MapGenerated,
            inSignalEnable = inSignal,
            outSignalsCompleted = new List<string> { outSignal },
        };
        quest.AddPart(arrived);

        if (!persistent) return;

        // Enabled on the same signal as the arrival check and never completes, so it outlives
        // every later stage and can keep the site defended and tidy it up at the end.
        QuestPart_PersistentSite keeper = new QuestPart_PersistentSite {
            quest = quest,
            site = site,
            factionDef = faction?.def,
            threatPoints = threatPoints,
            inSignalEnable = inSignal,
        };
        quest.AddPart(keeper);
        arrived.keeper = keeper;
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
