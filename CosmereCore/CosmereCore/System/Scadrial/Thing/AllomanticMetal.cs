using Cosmere.System.Scadrial.Def;
using RimWorld;
using Verse;
using GeneUtility = Cosmere.System.Scadrial.Utility.GeneUtility;
using Logger = Cosmere.Core.Logger;

namespace Cosmere.System.Scadrial.Thing;

public class AllomanticMetal : AllomanticVial {
    private MetallicArtsMetalDef? cachedMetal;

    public override MetallicArtsMetalDef? metal =>
        cachedMetal ??= DefDatabase<MetallicArtsMetalDef>.GetNamedSilentFail(def.defName);

    protected override void PostIngested(Pawn ingester) {
        if (metal is null) return;

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
                if (stuffMetal == null) {
                    Logger.Warning($"AllomanticMetal: could not find MetallicArtsMetalDef for stuff '{Stuff?.defName}'");
                    return;
                }
                GeneUtility.AddGene(ingester, stuffMetal.GetMistingGene(), false, true);
                ingester.FillAllomanticReserves(stuffMetal);
                ingester.skills.GetSkill(SkillDefOf.Cosmere_Scadrial_Skill_AllomanticPower).Level += 5;
                ingester.records.Increment(RecordDefOf.Cosmere_Scadrial_Record_IngestedLerasiumAlloy);
                StatDefOf.Cosmere_Scadrial_Stat_AllomanticPower.Worker.ClearCacheForThing(ingester);
            }

            if (metal.Equals(MetallicArtsMetalDefOf.LeratiumAlloy)) {
                MetallicArtsMetalDef? stuffMetal = DefDatabase<MetallicArtsMetalDef>.GetNamedSilentFail(Stuff.defName);
                if (stuffMetal == null) {
                    Logger.Warning($"AllomanticMetal: could not find MetallicArtsMetalDef for stuff '{Stuff?.defName}'");
                    return;
                }
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