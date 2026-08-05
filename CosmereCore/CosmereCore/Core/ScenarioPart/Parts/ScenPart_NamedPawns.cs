using RimWorld;
using Verse;

namespace Cosmere.Core.ScenarioPart.Parts;

public class ScenPart_NamedPawns : ScenPart {
    private int generationCounter;
    public List<NamedPawnDef> pawns = [];

    public override void PostIdeoChosen() {
        base.PostIdeoChosen();
        if (pawns.Count == 0) return;

        Find.GameInitData.startingPawnCount = pawns.Count;
        StartingPawnUtility.ClearAllStartingPawns();
        for (int i = 0; i < pawns.Count; i++) {
            NamedPawnDef template = pawns[i];

            // Say what the pawn is before it is built rather than editing it afterwards. Gender,
            // age and xenotype all decide body type, head, hair and beard during generation, so
            // anything set in Notify_PawnGenerated flips the label and leaves the body it was
            // given. AddNewPawn must be passed the index: with the default of -1 it generates
            // from DefaultStartingPawnRequest and never reads what was set here.
            PawnGenerationRequest request = StartingPawnUtility.GetGenerationRequest(i);

            if (template.gender != Gender.None) request.FixedGender = template.gender;

            if (template.age > 0) {
                // The default starting-pawn request excludes a child age band. ValidateAndFix
                // rejects a fixed age alongside any range, so both have to come off first.
                request.ExcludeBiologicalAgeRange = null;
                request.BiologicalAgeRange = null;
                request.FixedBiologicalAge = template.age;
                request.FixedChronologicalAge = template.GetChronologicalAge();
            }

            if (template.lastName != null) request.SetFixedLastName(template.lastName);

            if (template.xenotype != null) {
                XenotypeDef? xenotype = DefDatabase<XenotypeDef>.GetNamedSilentFail(template.xenotype);
                if (xenotype != null) request.ForcedXenotype = xenotype;
            }

            StartingPawnUtility.SetGenerationRequest(i, request);
            StartingPawnUtility.AddNewPawn(i);
        }
    }

    public override void Notify_PawnGenerated(Pawn pawn, PawnGenerationContext context, bool redressed) {
        if (context != PawnGenerationContext.PlayerStarter) return;
        if (pawns.Count == 0) return;

        int index = ResolveTemplateIndex();
        if (index < 0 || index >= pawns.Count) return;

        ApplyTemplate(pawn, pawns[index]);
        generationCounter++;
    }

    private int ResolveTemplateIndex() {
        List<Pawn>? startingPawns = Find.GameInitData?.startingAndOptionalPawns;
        if (startingPawns == null || startingPawns.Count == 0) {
            return generationCounter;
        }

        for (int i = 0; i < startingPawns.Count; i++) {
            if (startingPawns[i] == null) {
                return i;
            }
        }

        return startingPawns.Count;
    }

    public override void PreConfigure() {
        base.PreConfigure();
        generationCounter = 0;
    }

    public override void PostGameStart() {
        base.PostGameStart();
        ApplyPostStartEffects();
    }

    public override string Summary(Scenario scen) {
        if (pawns.Count == 0) return string.Empty;
        return "Named characters: " + string.Join(", ", pawns.Select(p => p.firstName ?? "Unknown"));
    }

    private void ApplyTemplate(Pawn pawn, NamedPawnDef template) {
        Name? name = template.GetName();
        if (name != null) {
            pawn.Name = name;
        }

        if (template.gender != Gender.None) {
            pawn.gender = template.gender;
        }

        if (template.age > 0) {
            pawn.ageTracker.AgeBiologicalTicks = template.age * 3600000L;
            pawn.ageTracker.AgeChronologicalTicks = template.GetChronologicalAge() * 3600000L;
        }

        ApplyBackstories(pawn, template);
        ApplyXenotype(pawn, template);
        ApplyTraits(pawn, template);
        ApplySkills(pawn, template);
        ApplyApparel(pawn, template);

        RedressAfterAgeChange(pawn);

        pawn.Drawer?.renderer?.SetAllGraphicsDirty();
    }

