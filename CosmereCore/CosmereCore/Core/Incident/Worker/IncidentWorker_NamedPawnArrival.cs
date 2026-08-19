using Cosmere.Core.DefModExtension;
using Cosmere.Core.ScenarioPart;
using RimWorld;
using Verse;

namespace Cosmere.Core.Incident.Worker;

public class IncidentWorker_NamedPawnArrival : IncidentWorker {
    protected override bool TryExecuteWorker(IncidentParms parms) {
        Map? map = parms.target as Map ?? Find.AnyPlayerHomeMap;
        if (map == null) return false;

        NamedPawnIncidentConfig? config =
            def.GetModExtension<NamedPawnIncidentConfig>();
        if (config?.pawn == null) {
            Logger.Warning($"NamedPawnArrival: No NamedPawnIncidentConfig on IncidentDef '{def.defName}'");
            return false;
        }

        NamedPawnDef template = config.pawn;

        PawnKindDef pawnKind = def.pawnKind ?? PawnKindDefOf.Colonist;

        // gender and age go into the request itself; setting pawn.gender after generation leaves a mismatched body.
        PawnGenerationRequest request = new PawnGenerationRequest(
            pawnKind,
            Faction.OfPlayer,
            forceGenerateNewPawn: true
        );

        if (template.gender != Gender.None) request.FixedGender = template.gender;

        if (template.age > 0) {
            request.ExcludeBiologicalAgeRange = null;
            request.BiologicalAgeRange = null;
            request.FixedBiologicalAge = template.age;
            request.FixedChronologicalAge = template.GetChronologicalAge();
        }

        if (template.lastName != null) request.SetFixedLastName(template.lastName);

        if (template.xenotype != null) {
            XenotypeDef? forced = DefDatabase<XenotypeDef>.GetNamedSilentFail(template.xenotype);
            if (forced != null) request.ForcedXenotype = forced;
        }

        Pawn pawn = PawnGenerator.GeneratePawn(request);

        ApplyTemplate(pawn, template);

        if (!CellFinder.TryFindRandomEdgeCellWith(
                c => map.reachability.CanReachColony(c) && !c.Fogged(map),
                map,
                CellFinder.EdgeRoadChance_Neutral,
                out IntVec3 cell
            )) {
            return false;
        }

        GenSpawn.Spawn(pawn, cell, map);

        TaggedString text = def.letterText.Formatted(pawn.Named("PAWN")).AdjustedFor(pawn);
        TaggedString title = def.letterLabel.Formatted(pawn.Named("PAWN")).AdjustedFor(pawn);
        SendStandardLetter(title, text, def.letterDef ?? LetterDefOf.PositiveEvent, parms, pawn);

        return true;
    }

    private static void ApplyTemplate(Pawn pawn, NamedPawnDef template) {
        Name? name = template.GetName();
        if (name != null) pawn.Name = name;

        // the request already fixed these; a template on an existing pawn still needs them straightened out.
        if (template.gender != Gender.None) pawn.gender = template.gender;

        if (template.age > 0) {
            pawn.ageTracker.AgeBiologicalTicks = template.age * 3600000L;
            pawn.ageTracker.AgeChronologicalTicks = template.GetChronologicalAge() * 3600000L;
        }

        if (template.xenotype != null) {
            XenotypeDef? xenotypeDef = DefDatabase<XenotypeDef>.GetNamedSilentFail(template.xenotype);
            if (xenotypeDef != null) pawn.genes?.SetXenotype(xenotypeDef);
        }

        ApplyBackstories(pawn, template);

        for (int i = 0; i < template.traits.Count; i++) {
            NamedPawnTraitEntry entry = template.traits[i];
            if (entry.def == null) continue;
            TraitDef? traitDef = DefDatabase<TraitDef>.GetNamedSilentFail(entry.def);
            if (traitDef != null) pawn.story?.traits?.GainTrait(new Trait(traitDef, entry.degree));
        }

        for (int i = 0; i < template.skills.Count; i++) {
            NamedPawnSkillEntry entry = template.skills[i];
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

        NamedPawnApplierRegistry.ApplyAll(pawn, template);

        for (int i = 0; i < template.apparel.Count; i++) {
            NamedPawnInventoryEntry entry = template.apparel[i];
            if (entry.thing == null) continue;
            ThingDef? apparelDef = DefDatabase<ThingDef>.GetNamedSilentFail(entry.thing);
            if (apparelDef == null) continue;

            ThingDef? apparelStuff = null;
            if (entry.stuff != null) apparelStuff = DefDatabase<ThingDef>.GetNamedSilentFail(entry.stuff);

            if (ThingMaker.MakeThing(apparelDef, apparelStuff) is Apparel worn) {
                pawn.apparel?.Wear(worn, false);
            }
        }

        for (int i = 0; i < template.inventory.Count; i++) {
            NamedPawnInventoryEntry entry = template.inventory[i];
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

    /// <summary>
    ///     A named pawn is written to be someone in particular, so their story has to stick. Left
    ///     to generation they'd get a random pair, off-character and prone to disabling work types.
    /// </summary>
    private static void ApplyBackstories(Pawn pawn, NamedPawnDef template) {
        if (pawn.story == null) return;

        if (template.childhood != null) {
            BackstoryDef? story = DefDatabase<BackstoryDef>.GetNamedSilentFail(template.childhood);
            if (story == null) {
                Logger.Warning($"NamedPawnArrival: Childhood '{template.childhood}' not found, skipping");
            } else {
                pawn.story.Childhood = story;
            }
        }

        if (template.adulthood != null) {
            BackstoryDef? story = DefDatabase<BackstoryDef>.GetNamedSilentFail(template.adulthood);
            if (story == null) {
                Logger.Warning($"NamedPawnArrival: Adulthood '{template.adulthood}' not found, skipping");
            } else {
                pawn.story.Adulthood = story;
            }
        }

        pawn.Notify_DisabledWorkTypesChanged();
    }
}
