using Cosmere.Core;
using Cosmere.Core.Comp.Game;
using Cosmere.Core.Quickstart;
using RimWorld;
using Verse;

namespace Cosmere.Core.Quickstart;

public class CosmereQuickstart : AbstractQuickstart {
    public override int mapSize => 75;

    public override TaggedString description => "Cosmere All-Stars: Radiants + Mistborn + mundane";

    public override StorytellerDef storyteller => StorytellerDefOf.Cassandra;

    public override DifficultyDef difficulty => DifficultyDefOf.Easy;

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

        Shards? shards = Current.Game?.GetComponent<Shards>();
        if (shards != null) {
            shards.EnableShard("Preservation", true);
            shards.EnableShard("Ruin", true);
            shards.EnableShard("Honor", true);
            shards.EnableShard("Cultivation", true);
            shards.EnableShard("Odium", true);
        }

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
                pawn.Name = new NameSingle("Wit");
                pawn.gender = Gender.Male;
                pawn.story.bodyType = BodyTypeDefOf.Thin;
                pawn.GetInvestiture()!.currentInvestitureSelf = 500;
                QuickstartCharacterSetupRegistry.Apply(index, pawn);
                Find.Selector.Select(pawn, false);
                break;
            case 1:
                pawn.Name = new NameSingle("Dalinar");
                pawn.gender = Gender.Male;
                pawn.story.bodyType = BodyTypeDefOf.Male;
                pawn.GetInvestiture()!.currentInvestitureSelf = 500;
                QuickstartCharacterSetupRegistry.Apply(index, pawn);
                break;
            case 2:
                pawn.Name = new NameTriple("Kaladin", "Kal", "Stormblessed");
                pawn.gender = Gender.Male;
                pawn.story.bodyType = BodyTypeDefOf.Male;
                pawn.GetInvestiture()!.currentInvestitureSelf = 500;
                QuickstartCharacterSetupRegistry.Apply(index, pawn);
                break;
            case 3:
                pawn.Name = new NameSingle("Szeth");
                pawn.gender = Gender.Male;
                pawn.story.bodyType = BodyTypeDefOf.Male;
                pawn.GetInvestiture()!.currentInvestitureSelf = 500;
                QuickstartCharacterSetupRegistry.Apply(index, pawn);
                break;
            case 4:
                pawn.Name = new NameSingle("Vin");
                pawn.gender = Gender.Female;
                pawn.story.bodyType = BodyTypeDefOf.Female;
                pawn.GetInvestiture()!.currentInvestitureSelf = 500;
                QuickstartCharacterSetupRegistry.Apply(index, pawn);
                break;
            case 5:
                pawn.Name = new NameSingle("Malata");
                pawn.gender = Gender.Female;
                pawn.story.bodyType = BodyTypeDefOf.Female;
                pawn.GetInvestiture()!.currentInvestitureSelf = 500;
                QuickstartCharacterSetupRegistry.Apply(index, pawn);
                break;
            case 6:
                pawn.Name = new NameSingle("Lift");
                pawn.gender = Gender.Female;
                pawn.story.bodyType = BodyTypeDefOf.Thin;
                pawn.GetInvestiture()!.currentInvestitureSelf = 500;
                QuickstartCharacterSetupRegistry.Apply(index, pawn);
                break;
            case 7:
                pawn.Name = new NameSingle("Renarin");
                pawn.gender = Gender.Male;
                pawn.story.bodyType = BodyTypeDefOf.Thin;
                pawn.GetInvestiture()!.currentInvestitureSelf = 500;
                QuickstartCharacterSetupRegistry.Apply(index, pawn);
                break;
            case 8:
                pawn.Name = new NameSingle("Jasnah");
                pawn.gender = Gender.Female;
                pawn.story.bodyType = BodyTypeDefOf.Female;
                pawn.GetInvestiture()!.currentInvestitureSelf = 500;
                QuickstartCharacterSetupRegistry.Apply(index, pawn);
                break;
            case 9:
                pawn.Name = new NameSingle("Venli");
                pawn.gender = Gender.Female;
                pawn.story.bodyType = BodyTypeDefOf.Female;
                pawn.GetInvestiture()!.currentInvestitureSelf = 500;
                QuickstartCharacterSetupRegistry.Apply(index, pawn);
                break;
            case 10:
                pawn.Name = new NameSingle("Tsazo");
                pawn.gender = Gender.Male;
                pawn.story.bodyType = BodyTypeDefOf.Hulk;
                pawn.GetInvestiture()!.currentInvestitureSelf = 500;
                QuickstartCharacterSetupRegistry.Apply(index, pawn);
                break;
            case 11:
                pawn.Name = new NameSingle("Navani");
                pawn.gender = Gender.Female;
                pawn.story.bodyType = BodyTypeDefOf.Female;
                pawn.GetInvestiture()!.currentInvestitureSelf = 500;
                QuickstartCharacterSetupRegistry.Apply(index, pawn);
                break;
            case 12:
                pawn.Name = new NameSingle("Sazed");
                pawn.gender = Gender.Male;
                pawn.story.bodyType = BodyTypeDefOf.Male;
                pawn.GetInvestiture()!.currentInvestitureSelf = 500;
                QuickstartCharacterSetupRegistry.Apply(index, pawn);
                break;
            case 13:
                pawn.Name = new NameSingle("Rashek");
                pawn.gender = Gender.Male;
                pawn.story.bodyType = BodyTypeDefOf.Hulk;
                pawn.GetInvestiture()!.currentInvestitureSelf = 500;
                QuickstartCharacterSetupRegistry.Apply(index, pawn);
                break;
            case 14:
                pawn.Name = new NameTriple("Waxillium", "Wax", "Ladrian");
                pawn.gender = Gender.Male;
                pawn.story.bodyType = BodyTypeDefOf.Male;
                pawn.GetInvestiture()!.currentInvestitureSelf = 500;
                QuickstartCharacterSetupRegistry.Apply(index, pawn);
                break;
            case 15:
                pawn.Name = new NameSingle("Wayne");
                pawn.gender = Gender.Male;
                pawn.story.bodyType = BodyTypeDefOf.Thin;
                pawn.GetInvestiture()!.currentInvestitureSelf = 500;
                QuickstartCharacterSetupRegistry.Apply(index, pawn);
                break;
        }
    }
}
