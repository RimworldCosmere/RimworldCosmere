using Cosmere.Core;
using Cosmere.Core.Quickstart;
using RimWorld;
using Verse;

namespace Cosmere.Core.Quickstart;

public class CosmereQuickstart : CosmereQuickstartBase {
    public override int mapSize => 75;

    public override TaggedString description => "Cosmere All-Stars: Radiants + Mistborn + mundane";

    public override StorytellerDef storyteller => StorytellerDefOf.Cassandra;

    public override DifficultyDef difficulty => DifficultyDefOf.Easy;

    public override IReadOnlyList<string> shards => [
        "Preservation",
        "Ruin",
        "Honor",
        "Cultivation",
        "Odium",
    ];

    /// <summary>
    ///     Gets the era to force. All-Stars spans every shard, so its scenario cannot pick one on its
    ///     own, and era-gated content would be filtered out of the sandbox instead of reachable.
    /// </summary>
    public override string? era => "Cosmere_Scadrial_Era_PreCatacendre";

    public override void PostApplyConfiguration() {
        Find.GameInitData.startingPawnCount = 16;
        List<ScenPart> parts = [.. Find.Scenario.AllParts];
        for (int i = 0; i < parts.Count; i++) {
            if (parts[i] is ScenPart_ConfigPage_ConfigureStartingPawns startingPawns) {
                startingPawns.pawnCount = 16;
            }
        }
    }

    public override void PostStart() {
        EnableDebugOverlays();
    }

    private static void EnableDebugOverlays() {
        DebugSettings.godMode = true;
        DebugViewSettings.showFpsCounter = true;
        DebugViewSettings.showTpsCounter = true;
        DebugViewSettings.showMemoryInfo = true;
    }

    public override void PostLoaded() {
        Current.Game?.researchManager.DebugSetAllProjectsFinished();
    }

    public override void PrepareColonists(List<Pawn> pawns) {
        if (pawns.Count == 0) return;

        BackstoryDef child = DefDatabase<BackstoryDef>.GetNamed("OptimisticChild30");
        BackstoryDef adult = DefDatabase<BackstoryDef>.GetNamed("CivilEngineer2");
        for (int i = 0; i < pawns.Count; i++) {
            pawns[i].story.Childhood = child;
            pawns[i].story.Adulthood = adult;
            if (pawns[i].playerSettings != null) {
                pawns[i].playerSettings.hostilityResponse = HostilityResponseMode.Attack;
            }
        }

        int pawnIndex = 0;
        while (pawns.TryPopFront(out Pawn? pawn)) {
            SetupCharacter(pawnIndex, pawn);
            pawnIndex++;
        }
    }

    private static void SetupCharacter(int index, Pawn pawn) {
        if (index is < 0 or > 15) return;

        pawn.GetInvestiture()!.currentInvestitureSelf = 500;
        QuickstartCharacterSetupRegistry.Apply(index, pawn);
        if (index == 0) {
            Find.Selector.Select(pawn, false);
        }
    }
}
