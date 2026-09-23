using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace Cosmere.Core.Quest.Objective;

/// <summary>
///     Opens the next stage once the player has generated a map at the target site, which is
///     what arriving with a caravan does. Never fails the quest: a site that is gone was
///     either already visited or is unreachable, and neither is the player's fault.
/// </summary>
public class QuestPart_ArrivedAtSite : QuestPart_CosmereActivable {
    /// <summary>
    ///     Vanilla's MapGenerated signal for this site's quest tag. MapParent.PostMapGenerate
    ///     raises it the moment the caravan's map exists, which is authoritative in a way the
    ///     HasMap poll below is not - the poll only sees the site this part still points at.
    /// </summary>
    public string? arrivalSignal;

    /// <summary>
    ///     Set when the site is persistent. The keeper re-posts the site as a new object, so
    ///     this part has to read the live one rather than the reference it was built with.
    /// </summary>
    public QuestPart_PersistentSite? keeper;

    private bool sawMap;
    public Site? site;

    private Site? CurrentSite => keeper != null ? keeper.site : site;

    protected override bool IsSatisfied() {
        Site? current = CurrentSite;
        if (current == null) {
            Log.Warn("QuestPart_ArrivedAtSite: lost its site reference. Opening the stage.");
            return true;
        }

        if (current.HasMap) {
            if (!sawMap) {
                sawMap = true;
                Log.Debug($"QuestPart_ArrivedAtSite: map generated at tile {current.Tile}.");
            }

            return true;
        }

        if (current.Destroyed) {
            Log.Debug(
                $"QuestPart_ArrivedAtSite: site at tile {current.Tile} is gone (sawMap={sawMap}). Opening the stage."
            );
            return true;
        }

        return false;
    }

    /// <summary>
    ///     The site was spawned at offer time for a branch the player did not take, so nothing
    ///     will ever visit it. Remove it now rather than leaving it on the world map until the
    ///     quest ends.
    /// </summary>
    protected override void OnSkipped() {
        base.OnSkipped();
        Site? current = CurrentSite;
        if (current != null && !current.Destroyed && !current.HasMap) current.Destroy();
    }

    protected override void ProcessQuestSignal(Signal signal) {
        base.ProcessQuestSignal(signal);

        string? expected = arrivalSignal;
        if (expected == null || expected.Length == 0) return;
        if (signal.tag != expected) return;

        sawMap = true;
        Log.Debug($"QuestPart_ArrivedAtSite: arrival signal '{expected}' received.");
        Complete();
    }

    public override IEnumerable<GlobalTargetInfo> QuestLookTargets {
        get {
            foreach (GlobalTargetInfo target in base.QuestLookTargets) {
                yield return target;
            }

            Site? current = CurrentSite;
            if (current != null && !current.Destroyed) yield return current;
        }
    }

    /// <summary>
    ///     Removes the site from the world map if the quest ends before the player ever
    ///     reached it. TravelToSiteObjective spawns the site at offer time, before the player
    ///     accepts, so a dec‍lined, failed, or expired quest would otherwise leave it sitting on
    ///     the map forever. A site the player did reach (site.HasMap) is left alone - it
    ///     follows the normal MapParent.ShouldRemoveMapNow lifecycle instead.
    /// </summary>
    public override void Cleanup() {
        base.Cleanup();

        // A persistent site belongs to the keeper, which cleans it up itself.
        if (keeper != null) return;
        if (site != null && !site.Destroyed && !site.HasMap) site.Destroy();
    }

    public override void ExposeData() {
        base.ExposeData();
        Scribe_References.Look(ref site, "site");
        Scribe_References.Look(ref keeper, "keeper");
        Scribe_Values.Look(ref arrivalSignal, "arrivalSignal");
        Scribe_Values.Look(ref sawMap, "sawMap");
    }
}
