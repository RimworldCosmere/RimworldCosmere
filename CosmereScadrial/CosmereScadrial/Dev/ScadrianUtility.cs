using System.Collections.Generic;
using System.Linq;
using CosmereScadrial.Comps.Things;
using CosmereScadrial.Defs;
using CosmereScadrial.Utils;
using LudeonTK;
using RimWorld;
using Verse;
using GeneUtility = CosmereScadrial.Utils.GeneUtility;

namespace CosmereScadrial.Dev;

[StaticConstructorOnStartup]
public static class ScadrianUtility {
    [DebugAction("Cosmere/Scadrial", "Prepare Dev Pawn",
        actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
    public static void PrepareDevPawn(Pawn pawn) {
        if (pawn.genes == null) return;
        if (!pawn.story.traits.HasTrait(TraitDefOf.Cosmere_Mistborn)) {
            GeneUtility.AddMistborn(pawn, false, true);
            Messages.Message($"Made {pawn.NameFullColored} a mistborn", pawn, MessageTypeDefOf.PositiveEvent);
        }

        FillAllReserves(pawn);
        GiveAllAllomanticVials(pawn);
    }

    [DebugAction("Cosmere/Scadrial", "Give All Allomantic Vials",
        actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
    public static void GiveAllAllomanticVials(Pawn pawn) {
        foreach (MetallicArtsMetalDef? metal in DefDatabase<MetallicArtsMetalDef>.AllDefsListForReading.Where(x =>
                     !x.godMetal && x.allomancy != null)) {
            ThingDef? vialDef = DefDatabase<ThingDef>.GetNamed($"Cosmere_AllomanticVial_{metal.LabelCap}");
            if (pawn.inventory.innerContainer.Contains(vialDef)) continue;

            Thing? vial = ThingMaker.MakeThing(vialDef);
            vial.stackCount = 20;
            pawn.inventory.innerContainer.TryAdd(vial);
        }

        Messages.Message($"Gave {pawn.NameFullColored} 20 vials of each metal.", pawn,
            MessageTypeDefOf.PositiveEvent);
    }

    [DebugAction("Cosmere/Scadrial", "Prepare All Dev Pawns", allowedGameStates = AllowedGameStates.PlayingOnMap)]
    public static void PrepareAllDevPawn() {
        foreach (Pawn? pawn in Find.CurrentMap.mapPawns.AllPawnsSpawned) {
            PrepareDevPawn(pawn);
        }
    }

    [DebugAction("Cosmere/Scadrial", "Fill allomantic reserves (all metals)",
        actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
    public static void FillAllReserves(Pawn pawn) {
        MetalReserves? comp = pawn.GetComp<MetalReserves>();
        foreach (MetallicArtsMetalDef? metal in DefDatabase<MetallicArtsMetalDef>.AllDefsListForReading) {
            comp.SetReserve(metal, MetalReserves.MAX_AMOUNT);
        }

        Messages.Message($"Gave {pawn.NameFullColored} full reserves reserves for all metals", pawn,
            MessageTypeDefOf.PositiveEvent);
    }

    [DebugAction("Cosmere/Scadrial", "Wipe allomantic reserves (all metals)",
        actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
    public static void WipeAllReserves(Pawn pawn) {
        MetalReserves? comp = pawn.GetComp<MetalReserves>();
        foreach (MetallicArtsMetalDef? metal in DefDatabase<MetallicArtsMetalDef>.AllDefs) comp.RemoveReserve(metal);

        Messages.Message($"Wiped all reserves for {pawn.LabelShort}", pawn, MessageTypeDefOf.PositiveEvent);
    }

    [DebugAction("Cosmere/Scadrial", "Fill specific metal reserve", actionType = DebugActionType.ToolMapForPawns,
        allowedGameStates = AllowedGameStates.PlayingOnMap)]
    public static void FillSpecificMetal(Pawn pawn) {
        List<DebugMenuOption> options = new List<DebugMenuOption>();

        MetalReserves? comp = pawn.GetComp<MetalReserves>();
        foreach (MetallicArtsMetalDef? metal in DefDatabase<MetallicArtsMetalDef>.AllDefs) {
            string? label = metal.label.CapitalizeFirst();
            options.Add(new DebugMenuOption(label, DebugMenuOptionMode.Action, () => {
                comp.SetReserve(metal, MetalReserves.MAX_AMOUNT);
                Messages.Message($"Filled {label} for {pawn.LabelShort}", pawn, MessageTypeDefOf.PositiveEvent);
            }));
        }

        Find.WindowStack.Add(new Dialog_DebugOptionListLister(options));
    }

    [DebugAction("Cosmere/Scadrial", "Wipe specific metal reserve", actionType = DebugActionType.ToolMapForPawns,
        allowedGameStates = AllowedGameStates.PlayingOnMap)]
    public static void WipeSpecificMetal(Pawn pawn) {
        List<DebugMenuOption> options = new List<DebugMenuOption>();

        MetalReserves? comp = pawn.GetComp<MetalReserves>();
        foreach (MetallicArtsMetalDef? metal in DefDatabase<MetallicArtsMetalDef>.AllDefs) {
            string? label = metal.label.CapitalizeFirst();
            options.Add(new DebugMenuOption(label, DebugMenuOptionMode.Action, () => {
                comp.SetReserve(metal, 0);
                Messages.Message($"Wiped {label} for {pawn.LabelShort}", pawn, MessageTypeDefOf.PositiveEvent);
            }));
        }

        Find.WindowStack.Add(new Dialog_DebugOptionListLister(options));
    }

    [DebugAction("Cosmere/Scadrial", "Snap Pawn",
        actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
    public static void SnapPawn(Pawn pawn) {
        SnapUtility.TrySnap(pawn);
    }

    [DebugAction("Cosmere/Scadrial", "Try Give Random Allomantic Ability",
        actionType = DebugActionType.ToolMapForPawns,
        allowedGameStates = AllowedGameStates.PlayingOnMap)]
    public static void TryGiveRandomAllomanticAbility(Pawn pawn) {
        if (Rand.Chance(1f / 16f)) {
            GeneUtility.AddMistborn(pawn);
        } else {
            GeneUtility.TryAddRandomAllomanticGene(pawn);
        }
    }

    [DebugAction("Cosmere/Scadrial", "Try Give Random Feruchemical Ability",
        actionType = DebugActionType.ToolMapForPawns,
        allowedGameStates = AllowedGameStates.PlayingOnMap)]
    public static void TryGiveRandomFeruchemicalAbility(Pawn pawn) {
        if (Rand.Chance(1f / 16f)) {
            GeneUtility.AddFullFeruchemist(pawn);
        } else {
            GeneUtility.TryAddRandomFeruchemicalGene(pawn);
        }
    }
}