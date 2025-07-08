using System.Collections.Generic;
using CosmereScadrial.Def;
using CosmereScadrial.Extension;
using RimWorld;
using Verse;
using GeneUtility = CosmereScadrial.Util.GeneUtility;

namespace CosmereScadrial.Thing;

public class AllomanticMetal : AllomanticVial {
    public override MetallicArtsMetalDef metal => DefDatabase<MetallicArtsMetalDef>.GetNamed(def.defName);

    protected override void PostIngested(Pawn ingester) {
        if (metal.godMetal) {
            if (metal.Equals(MetallicArtsMetalDefOf.Lerasium)) {
                GeneUtility.AddMistborn(ingester, false, true, "ingested Lerasium");
                ingester.FillAllAllomanticReserves();
                ingester.skills.GetSkill(SkillDefOf.Cosmere_Scadrial_Skill_AllomanticPower).Level += 10;
                ingester.records.Increment(RecordDefOf.Cosmere_Scadrial_Record_IngestedLerasium);
                StatDefOf.Cosmere_Scadrial_Stat_AllomanticPower.Worker.ClearCacheForThing(ingester);
            }

            if (metal.Equals(MetallicArtsMetalDefOf.Leratium)) {
                GeneUtility.AddFullFeruchemist(ingester, false, true, "ingested Leratium");
                ingester.skills.GetSkill(SkillDefOf.Cosmere_Scadrial_Skill_FeruchemicPower).Level += 10;
                ingester.records.Increment(RecordDefOf.Cosmere_Scadrial_Record_IngestedLeratium);
                StatDefOf.Cosmere_Scadrial_Stat_FeruchemicPower.Worker.ClearCacheForThing(ingester);
            }

            if (metal.Equals(MetallicArtsMetalDefOf.Atium)) {
                GeneUtility.AddGene(ingester, metal.GetMistingGene(), false, true);
                ingester.FillAllomanticReserves(metal);
            }

            if (metal.Equals(MetallicArtsMetalDefOf.LerasiumAlloy)) {
                MetallicArtsMetalDef? stuffMetal = DefDatabase<MetallicArtsMetalDef>.GetNamedSilentFail(Stuff.defName);
                GeneUtility.AddGene(ingester, stuffMetal.GetMistingGene(), false, true);
                ingester.FillAllomanticReserves(stuffMetal);
                ingester.skills.GetSkill(SkillDefOf.Cosmere_Scadrial_Skill_AllomanticPower).Level += 5;
                ingester.records.Increment(RecordDefOf.Cosmere_Scadrial_Record_IngestedLerasiumAlloy);
                StatDefOf.Cosmere_Scadrial_Stat_AllomanticPower.Worker.ClearCacheForThing(ingester);
            }

            if (metal.Equals(MetallicArtsMetalDefOf.LeratiumAlloy)) {
                MetallicArtsMetalDef? stuffMetal = DefDatabase<MetallicArtsMetalDef>.GetNamedSilentFail(Stuff.defName);
                GeneUtility.AddGene(ingester, stuffMetal.GetFerringGene(), false, true);
                ingester.skills.GetSkill(SkillDefOf.Cosmere_Scadrial_Skill_FeruchemicPower).Level += 5;
                ingester.records.Increment(RecordDefOf.Cosmere_Scadrial_Record_IngestedLeratiumAlloy);
                StatDefOf.Cosmere_Scadrial_Stat_FeruchemicPower.Worker.ClearCacheForThing(ingester);
            }

            Find.LetterStack.ReceiveLetter(
                "CS_BurnedGodMetal".Translate(metal.LabelCap.Named("METAL")),
                $"CS_BurnedGodMetal_{metal.LabelCap}".Translate(ingester.NameFullColored.Named("PAWN")).Resolve(),
                LetterDefOf.PositiveEvent,
                ingester
            );

            return;
        }

        Messages.Message(
            "CS_IngestedThing".Translate(ingester.NameFullColored.Named("PAWN"), metal.coloredLabel.Named("THING")),
            ingester,
            MessageTypeDefOf.PositiveEvent
        );
        ingester.genes.GetAllomanticGeneForMetal(metal)?.AddToReserve(Constants.RawMetalMetalAmount);

        ingester.records.Increment(RecordDefOf.Cosmere_Scadrial_Record_IngestedRawMetal);
    }

    public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn) {
        yield break;
    }

    public override IEnumerable<FloatMenuOption> GetMultiSelectFloatMenuOptions(IEnumerable<Pawn> selPawns) {
        yield break;
    }
}