    // This hook fires after gear generation, so aging a pawn down here trips
    // LifeStageWorker_HumanlikeAdult into stripping adult-only apparel into their inventory.
    private static void RedressAfterAgeChange(Pawn pawn) {
        if (pawn.apparel == null || pawn.inventory == null) return;

        List<Verse.Thing> carried = [.. pawn.inventory.innerContainer];
        for (int i = 0; i < carried.Count; i++) {
            if (carried[i] is not Apparel apparel) continue;
            if (!ApparelUtility.HasPartsToWear(pawn, apparel.def)) continue;

            pawn.inventory.innerContainer.Remove(apparel);
            pawn.apparel.Wear(apparel, false);
        }
    }

    private void ApplyPostStartEffects() {
        List<Pawn>? colonists = Find.CurrentMap?.mapPawns?.FreeColonists?.ToList();
        if (colonists == null) return;

        for (int i = 0; i < pawns.Count && i < colonists.Count; i++) {
            NamedPawnDef template = pawns[i];
            Pawn? pawn = FindPawnByName(colonists, template);
            if (pawn == null) continue;

            ApplyGenes(pawn, template);
            NamedPawnApplierRegistry.ApplyAll(pawn, template);
            ApplyInventory(pawn, template);

            pawn.Drawer?.renderer?.SetAllGraphicsDirty();
        }
    }

    private static Pawn? FindPawnByName(List<Pawn> colonists, NamedPawnDef template) {
        if (template.firstName == null) return null;

        for (int i = 0; i < colonists.Count; i++) {
            Pawn pawn = colonists[i];
            if (pawn.Name is NameTriple triple && triple.First == template.firstName) {
                return pawn;
            }

            if (pawn.Name is NameSingle single && single.Name.StartsWith(template.firstName)) {
                return pawn;
            }
        }

        return null;
    }

    // A named pawn is written to be someone in particular, so the story they are
    // given has to stick. Left to pawn generation they take a random pair, which
    // is both off-character and how they end up incapable of work the scenario
    // never meant to bar them from.
    private static void ApplyBackstories(Pawn pawn, NamedPawnDef template) {
        if (pawn.story == null) return;

        if (template.childhood != null) {
            BackstoryDef? story = DefDatabase<BackstoryDef>.GetNamedSilentFail(template.childhood);
            if (story == null) {
                Logger.Warning($"ScenPart_NamedPawns: Childhood '{template.childhood}' not found, skipping");
            } else {
                pawn.story.Childhood = story;
            }
        }

        if (template.adulthood != null) {
            BackstoryDef? story = DefDatabase<BackstoryDef>.GetNamedSilentFail(template.adulthood);
            if (story == null) {
                Logger.Warning($"ScenPart_NamedPawns: Adulthood '{template.adulthood}' not found, skipping");
            } else {
                pawn.story.Adulthood = story;
            }
        }

        pawn.Notify_DisabledWorkTypesChanged();
    }

    private static void ApplyXenotype(Pawn pawn, NamedPawnDef template) {
        if (template.xenotype == null) return;

        XenotypeDef xenotypeDef = DefDatabase<XenotypeDef>.GetNamedSilentFail(template.xenotype);
        if (xenotypeDef == null) {
            Logger.Warning($"ScenPart_NamedPawns: Xenotype '{template.xenotype}' not found, skipping");
            return;
        }

        pawn.genes?.SetXenotype(xenotypeDef);
    }

