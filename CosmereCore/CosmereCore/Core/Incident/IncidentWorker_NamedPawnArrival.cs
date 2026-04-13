using System;
using RimWorld;
using Verse;

namespace Cosmere.Core.Incident;

public class IncidentWorker_NamedPawnArrival : IncidentWorker {
    protected override bool TryExecuteWorker(IncidentParms parms) {
        Map? map = parms.target as Map ?? Find.AnyPlayerHomeMap;
        if (map == null) return false;

        DefModExtension.NamedPawnIncidentConfig? config =
            def.GetModExtension<DefModExtension.NamedPawnIncidentConfig>();
        if (config?.pawn == null) {
            Logger.Warning($"NamedPawnArrival: No NamedPawnIncidentConfig on IncidentDef '{def.defName}'");
            return false;
        }

        ScenarioPart.NamedPawnDef template = config.pawn;

        PawnKindDef pawnKind = def.pawnKind ?? PawnKindDefOf.Colonist;
        Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(
            pawnKind,
            Faction.OfPlayer,
            PawnGenerationContext.NonPlayer,
            forceGenerateNewPawn: true
        ));

        ApplyTemplate(pawn, template);

        if (!CellFinder.TryFindRandomEdgeCellWith(
                c => map.reachability.CanReachColony(c) && !c.Fogged(map),
                map, CellFinder.EdgeRoadChance_Neutral, out IntVec3 cell)) {
            return false;
        }

        GenSpawn.Spawn(pawn, cell, map);

        TaggedString text = def.letterText.Formatted(pawn.Named("PAWN")).AdjustedFor(pawn);
        TaggedString title = def.letterLabel.Formatted(pawn.Named("PAWN")).AdjustedFor(pawn);
        SendStandardLetter(title, text, def.letterDef ?? LetterDefOf.PositiveEvent, parms, pawn);

        return true;
    }

    private static void ApplyTemplate(Pawn pawn, ScenarioPart.NamedPawnDef template) {
        Name? name = template.GetName();
        if (name != null) pawn.Name = name;

        if (template.gender != Gender.None) pawn.gender = template.gender;

        if (template.age > 0) {
            pawn.ageTracker.AgeBiologicalTicks = template.age * 3600000L;
            pawn.ageTracker.AgeChronologicalTicks = template.GetChronologicalAge() * 3600000L;
        }

        if (template.xenotype != null) {
            XenotypeDef? xenotypeDef = DefDatabase<XenotypeDef>.GetNamedSilentFail(template.xenotype);
            if (xenotypeDef != null) pawn.genes?.SetXenotype(xenotypeDef);
        }

        for (int i = 0; i < template.traits.Count; i++) {
            ScenarioPart.NamedPawnTraitEntry entry = template.traits[i];
            if (entry.def == null) continue;
            TraitDef? traitDef = DefDatabase<TraitDef>.GetNamedSilentFail(entry.def);
            if (traitDef != null) pawn.story?.traits?.GainTrait(new Trait(traitDef, entry.degree));
        }

        for (int i = 0; i < template.skills.Count; i++) {
            ScenarioPart.NamedPawnSkillEntry entry = template.skills[i];
            if (entry.def == null) continue;
            SkillDef? skillDef = DefDatabase<SkillDef>.GetNamedSilentFail(entry.def);
            if (skillDef == null) continue;
            SkillRecord? record = pawn.skills?.GetSkill(skillDef);
            if (record == null) continue;
            record.Level = entry.level;
            record.passion = entry.passion;
        }

        for (int i = 0; i < template.genes.Count; i++) {
            string geneName = template.genes[i];
            GeneDef? geneDef = DefDatabase<GeneDef>.GetNamedSilentFail(geneName);
            if (geneDef != null && pawn.genes != null && !pawn.genes.HasActiveGene(geneDef)) {
                pawn.genes.AddGene(geneDef, true);
            }
        }

        if (template.radiantOrder != null) {
            GeneDef? orderGeneDef = DefDatabase<GeneDef>.GetNamedSilentFail(template.radiantOrder);
            if (orderGeneDef != null) {
                try {
                    pawn.genes?.TryAddRadiantOrder(orderGeneDef, template.idealLevel);
                } catch (Exception ex) {
                    Logger.Warning($"NamedPawnArrival: Failed to add radiant order: {ex.Message}");
                }
            }
        }

        if (template.mistborn) {
            Cosmere.System.Scadrial.Utility.GeneUtility.AddMistborn(pawn, false, true);
        }

        if (template.fullFeruchemist) {
            Cosmere.System.Scadrial.Utility.GeneUtility.AddFullFeruchemist(pawn, false, true);
        }

        for (int i = 0; i < template.inventory.Count; i++) {
            ScenarioPart.NamedPawnInventoryEntry entry = template.inventory[i];
            if (entry.thing == null) continue;
            ThingDef? thingDef = DefDatabase<ThingDef>.GetNamedSilentFail(entry.thing);
            if (thingDef == null) continue;

            ThingDef? stuffDef = null;
            if (entry.stuff != null) stuffDef = DefDatabase<ThingDef>.GetNamedSilentFail(entry.stuff);

            Verse.Thing thing = ThingMaker.MakeThing(thingDef, stuffDef);
            thing.stackCount = entry.count;
            pawn.inventory?.innerContainer.TryAdd(thing);
        }
    }
}
