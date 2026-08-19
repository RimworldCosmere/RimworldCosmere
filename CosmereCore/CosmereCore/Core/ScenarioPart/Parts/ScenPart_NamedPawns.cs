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
        generationCounter = 0;
        StartingPawnUtility.ClearAllStartingPawns();
        for (int i = 0; i < pawns.Count; i++) {
            NamedPawnDef template = pawns[i];

            // Set before generation: post-generation edits only change the label, not body/head/hair.
            PawnGenerationRequest request = StartingPawnUtility.GetGenerationRequest(i);

            if (template.gender != Gender.None) request.FixedGender = template.gender;

            if (template.age > 0) {
                // ValidateAndFix rejects a fixed age set alongside a range, so both must clear first.
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

    /// <summary>
    ///     Wires the named pawns to each other once they all exist. Anything the generator
    ///     invented in the meantime - a spouse it picked at random - is cleared first, so a
    ///     scenario that says two people are married does not leave them married to strangers.
    /// </summary>
    private void ApplyRelations() {
        for (int i = 0; i < pawns.Count; i++) {
            NamedPawnDef template = pawns[i];
            if (template.relations.Count == 0 || template.firstName == null) continue;

            Pawn? self = FindNamed(template.firstName);
            if (self?.relations == null) continue;

            for (int r = 0; r < template.relations.Count; r++) {
                NamedPawnRelationEntry entry = template.relations[r];
                if (entry.def == null || entry.to == null) continue;

                PawnRelationDef? def = DefDatabase<PawnRelationDef>.GetNamedSilentFail(entry.def);
                if (def == null) {
                    Logger.Warning($"ScenPart_NamedPawns: PawnRelationDef '{entry.def}' not found");
                    continue;
                }

                Pawn? other = FindNamed(entry.to);
                if (other == null || other == self) continue;
                if (self.relations.DirectRelationExists(def, other)) continue;

                if (def == PawnRelationDefOf.Spouse || def == PawnRelationDefOf.Lover ||
                    def == PawnRelationDefOf.Fiance) {
                    ClearRomance(self);
                    ClearRomance(other);
                }

                self.relations.AddDirectRelation(def, other);
                Logger.Info($"ScenPart_NamedPawns: {template.firstName} is {def.defName} to {entry.to}.");
            }
        }
    }

    private static void ClearRomance(Pawn pawn) {
        if (pawn.relations == null) return;

        List<DirectPawnRelation> existing = new List<DirectPawnRelation>(pawn.relations.DirectRelations);
        for (int i = 0; i < existing.Count; i++) {
            PawnRelationDef def = existing[i].def;
            if (def == PawnRelationDefOf.Spouse || def == PawnRelationDefOf.Lover ||
                def == PawnRelationDefOf.Fiance) {
                pawn.relations.RemoveDirectRelation(existing[i]);
            }
        }
    }

    private static Pawn? FindNamed(string firstName) {
        List<Map> maps = Find.Maps;
        for (int m = 0; m < maps.Count; m++) {
            List<Pawn> colonists = maps[m].mapPawns.FreeColonists;
            for (int i = 0; i < colonists.Count; i++) {
                Pawn pawn = colonists[i];
                if (pawn.Name is NameTriple t && (t.First == firstName || t.Nick == firstName)) return pawn;
            }
        }

        return null;
    }

    public override void Notify_PawnGenerated(Pawn pawn, PawnGenerationContext context, bool redressed) {
        if (context != PawnGenerationContext.PlayerStarter) return;
        if (pawns.Count == 0) return;

        int index = ResolveTemplateIndex();
        if (index < 0 || index >= pawns.Count) return;

        ApplyTemplate(pawn, pawns[index]);
        generationCounter++;
    }

    /// <summary>
    ///     Which template the pawn currently being generated should get.
    /// </summary>
    /// <remarks>
    ///     The roster is the source of truth: the pawn being built is not in it yet, so its
    ///     count is the index. An empty roster means index zero, not "fall back to the counter".
    ///     <para>
    ///         That fallback used to fire on the first pawn of a regeneration pass, because
    ///         ClearAllStartingPawns removes entries rather than nulling them and leaves the list
    ///         empty. The counter was already at the roster size from the previous pass, so
    ///         template zero was skipped and the first pawn came out a random colonist. Kelsier
    ///         arrived called Irish.
    ///     </para>
    ///     The counter survives only for the case where there is no GameInitData at all.
    /// </remarks>
    private int ResolveTemplateIndex() {
        List<Pawn>? startingPawns = Find.GameInitData?.startingAndOptionalPawns;
        if (startingPawns == null) {
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
        ApplyRelations();
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

        ApplyAppearance(pawn, template);

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
        if (template.kandraGeneration > 0) {
            // A story kandra's generation is a fact about it, not a roll.
            Verse.Gene? heritage = pawn.genes?.GetGene(
                DefDatabase<GeneDef>.GetNamedSilentFail("Cosmere_Scadrial_Gene_KandraHeritage")
            );
            if (heritage is System.Scadrial.Gene.KandraHeritage kandra) {
                kandra.SetGeneration(template.kandraGeneration);
            }
        }
    }

    /// <summary>
    ///     This hook fires after gear generation, so aging a pawn down here trips
    ///     LifeStageWorker_HumanlikeAdult into stripping adult-only apparel into their inventory.
    /// </summary>
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

    /// <summary>
    ///     Makes the pawn look like the person the template names.
    /// </summary>
    /// <remarks>
    ///     A named pawn is written to be someone in particular, so the backstory has to stick -
    ///     a random pair off pawn generation is off-character and can bar work the scenario never meant to exclude.
    ///     <para>
    ///         Gender used to be handed to the generator as a hint, which worked while this part
    ///         built the roster itself. The config page now clears the roster and regenerates it from
    ///         default requests, so the hint is gone by the time these pawns exist and the only thing
    ///         left is to correct them afterwards. Setting gender alone flips the label and leaves the
    ///         body and head that were rolled, which is how Kelsier ended up a woman.
    ///     </para>
    /// </remarks>
    private static void ApplyAppearance(Pawn pawn, NamedPawnDef template) {
        if (pawn.story == null) return;

        BodyTypeDef? body = template.bodyType == null
            ? null
            : DefDatabase<BodyTypeDef>.GetNamedSilentFail(template.bodyType);

        // No explicit body, but the rolled one belongs to the other gender.
        if (body == null && template.gender != Gender.None) {
            bool mismatched = (template.gender == Gender.Male && pawn.story.bodyType == BodyTypeDefOf.Female)
                              || (template.gender == Gender.Female && pawn.story.bodyType == BodyTypeDefOf.Male);
            if (mismatched) {
                body = template.gender == Gender.Male ? BodyTypeDefOf.Male : BodyTypeDefOf.Female;
            }
        }

        if (body != null) pawn.story.bodyType = body;

        HeadTypeDef? head = template.headType == null
            ? null
            : DefDatabase<HeadTypeDef>.GetNamedSilentFail(template.headType);

        // Head types carry a gender of their own, so a rolled one can contradict the template.
        if (head == null && template.gender != Gender.None) {
            HeadTypeDef? current = pawn.story.headType;
            if (current != null && current.gender != Gender.None && current.gender != template.gender) {
                head = Util.HeadTypeUtility.RandomFor(template.gender, pawn, current);
            }
        }

        if (head != null) pawn.story.headType = head;

        // Hair carries a style gender too, and a rolled one contradicts the template just as readily as the head did.
        HairDef? hair = pawn.story.hairDef;
        if (hair != null && !SuitsGender(hair.styleGender, template.gender)) {
            pawn.story.hairDef = DefDatabase<HairDef>.AllDefsListForReading
                .Where(candidate => SuitsGender(candidate.styleGender, template.gender))
                .RandomElementWithFallback(hair);
        }

        // Same for beards, which no amount of hair fixing hides.
        BeardDef? beard = pawn.style?.beardDef;
        if (pawn.style != null && template.gender == Gender.Female && beard != null && beard != BeardDefOf.NoBeard) {
            pawn.style.beardDef = BeardDefOf.NoBeard;
        }

        if (template.skinColor != null && ParseColour(template.skinColor) is UnityEngine.Color skin) {
            pawn.story.skinColorOverride = skin;
        }
    }

    /// <summary>
    ///     Whether a style is acceptable for this gender. "Usually" variants read as allowed,
    ///     the way vanilla treats them when it dresses a pawn.
    /// </summary>
    private static bool SuitsGender(StyleGender style, Gender gender) {
        if (gender == Gender.None) return true;

        return style switch {
            StyleGender.Any => true,
            StyleGender.Male or StyleGender.MaleUsually => gender == Gender.Male,
            StyleGender.Female or StyleGender.FemaleUsually => gender == Gender.Female,
            _ => true,
        };
    }

    /// <summary>Reads "(r, g, b)" in 0-255, the way the gene defs write colours.</summary>
    private static UnityEngine.Color? ParseColour(string text) {
        string[] parts = text.Trim('(', ')', ' ').Split(',');
        if (parts.Length != 3) return null;

        if (!float.TryParse(parts[0], out float r)) return null;
        if (!float.TryParse(parts[1], out float g)) return null;
        if (!float.TryParse(parts[2], out float b)) return null;

        return new UnityEngine.Color(r / 255f, g / 255f, b / 255f);
    }

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
