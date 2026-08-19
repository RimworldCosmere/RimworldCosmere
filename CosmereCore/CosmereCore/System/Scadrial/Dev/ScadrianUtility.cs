using Cosmere.Core.Def;
using Cosmere.System.Scadrial.Def;
using Cosmere.System.Scadrial.Feruchemy.Comp.Thing;
using Cosmere.System.Scadrial.Feruchemy.Hediff;
using Cosmere.System.Scadrial.Gene;
using Cosmere.System.Scadrial.Util;
using LudeonTK;
using RimWorld;
using Verse;
using GeneUtility = Cosmere.System.Scadrial.Util.GeneUtility;

namespace Cosmere.System.Scadrial.Dev;

[StaticConstructorOnStartup]
public static class ScadrianUtility {
    /// <summary>
    ///     Compounding demands level ten in both arts, so a pawn meant to demonstrate it cannot be
    ///     left on whatever pawn generation rolled.
    /// </summary>
    public static void SetMetallicArtsSkills(Pawn pawn, int level) {
        if (pawn.skills == null) return;

        pawn.skills.GetSkill(SkillDefOf.Cosmere_Scadrial_Skill_AllomanticPower).Level = level;
        pawn.skills.GetSkill(SkillDefOf.Cosmere_Scadrial_Skill_FeruchemicPower).Level = level;
    }

    /// <summary>
    ///     Compounding only reaches a metalmind inside the body, so a pawn meant to demonstrate it
    ///     needs implants rather than the bands everyone else carries.
    /// </summary>
    public static void AddImplantedMetalminds(Pawn pawn, MetalDef metal, int count) {
        ThingDef? implantDef = DefDatabase<ThingDef>.GetNamedSilentFail("Cosmere_Scadrial_Thing_MetalmindImplant");
        if (implantDef == null || pawn.health == null) return;

        float capacity = implantDef.GetCompProperties<MetalmindProperties>()?.maxAmount ?? 0f;
        if (capacity <= 0f) return;

        BodyPartRecord? torso = null;
        List<BodyPartRecord> parts = pawn.RaceProps.body.AllParts;
        for (int i = 0; i < parts.Count; i++) {
            if (parts[i].def != pawn.RaceProps.body.corePart.def) continue;
            torso = parts[i];
            break;
        }

        torso ??= pawn.RaceProps.body.corePart;
        if (torso == null) return;

        for (int i = 0; i < count; i++) {
            ImplantedMetalminds.Attach(
                pawn,
                new ImplantedMetalmindData {
                    metalDefName = metal.defName,
                    metalmindType = implantDef.defName,
                    MaxAmount = capacity,
                    ownerName = pawn.Name?.ToStringFull ?? string.Empty,
                },
                torso
            );
        }
    }

    [DebugAction(
        "Cosmere/Scadrial",
        "Prepare Dev Pawn",
        actionType = DebugActionType.ToolMapForPawns,
        allowedGameStates = AllowedGameStates.PlayingOnMap
    )]
    public static void PrepareDevPawn(Pawn pawn) {
        if (pawn.genes == null) return;
        if (!pawn.story.traits.HasTrait(TraitDefOf.Cosmere_Scadrial_Trait_Mistborn)) {
            GeneUtility.AddMistborn(pawn, false, true);
            Messages.Message($"Made {pawn.NameFullColored} a mistborn", pawn, MessageTypeDefOf.PositiveEvent);
        }

        FillAllReserves(pawn);
        GiveAllAllomanticVials(pawn);
    }

    [DebugAction(
        "Cosmere/Scadrial",
        "Give All Allomantic Vials",
        actionType = DebugActionType.ToolMapForPawns,
        allowedGameStates = AllowedGameStates.PlayingOnMap
    )]
    public static void GiveAllAllomanticVials(Pawn pawn) {
        foreach (MetallicArtsMetalDef? metal in DefDatabase<MetallicArtsMetalDef>.AllDefsListForReading.Where(x =>
                     !x.godMetal && x.allomancy != null
                 )) {
            Verse.Thing? vial = ThingMaker.MakeThing(ThingDefOf.Cosmere_Scadrial_Thing_AllomanticVial, metal.Item);
            vial.stackCount = 20;
            pawn.inventory.innerContainer.TryAdd(vial);
        }

        Messages.Message(
            $"Gave {pawn.NameFullColored} 20 vials of each metal.",
            pawn,
            MessageTypeDefOf.PositiveEvent
        );
    }

