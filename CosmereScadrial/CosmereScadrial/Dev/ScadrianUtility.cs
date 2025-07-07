using System.Collections.Generic;
using System.Linq;
using CosmereScadrial.Def;
using CosmereScadrial.Extension;
using CosmereScadrial.Gene;
using CosmereScadrial.Util;
using LudeonTK;
using RimWorld;
using Verse;
using GeneUtility = CosmereScadrial.Util.GeneUtility;

namespace CosmereScadrial.Dev;

[StaticConstructorOnStartup]
public static class ScadrianUtility {
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
        List<DebugMenuOption> options = new List<DebugMenuOption>();

        foreach (MetallicArtsMetalDef? metal in DefDatabase<MetallicArtsMetalDef>.AllDefs) {
            Allomancer gene = pawn.genes.GetAllomanticGeneForMetal(metal)!;
            string? label = metal.label.CapitalizeFirst();
            options.Add(
                new DebugMenuOption(
                    label,
                    DebugMenuOptionMode.Action,
                    () => {
                        gene.SetReserve(gene.Max);
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
        List<DebugMenuOption> options = new List<DebugMenuOption>();

        foreach (MetallicArtsMetalDef? metal in DefDatabase<MetallicArtsMetalDef>.AllDefs) {
            Allomancer gene = pawn.genes.GetAllomanticGeneForMetal(metal)!;
            string? label = metal.label.CapitalizeFirst();
            options.Add(
                new DebugMenuOption(
                    label,
                    DebugMenuOptionMode.Action,
                    () => {
                        gene.SetReserve(0);
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
        SnapUtility.TrySnap(pawn);
    }

    [DebugAction(
        "Cosmere/Scadrial",
        "Try Give Random Allomantic Ability",
        actionType = DebugActionType.ToolMapForPawns,
        allowedGameStates = AllowedGameStates.PlayingOnMap
    )]
    public static void TryGiveRandomAllomanticAbility(Pawn pawn) {
        if (Rand.Chance(1f / 16f)) {
            GeneUtility.AddMistborn(pawn);
        } else {
            GeneUtility.TryAddRandomAllomanticGene(pawn);
        }
    }

    [DebugAction(
        "Cosmere/Scadrial",
        "Try Give Random Feruchemical Ability",
        actionType = DebugActionType.ToolMapForPawns,
        allowedGameStates = AllowedGameStates.PlayingOnMap
    )]
    public static void TryGiveRandomFeruchemicalAbility(Pawn pawn) {
        if (Rand.Chance(1f / 16f)) {
            GeneUtility.AddFullFeruchemist(pawn);
        } else {
            GeneUtility.TryAddRandomFeruchemicalGene(pawn);
        }
    }
}