    private static void ApplyTraits(Pawn pawn, NamedPawnDef template) {
        if (template.traits.Count == 0 && !template.noRandomTraits) return;

        List<Trait>? existingTraits = pawn.story?.traits?.allTraits;
        if (existingTraits != null) {
            for (int i = existingTraits.Count - 1; i >= 0; i--) {
                pawn.story!.traits.RemoveTrait(existingTraits[i]);
            }
        }

        for (int i = 0; i < template.traits.Count; i++) {
            NamedPawnTraitEntry entry = template.traits[i];
            TraitDef traitDef = DefDatabase<TraitDef>.GetNamedSilentFail(entry.def);
            if (traitDef == null) {
                Logger.Warning($"ScenPart_NamedPawns: Trait '{entry.def}' not found, skipping");
                continue;
            }

            pawn.story?.traits?.GainTrait(new Trait(traitDef, entry.degree));
        }
    }

    private static void ApplySkills(Pawn pawn, NamedPawnDef template) {
        if (template.skills.Count == 0) return;

        for (int i = 0; i < template.skills.Count; i++) {
            NamedPawnSkillEntry entry = template.skills[i];
            SkillDef skillDef = DefDatabase<SkillDef>.GetNamedSilentFail(entry.def);
            if (skillDef == null) {
                Logger.Warning($"ScenPart_NamedPawns: Skill '{entry.def}' not found, skipping");
                continue;
            }

            SkillRecord? skill = pawn.skills?.GetSkill(skillDef);
            if (skill == null) continue;

            skill.Level = entry.level;
            skill.passion = entry.passion;
        }
    }

    private static void ApplyGenes(Pawn pawn, NamedPawnDef template) {
        if (template.genes.Count == 0) return;

        for (int i = 0; i < template.genes.Count; i++) {
            string geneName = template.genes[i];
            GeneDef geneDef = DefDatabase<GeneDef>.GetNamedSilentFail(geneName);
            if (geneDef == null) {
                Logger.Warning($"ScenPart_NamedPawns: Gene '{geneName}' not found, skipping");
                continue;
            }

            if (pawn.genes != null && !pawn.genes.HasActiveGene(geneDef)) {
                pawn.genes.AddGene(geneDef, true);
            }
        }
    }

    /// <summary>Puts clothing on rather than in the pack, replacing whatever it conflicts with.</summary>
    private static void ApplyApparel(Pawn pawn, NamedPawnDef template) {
        if (template.apparel.Count == 0 || pawn.apparel == null) return;

        for (int i = 0; i < template.apparel.Count; i++) {
            NamedPawnInventoryEntry entry = template.apparel[i];
            ThingDef? thingDef = DefDatabase<ThingDef>.GetNamedSilentFail(entry.thing);
            if (thingDef == null) {
                Logger.Warning($"ScenPart_NamedPawns: Apparel '{entry.thing}' not found, skipping");
                continue;
            }

            ThingDef? stuffDef = entry.stuff == null
                ? GenStuff.DefaultStuffFor(thingDef)
                : DefDatabase<ThingDef>.GetNamedSilentFail(entry.stuff);

            if (ThingMaker.MakeThing(thingDef, stuffDef) is not Apparel made) continue;

            pawn.apparel.Wear(made, false);
        }
    }

    private static void ApplyInventory(Pawn pawn, NamedPawnDef template) {
        if (template.inventory.Count == 0) return;

        for (int i = 0; i < template.inventory.Count; i++) {
            NamedPawnInventoryEntry entry = template.inventory[i];
            ThingDef thingDef = DefDatabase<ThingDef>.GetNamedSilentFail(entry.thing);
            if (thingDef == null) {
                Logger.Warning($"ScenPart_NamedPawns: Thing '{entry.thing}' not found, skipping");
                continue;
            }

            ThingDef? stuffDef = null;
            if (entry.stuff != null) {
                stuffDef = DefDatabase<ThingDef>.GetNamedSilentFail(entry.stuff);
            }

            Verse.Thing thing = ThingMaker.MakeThing(thingDef, stuffDef);
            thing.stackCount = entry.count;
            pawn.inventory?.innerContainer.TryAdd(thing);
        }
    }
}