    [DebugAction("Cosmere/Scadrial", "Prepare All Dev Pawns", allowedGameStates = AllowedGameStates.PlayingOnMap)]
    public static void PrepareAllDevPawn() {
        foreach (Pawn? pawn in Find.CurrentMap.mapPawns.AllPawnsSpawned) {
            PrepareDevPawn(pawn);
        }
    }

    [DebugAction(
        "Cosmere/Scadrial",
        "Fill allomantic reserves (all metals)",
        actionType = DebugActionType.ToolMapForPawns,
        allowedGameStates = AllowedGameStates.PlayingOnMap
    )]
    public static void FillAllReserves(Pawn pawn) {
        pawn.FillAllAllomanticReserves();

        Messages.Message(
            $"Gave {pawn.NameFullColored} full reserves reserves for all metals",
            pawn,
            MessageTypeDefOf.PositiveEvent
        );
    }

    [DebugAction(
        "Cosmere/Scadrial",
        "Wipe allomantic reserves (all metals)",
        actionType = DebugActionType.ToolMapForPawns,
        allowedGameStates = AllowedGameStates.PlayingOnMap
    )]
    public static void WipeAllReserves(Pawn pawn) {
        pawn.WipeAllAllomanticReserves();

        Messages.Message($"Wiped all reserves for {pawn.LabelShort}", pawn, MessageTypeDefOf.PositiveEvent);
    }

    [DebugAction(
        "Cosmere/Scadrial",
        "Fill specific metal reserve",
        actionType = DebugActionType.ToolMapForPawns,
        allowedGameStates = AllowedGameStates.PlayingOnMap
    )]
    public static void FillSpecificMetal(Pawn pawn) {
        List<DebugMenuOption> options = [];

        foreach (MetallicArtsMetalDef? metal in DefDatabase<MetallicArtsMetalDef>.AllDefs) {
            Allomancer gene = pawn.genes.GetAllomanticGeneForMetal(metal)!;
            string? label = metal.label.CapitalizeFirst();
            options.Add(
                new DebugMenuOption(
                    label,
                    DebugMenuOptionMode.Action,
                    () => {
                        gene.FillReserve();
                        Messages.Message($"Filled {label} for {pawn.LabelShort}", pawn, MessageTypeDefOf.PositiveEvent);
                    }
                )
            );
        }

        Find.WindowStack.Add(new Dialog_DebugOptionListLister(options));
    }

    [DebugAction(
        "Cosmere/Scadrial",
        "Wipe specific metal reserve",
        actionType = DebugActionType.ToolMapForPawns,
        allowedGameStates = AllowedGameStates.PlayingOnMap
    )]
    public static void WipeSpecificMetal(Pawn pawn) {
        List<DebugMenuOption> options = [];

        foreach (MetallicArtsMetalDef? metal in DefDatabase<MetallicArtsMetalDef>.AllDefs) {
            Allomancer gene = pawn.genes.GetAllomanticGeneForMetal(metal)!;
            string? label = metal.label.CapitalizeFirst();
            options.Add(
                new DebugMenuOption(
                    label,
                    DebugMenuOptionMode.Action,
                    () => {
                        gene.WipeReserve();
                        Messages.Message($"Wiped {label} for {pawn.LabelShort}", pawn, MessageTypeDefOf.PositiveEvent);
                    }
                )
            );
        }

        Find.WindowStack.Add(new Dialog_DebugOptionListLister(options));
    }

    [DebugAction(
        "Cosmere/Scadrial",
        "Snap Pawn",
        actionType = DebugActionType.ToolMapForPawns,
        allowedGameStates = AllowedGameStates.PlayingOnMap
    )]
    public static void SnapPawn(Pawn pawn) {
        SnapUtility.Snap(pawn);
    }

    [DebugAction(
        "Cosmere/Scadrial",
        "Try Give Random Allomantic Ability",
        actionType = DebugActionType.ToolMapForPawns,
        allowedGameStates = AllowedGameStates.PlayingOnMap
    )]
    public static void GiveRandomAllomanticAbility(Pawn pawn) {
        if (Rand.Chance(1f / 16f)) {
            GeneUtility.AddMistborn(pawn);
        } else {
            GeneUtility.AddRandomAllomanticGene(pawn);
        }
    }

    [DebugAction(
        "Cosmere/Scadrial",
        "Try Give Random Feruchemical Ability",
        actionType = DebugActionType.ToolMapForPawns,
        allowedGameStates = AllowedGameStates.PlayingOnMap
    )]
    public static void GiveRandomFeruchemicalAbility(Pawn pawn) {
        if (Rand.Chance(1f / 16f)) {
            GeneUtility.AddFullFeruchemist(pawn);
        } else {
            GeneUtility.AddRandomFeruchemicalGene(pawn);
        }
    }
}
