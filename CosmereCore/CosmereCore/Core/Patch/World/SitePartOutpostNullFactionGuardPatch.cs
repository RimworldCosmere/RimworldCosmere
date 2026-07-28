using Concord;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace Cosmere.Core.Patch;

[Patch]
public abstract class SitePartOutpostNullFactionGuardPatch : SitePartWorker_Outpost {
    [Inject(At.Head, nameof(GenerateDefaultParams))]
    private Control BeforeGenerateDefaultParams(
        float myThreatPoints,
        PlanetTile tile,
        Faction faction,
        ControlHandle<SitePartParams> ch
    ) {
        if (faction != null) return Control.Continue;

        SitePartParams sitePartParams = new SitePartParams {
            randomValue = Rand.Int,
            threatPoints = def.wantsThreatPoints ? myThreatPoints : 0f,
        };
        sitePartParams.lootMarketValue =
            SitePartWorker_Outpost.ThreatPointsLootMarketValue.Evaluate(sitePartParams.threatPoints);
        ch.ReturnValue = sitePartParams;
        return Control.Cancel;
    }
}
