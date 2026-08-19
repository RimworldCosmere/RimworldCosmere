using Cosmere.Core;
using Cosmere.Core.Quickstart;
using RimWorld;
using Verse;

namespace Cosmere.Core.Quickstart;

public class CosmereQuickstart : AbstractQuickstart {
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
    ///     All-Stars spans every shard, so its scenario cannot pick an era on its own. Set here so
    ///     era-gated content is reachable in the sandbox instead of silently filtered out.
    /// </summary>
    public override string? era => "Cosmere_Scadrial_Era_PreCatacendre";

    public override void PostApplyConfiguration() {
        Find.GameInitData.startingPawnCount = 16;
        List<ScenPart> parts = Find.Scenario.AllParts.ToList();
        for (int i = 0; i < parts.Count; i++) {
            if (parts[i] is ScenPart_ConfigPage_ConfigureStartingPawns startingPawns) {
                startingPawns.pawnCount = 16;
            }
        }
    }

    public override void PostStart() {
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
        switch (index) {
            case 0:
                pawn.GetInvestiture()!.currentInvestitureSelf = 500;
                QuickstartCharacterSetupRegistry.Apply(index, pawn);
                Find.Selector.Select(pawn, false);
                break;
            case 1:
                pawn.GetInvestiture()!.currentInvestitureSelf = 500;
                QuickstartCharacterSetupRegistry.Apply(index, pawn);
                break;
            case 2:
                pawn.GetInvestiture()!.currentInvestitureSelf = 500;
                QuickstartCharacterSetupRegistry.Apply(index, pawn);
                break;
            case 3:
                pawn.GetInvestiture()!.currentInvestitureSelf = 500;
                QuickstartCharacterSetupRegistry.Apply(index, pawn);
                break;
            case 4:
                pawn.GetInvestiture()!.currentInvestitureSelf = 500;
                QuickstartCharacterSetupRegistry.Apply(index, pawn);
                break;
            case 5:
                pawn.GetInvestiture()!.currentInvestitureSelf = 500;
                QuickstartCharacterSetupRegistry.Apply(index, pawn);
                break;
            case 6:
                pawn.GetInvestiture()!.currentInvestitureSelf = 500;
                QuickstartCharacterSetupRegistry.Apply(index, pawn);
                break;
            case 7:
                pawn.GetInvestiture()!.currentInvestitureSelf = 500;
                QuickstartCharacterSetupRegistry.Apply(index, pawn);
                break;
            case 8:
                pawn.GetInvestiture()!.currentInvestitureSelf = 500;
                QuickstartCharacterSetupRegistry.Apply(index, pawn);
                break;
            case 9:
                pawn.GetInvestiture()!.currentInvestitureSelf = 500;
                QuickstartCharacterSetupRegistry.Apply(index, pawn);
                break;
            case 10:
                pawn.GetInvestiture()!.currentInvestitureSelf = 500;
                QuickstartCharacterSetupRegistry.Apply(index, pawn);
                break;
            case 11:
                pawn.GetInvestiture()!.currentInvestitureSelf = 500;
                QuickstartCharacterSetupRegistry.Apply(index, pawn);
                break;
            case 12:
                pawn.GetInvestiture()!.currentInvestitureSelf = 500;
                QuickstartCharacterSetupRegistry.Apply(index, pawn);
                break;
            case 13:
                pawn.GetInvestiture()!.currentInvestitureSelf = 500;
                QuickstartCharacterSetupRegistry.Apply(index, pawn);
                break;
            case 14:
                pawn.GetInvestiture()!.currentInvestitureSelf = 500;
                QuickstartCharacterSetupRegistry.Apply(index, pawn);
                break;
            case 15:
                pawn.GetInvestiture()!.currentInvestitureSelf = 500;
                QuickstartCharacterSetupRegistry.Apply(index, pawn);
                break;
        }
    }
}
