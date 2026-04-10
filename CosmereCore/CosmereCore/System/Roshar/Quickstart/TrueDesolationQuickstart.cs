using Cosmere.Core.Comp.Thing;
using Cosmere.Core.Def;
using Cosmere.Core.Quickstart;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Quickstart;

public class TrueDesolationQuickstart : AbstractQuickstart {
    //public override ScenarioDef? scenario => ScenarioDefOf.Cosmere_Scadrial_PreCatacendre;

    public override int mapSize => 50;

    public override TaggedString description => "Used to test True Desolation pawns";

    public override StorytellerDef storyteller => StorytellerDefOf.Cassandra;

    public override DifficultyDef difficulty => DifficultyDefOf.Easy;

    public override void PostApplyConfiguration() {
        Find.GameInitData.startingPawnCount = 5;
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
        }

        foreach (GemDef gemDef in DefDatabase<GemDef>.AllDefsListForReading) {
            Verse.Thing gem = ThingMaker.MakeThing(ThingDefOf.Cosmere_Roshar_Thing_Mark, gemDef.Item);
            gem.stackCount = 25;
            gem.TryGetComp<InvestitureHolder>().FillInvestiture();
            GenPlace.TryPlaceThing(gem, pawns[0].Position, pawns[0].Map, ThingPlaceMode.Near);
        }

        if (pawns.TryPopFront(out Pawn pawn)) {
            pawn.Name = new NameTriple("Kaladin", "Kal", "Stormblessed");
            pawn.gender = Gender.Male;
            pawn.story.bodyType = BodyTypeDefOf.Male;
            pawn.genes.TryAddRadiantOrder(GeneDefOf.Cosmere_Roshar_Gene_RadiantWindrunner, 4);
            pawn.skills.GetSkill(SkillDefOf.Cosmere_Roshar_Skill_SurgebindingPower).Level = 20;

            Apparel pouch = (Apparel)ThingMaker.MakeThing(
                ThingDefOf.Cosmere_Roshar_Apparel_SpherePouch,
                GenStuff.RandomStuffFor(ThingDefOf.Cosmere_Roshar_Apparel_SpherePouch)
            );
            Verse.Thing broam = ThingMaker.MakeThing(
                ThingDefOf.Cosmere_Roshar_Thing_Broam,
                Core.ThingDefOf.RawEmerald
            );
            if (broam.TryGetComp(out InvestitureHolder broamInvestiture)) {
                broamInvestiture.currentInvestitureSelf = broamInvestiture.maxInvestitureSelf;
            }

            pouch.TryGetComp<InnerStorage>().innerContainer!.TryAdd(broam);
            pawn.apparel.Wear(pouch);
            pawn.GetInvestiture().currentInvestitureSelf = 1000;

            Find.Selector.Select(pawn, false);
        }

        if (pawns.TryPopFront(out pawn)) {
            pawn.Name = new NameSingle("Szeth");
            pawn.gender = Gender.Male;
            pawn.story.bodyType = BodyTypeDefOf.Male;
            pawn.story.traits.GainTrait(new Trait(RimWorld.TraitDefOf.Pyromaniac));
            pawn.genes.TryAddRadiantOrder(GeneDefOf.Cosmere_Roshar_Gene_RadiantSkybreaker, 1);
            pawn.skills.GetSkill(SkillDefOf.Cosmere_Roshar_Skill_SurgebindingPower).Level = 10;
            pawn.records.AddTo(RecordDefOf.Cosmere_Roshar_Record_ArrestsMade, 5);
            pawn.records.AddTo(RecordDefOf.Cosmere_Roshar_Record_ZoneComplianceDays, 30);
            pawn.GetInvestiture().currentInvestitureSelf = 1000;
        }

        if (pawns.TryPopFront(out pawn)) {
            pawn.Name = new NameSingle("Malata");
            pawn.gender = Gender.Female;
            pawn.story.bodyType = BodyTypeDefOf.Female;
            pawn.story.traits.GainTrait(new Trait(RimWorld.TraitDefOf.Pyromaniac));
            pawn.genes.TryAddRadiantOrder(GeneDefOf.Cosmere_Roshar_Gene_RadiantDustbringer, 0);
            pawn.skills.GetSkill(SkillDefOf.Cosmere_Roshar_Skill_SurgebindingPower).Level = 5;
            pawn.records.AddTo(RimWorld.RecordDefOf.KillsHumanlikes, 10);
            pawn.GetInvestiture().currentInvestitureSelf = 1000;
        }

        if (pawns.TryPopFront(out pawn)) {
            pawn.Name = new NameTriple("Renarin", "Son of Thorns", "Kholin");
            pawn.gender = Gender.Male;
            pawn.story.bodyType = BodyTypeDefOf.Male;
            pawn.genes.TryAddRadiantOrder(GeneDefOf.Cosmere_Roshar_Gene_RadiantTruthwatcher, 0);
            pawn.skills.GetSkill(SkillDefOf.Cosmere_Roshar_Skill_SurgebindingPower).Level = 5;
            pawn.records.AddTo(RecordDefOf.Cosmere_Roshar_Record_PatientsSaved, 5);
            pawn.GetInvestiture().currentInvestitureSelf = 1000;
        }

        if (pawns.TryPopFront(out pawn)) {
            pawn.Name = new NameSingle("Dalinar");
            pawn.gender = Gender.Male;
            pawn.story.bodyType = BodyTypeDefOf.Male;
            pawn.genes.TryAddRadiantOrder(GeneDefOf.Cosmere_Roshar_Gene_RadiantBondsmith, 3);
            pawn.skills.GetSkill(SkillDefOf.Cosmere_Roshar_Skill_SurgebindingPower).Level = 15;
            pawn.records.AddTo(RecordDefOf.Cosmere_Roshar_Record_FriendshipsFormed, 5);
            pawn.GetInvestiture().currentInvestitureSelf = 1000;
        }
    }
}