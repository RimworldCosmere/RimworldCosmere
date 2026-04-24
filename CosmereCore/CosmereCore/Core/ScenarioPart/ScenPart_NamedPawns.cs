using System;
using RimWorld;
using Verse;
using GeneUtility = Cosmere.System.Scadrial.Utility.GeneUtility;

namespace Cosmere.Core.ScenarioPart;

public class ScenPart_NamedPawns : ScenPart {
    private int generationCounter;
    public List<NamedPawnDef> pawns = [];

    public override void PostIdeoChosen() {
        base.PostIdeoChosen();
        if (pawns.Count == 0) return;

        Find.GameInitData.startingPawnCount = pawns.Count;
        StartingPawnUtility.ClearAllStartingPawns();
        for (int i = 0; i < pawns.Count; i++) {
            StartingPawnUtility.AddNewPawn();
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
        if (pawns.Count == 0) return "";
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

        ApplyXenotype(pawn, template);
        ApplyTraits(pawn, template);
        ApplySkills(pawn, template);

        pawn.Drawer?.renderer?.SetAllGraphicsDirty();
    }

    private void ApplyPostStartEffects() {
        List<Pawn>? colonists = Find.CurrentMap?.mapPawns?.FreeColonists?.ToList();
        if (colonists == null) return;

        for (int i = 0; i < pawns.Count && i < colonists.Count; i++) {
            NamedPawnDef template = pawns[i];
            Pawn? pawn = FindPawnByName(colonists, template);
            if (pawn == null) continue;

            ApplyGenes(pawn, template);
            ApplyRadiantOrder(pawn, template);
            ApplyMetalborn(pawn, template);
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
        if (template.traits.Count == 0) return;

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

    private static void ApplyRadiantOrder(Pawn pawn, NamedPawnDef template) {
        if (template.radiantOrder == null) return;

        GeneDef orderGeneDef = DefDatabase<GeneDef>.GetNamedSilentFail(template.radiantOrder);
        if (orderGeneDef == null) {
            Logger.Warning($"ScenPart_NamedPawns: Radiant order gene '{template.radiantOrder}' not found, skipping");
            return;
        }

        try {
            pawn.genes?.TryAddRadiantOrder(orderGeneDef, template.idealLevel);
        } catch (Exception ex) {
            Logger.Warning($"ScenPart_NamedPawns: Failed to add radiant order '{template.radiantOrder}': {ex.Message}");
        }
    }

    private static void ApplyMetalborn(Pawn pawn, NamedPawnDef template) {
        if (!template.mistborn && !template.fullFeruchemist) return;

        try {
            if (template.mistborn) {
                GeneUtility.AddMistborn(pawn, false, true);
            }

            if (template.fullFeruchemist) {
                GeneUtility.AddFullFeruchemist(pawn, false, true);
            }
        } catch (Exception ex) {
            Logger.Warning($"ScenPart_NamedPawns: Failed to add metalborn genes: {ex.Message}");
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