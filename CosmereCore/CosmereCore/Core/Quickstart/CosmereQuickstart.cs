using Cosmere.Core.Comp.Game;
using Cosmere.Core.Comp.Thing;
using Cosmere.Core.Def;
using Cosmere.System.Roshar.Gene;
using Cosmere.System.Scadrial.Def;
using Cosmere.System.Scadrial.Dev;
using RimWorld;
using ScadrialThingDefOf = Cosmere.System.Scadrial.ThingDefOf;
using Verse;
using RosharGeneDefOf = Cosmere.System.Roshar.GeneDefOf;
using ScadrialGeneUtility = Cosmere.System.Scadrial.Utility.GeneUtility;
using ScadrialRecordDefOf = Cosmere.System.Scadrial.RecordDefOf;
using ScadrialGeneDefOf = Cosmere.System.Scadrial.GeneDefOf;

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
        Shards? shards = Current.Game?.GetComponent<Shards>();
        if (shards != null) {
            shards.EnableShard("Preservation", allowConflicts: true);
            shards.EnableShard("Ruin", allowConflicts: true);
            shards.EnableShard("Honor", allowConflicts: true);
            shards.EnableShard("Cultivation", allowConflicts: true);
            shards.EnableShard("Odium", allowConflicts: true);
        }
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

        if (pawns.TryPopFront(out Pawn pawn)) {
            pawn.Name = new NameSingle("Wit");
            pawn.gender = Gender.Male;
            pawn.story.bodyType = BodyTypeDefOf.Thin;
            pawn.genes.TryAddRadiantOrder(RosharGeneDefOf.Cosmere_Roshar_Gene_RadiantLightweaver, 5);
            ScadrialGeneUtility.AddMistborn(pawn, false, true);
            pawn.skills.GetSkill(System.Roshar.SkillDefOf.Cosmere_Roshar_Skill_SurgebindingPower).Level = 15;
            pawn.GetInvestiture().currentInvestitureSelf = 500;
            Find.Selector.Select(pawn, false);
        }

        if (pawns.TryPopFront(out pawn)) {
            pawn.Name = new NameSingle("Dalinar");
            pawn.gender = Gender.Male;
            pawn.story.bodyType = BodyTypeDefOf.Male;
            Surgebinder? bondsmith = pawn.genes.TryAddRadiantOrder(RosharGeneDefOf.Cosmere_Roshar_Gene_RadiantBondsmith, 5);
            if (bondsmith != null) {
                bondsmith.godsprenName = "Stormfather";
                bondsmith.UpdateAbilities();
            }
            pawn.skills.GetSkill(System.Roshar.SkillDefOf.Cosmere_Roshar_Skill_SurgebindingPower).Level = 15;
            pawn.records.AddTo(System.Roshar.RecordDefOf.Cosmere_Roshar_Record_FriendshipsFormed, 25);
            pawn.GetInvestiture().currentInvestitureSelf = 500;
        }

        if (pawns.TryPopFront(out pawn)) {
            pawn.Name = new NameTriple("Kaladin", "Kal", "Stormblessed");
            pawn.gender = Gender.Male;
            pawn.story.bodyType = BodyTypeDefOf.Male;
            pawn.genes.TryAddRadiantOrder(RosharGeneDefOf.Cosmere_Roshar_Gene_RadiantWindrunner, 5);
            pawn.skills.GetSkill(System.Roshar.SkillDefOf.Cosmere_Roshar_Skill_SurgebindingPower).Level = 20;
            pawn.GetInvestiture().currentInvestitureSelf = 500;
        }

        if (pawns.TryPopFront(out pawn)) {
            pawn.Name = new NameSingle("Szeth");
            pawn.gender = Gender.Male;
            pawn.story.bodyType = BodyTypeDefOf.Male;
            pawn.genes.TryAddRadiantOrder(RosharGeneDefOf.Cosmere_Roshar_Gene_RadiantSkybreaker, 5);
            pawn.skills.GetSkill(System.Roshar.SkillDefOf.Cosmere_Roshar_Skill_SurgebindingPower).Level = 15;
            pawn.records.AddTo(System.Roshar.RecordDefOf.Cosmere_Roshar_Record_ArrestsMade, 5);
            pawn.records.AddTo(System.Roshar.RecordDefOf.Cosmere_Roshar_Record_ZoneComplianceDays, 30);
            pawn.GetInvestiture().currentInvestitureSelf = 500;
        }

        if (pawns.TryPopFront(out pawn)) {
            pawn.Name = new NameSingle("Vin");
            pawn.gender = Gender.Female;
            pawn.story.bodyType = BodyTypeDefOf.Female;
            ScadrianUtility.PrepareDevPawn(pawn);
            pawn.GetInvestiture().currentInvestitureSelf = 500;
        }

        if (pawns.TryPopFront(out pawn)) {
            pawn.Name = new NameSingle("Malata");
            pawn.gender = Gender.Female;
            pawn.story.bodyType = BodyTypeDefOf.Female;
            pawn.genes.TryAddRadiantOrder(RosharGeneDefOf.Cosmere_Roshar_Gene_RadiantDustbringer, 5);
            pawn.skills.GetSkill(System.Roshar.SkillDefOf.Cosmere_Roshar_Skill_SurgebindingPower).Level = 15;
            pawn.GetInvestiture().currentInvestitureSelf = 500;
        }

        if (pawns.TryPopFront(out pawn)) {
            pawn.Name = new NameSingle("Lift");
            pawn.gender = Gender.Female;
            pawn.story.bodyType = BodyTypeDefOf.Thin;
            pawn.genes.TryAddRadiantOrder(RosharGeneDefOf.Cosmere_Roshar_Gene_RadiantEdgedancer, 5);
            Surgebinder? liftBondsmith = pawn.genes.TryAddRadiantOrder(RosharGeneDefOf.Cosmere_Roshar_Gene_RadiantBondsmith, 3);
            if (liftBondsmith != null) {
                liftBondsmith.godsprenName = "Nightwatcher";
                liftBondsmith.UpdateAbilities();
            }
            pawn.skills.GetSkill(System.Roshar.SkillDefOf.Cosmere_Roshar_Skill_SurgebindingPower).Level = 15;
            pawn.GetInvestiture().currentInvestitureSelf = 500;
        }

        if (pawns.TryPopFront(out pawn)) {
            pawn.Name = new NameSingle("Renarin");
            pawn.gender = Gender.Male;
            pawn.story.bodyType = BodyTypeDefOf.Thin;
            pawn.genes.TryAddRadiantOrder(RosharGeneDefOf.Cosmere_Roshar_Gene_RadiantTruthwatcher, 5);
            pawn.skills.GetSkill(System.Roshar.SkillDefOf.Cosmere_Roshar_Skill_SurgebindingPower).Level = 15;
            pawn.GetInvestiture().currentInvestitureSelf = 500;
        }

        if (pawns.TryPopFront(out pawn)) {
            pawn.Name = new NameSingle("Jasnah");
            pawn.gender = Gender.Female;
            pawn.story.bodyType = BodyTypeDefOf.Female;
            pawn.genes.TryAddRadiantOrder(RosharGeneDefOf.Cosmere_Roshar_Gene_RadiantElsecaller, 5);
            pawn.skills.GetSkill(System.Roshar.SkillDefOf.Cosmere_Roshar_Skill_SurgebindingPower).Level = 15;
            pawn.GetInvestiture().currentInvestitureSelf = 500;
        }

        if (pawns.TryPopFront(out pawn)) {
            pawn.Name = new NameSingle("Venli");
            pawn.gender = Gender.Female;
            pawn.story.bodyType = BodyTypeDefOf.Female;
            pawn.genes.TryAddRadiantOrder(RosharGeneDefOf.Cosmere_Roshar_Gene_RadiantWillshaper, 5);
            pawn.skills.GetSkill(System.Roshar.SkillDefOf.Cosmere_Roshar_Skill_SurgebindingPower).Level = 15;
            pawn.GetInvestiture().currentInvestitureSelf = 500;
        }

        if (pawns.TryPopFront(out pawn)) {
            pawn.Name = new NameSingle("Tsazo");
            pawn.gender = Gender.Male;
            pawn.story.bodyType = BodyTypeDefOf.Hulk;
            pawn.genes.TryAddRadiantOrder(RosharGeneDefOf.Cosmere_Roshar_Gene_RadiantStoneward, 5);
            pawn.skills.GetSkill(System.Roshar.SkillDefOf.Cosmere_Roshar_Skill_SurgebindingPower).Level = 15;
            pawn.GetInvestiture().currentInvestitureSelf = 500;
        }

        if (pawns.TryPopFront(out pawn)) {
            pawn.Name = new NameSingle("Navani");
            pawn.gender = Gender.Female;
            pawn.story.bodyType = BodyTypeDefOf.Female;
            Surgebinder? navani = pawn.genes.TryAddRadiantOrder(RosharGeneDefOf.Cosmere_Roshar_Gene_RadiantBondsmith, 3);
            if (navani != null) {
                navani.godsprenName = "Sibling";
                navani.UpdateAbilities();
            }
            pawn.skills.GetSkill(System.Roshar.SkillDefOf.Cosmere_Roshar_Skill_SurgebindingPower).Level = 15;
            pawn.GetInvestiture().currentInvestitureSelf = 500;
        }

        if (pawns.TryPopFront(out pawn)) {
            pawn.Name = new NameSingle("Sazed");
            pawn.gender = Gender.Male;
            pawn.story.bodyType = BodyTypeDefOf.Male;
            ScadrialGeneUtility.AddFullFeruchemist(pawn, false, true);
            foreach (MetallicArtsMetalDef metal in DefDatabase<MetallicArtsMetalDef>.AllDefsListForReading) {
                if (metal.feruchemy?.userName == null) continue;
                Verse.Thing metalmind = ThingMaker.MakeThing(ScadrialThingDefOf.Cosmere_Scadrial_Thing_MetalmindBand, metal.Item);
                pawn.inventory.innerContainer.TryAdd(metalmind);
            }
            pawn.GetInvestiture().currentInvestitureSelf = 500;
        }

        if (pawns.TryPopFront(out pawn)) {
            pawn.Name = new NameSingle("Rashek");
            pawn.gender = Gender.Male;
            pawn.story.bodyType = BodyTypeDefOf.Hulk;
            ScadrialGeneUtility.AddMistborn(pawn, false, true);
            ScadrialGeneUtility.AddFullFeruchemist(pawn, false, true);
            pawn.records.Increment(ScadrialRecordDefOf.Cosmere_Scadrial_Record_IngestedLerasium);
            pawn.records.Increment(ScadrialRecordDefOf.Cosmere_Scadrial_Record_IngestedLeratium);
            foreach (MetallicArtsMetalDef metal in DefDatabase<MetallicArtsMetalDef>.AllDefsListForReading) {
                if (metal.feruchemy?.userName == null) continue;
                Verse.Thing metalmind = ThingMaker.MakeThing(ScadrialThingDefOf.Cosmere_Scadrial_Thing_MetalmindBand, metal.Item);
                pawn.inventory.innerContainer.TryAdd(metalmind);
            }
            pawn.GetInvestiture().currentInvestitureSelf = 500;
        }

        if (pawns.TryPopFront(out pawn)) {
            pawn.Name = new NameTriple("Waxillium", "Wax", "Ladrian");
            pawn.gender = Gender.Male;
            pawn.story.bodyType = BodyTypeDefOf.Male;
            ScadrialGeneUtility.AddGene(pawn, ScadrialGeneDefOf.GetMistingGeneForMetal(MetalDefOf.Steel), false, true);
            ScadrialGeneUtility.AddGene(pawn, ScadrialGeneDefOf.GetFerringGeneForMetal(MetalDefOf.Iron), false, true);
            pawn.inventory.innerContainer.TryAdd(
                ThingMaker.MakeThing(ScadrialThingDefOf.Cosmere_Scadrial_Thing_MetalmindBand, MetalDefOf.Iron.Item)
            );
            pawn.GetInvestiture().currentInvestitureSelf = 500;
        }

        if (pawns.TryPopFront(out pawn)) {
            pawn.Name = new NameSingle("Wayne");
            pawn.gender = Gender.Male;
            pawn.story.bodyType = BodyTypeDefOf.Thin;
            ScadrialGeneUtility.AddGene(pawn, ScadrialGeneDefOf.GetMistingGeneForMetal(MetalDefOf.Bendalloy), false, true);
            ScadrialGeneUtility.AddGene(pawn, ScadrialGeneDefOf.GetFerringGeneForMetal(MetalDefOf.Gold), false, true);
            pawn.inventory.innerContainer.TryAdd(
                ThingMaker.MakeThing(ScadrialThingDefOf.Cosmere_Scadrial_Thing_MetalmindBand, MetalDefOf.Gold.Item)
            );
            pawn.GetInvestiture().currentInvestitureSelf = 500;
        }
    }
}
