using LudeonTK;
using RimWorld;
using Verse;

namespace Cosmere.Core.Dev;

/// <summary>
///     A spike, not a feature.
/// </summary>
/// <remarks>
///     Kandra animal forms need a controllable animal, and RimWorld does not offer one: a
///     <c>Pawn_DraftController</c> is only built for humanlike player pawns or colony mechs, and
///     <c>IsColonistPlayerControlled</c> wants <c>IsColonist</c>, which wants humanlike again.
///     <para>
///         Before committing to patching those gates, this answers the only question that
///         matters: hand a normal colony animal a drafter and see what the game actually does
///         with it. Delete this once the answer is known.
///     </para>
/// </remarks>
[StaticConstructorOnStartup]
public static class AnimalControlSpike {
    [DebugAction(
        "Cosmere/Core",
        "Spike: give animal a drafter",
        actionType = DebugActionType.ToolMap,
        allowedGameStates = AllowedGameStates.PlayingOnMap
    )]
    public static void GiveAnimalDrafter() {
        foreach (Verse.Thing thing in Find.CurrentMap.thingGrid.ThingsListAt(Verse.UI.MouseCell())) {
            if (thing is not Pawn pawn) continue;
            if (!pawn.RaceProps.Animal) continue;

            if (pawn.Faction != Faction.OfPlayer) {
                Logger.Warning($"AnimalControlSpike: {pawn.LabelShort} is not ours, so nothing to control.");
                continue;
            }

            pawn.drafter ??= new Pawn_DraftController(pawn);

            Report(pawn, "given a drafter");
        }
    }

    /// <summary>
    ///     Drops a tamed colony dog on the cursor and hands it a drafter, so the spike needs no
    ///     setup at all. Click an empty tile.
    /// </summary>
    [DebugAction(
        "Cosmere/Core",
        "Spike: spawn controllable dog",
        actionType = DebugActionType.ToolMap,
        allowedGameStates = AllowedGameStates.PlayingOnMap
    )]
    public static void SpawnControllableDog() {
        PawnKindDef? kind = DefDatabase<PawnKindDef>.GetNamedSilentFail("Husky")
                            ?? DefDatabase<PawnKindDef>.GetNamedSilentFail("LabradorRetriever")
                            ?? DefDatabase<PawnKindDef>.GetNamedSilentFail("YorkshireTerrier");
        if (kind == null) {
            Logger.Warning("AnimalControlSpike: no dog pawnkind found to spawn.");
            return;
        }

        Pawn dog = PawnGenerator.GeneratePawn(new PawnGenerationRequest(kind, Faction.OfPlayer));
        GenSpawn.Spawn(dog, Verse.UI.MouseCell(), Find.CurrentMap);

        dog.drafter ??= new Pawn_DraftController(dog);
        dog.playerSettings ??= new Pawn_PlayerSettings(dog);

        Report(dog, "spawned and given a drafter");
    }

    /// <summary>
    ///     The real question: does a humanlike-intelligence pawn with the Animal render tree draw
    ///     as a dog and still take orders? Click an empty tile.
    /// </summary>
    [DebugAction(
        "Cosmere/Core",
        "Spike: spawn kandra wolfhound",
        actionType = DebugActionType.ToolMap,
        allowedGameStates = AllowedGameStates.PlayingOnMap
    )]
    public static void SpawnKandraWolfhound() {
        PawnKindDef? kind = DefDatabase<PawnKindDef>.GetNamedSilentFail(
            "Cosmere_Scadrial_PawnKind_KandraWolfhound"
        );
        if (kind == null) {
            Logger.Warning("AnimalControlSpike: the kandra wolfhound pawnkind did not load.");
            return;
        }

        Pawn hound = PawnGenerator.GeneratePawn(new PawnGenerationRequest(kind, Faction.OfPlayer));
        GenSpawn.Spawn(hound, Verse.UI.MouseCell(), Find.CurrentMap);

        Report(hound, "spawned");
    }

    /// <summary>Reports what the game thinks of whatever is under the cursor.</summary>
    [DebugAction(
        "Cosmere/Core",
        "Spike: report pawn control flags",
        actionType = DebugActionType.ToolMap,
        allowedGameStates = AllowedGameStates.PlayingOnMap
    )]
    public static void ReportControlFlags() {
        foreach (Verse.Thing thing in Find.CurrentMap.thingGrid.ThingsListAt(Verse.UI.MouseCell())) {
            if (thing is not Pawn pawn) continue;

            Report(pawn, "inspected");
        }
    }

    private static void Report(Pawn pawn, string what) {
        Logger.Important(
            $"AnimalControlSpike: {pawn.LabelShort} ({pawn.def.defName}) {what}"
            + $"\n  intelligence: {pawn.RaceProps.intelligence}"
            + $"\n  Humanlike: {pawn.RaceProps.Humanlike} | Animal: {pawn.RaceProps.Animal}"
            + $"\n  IsColonist: {pawn.IsColonist}"
            + $"\n  IsColonistPlayerControlled: {pawn.IsColonistPlayerControlled}"
            + $"\n  drafter: {(pawn.drafter != null ? "yes" : "no")} | Drafted: {pawn.Drafted}"
            + "\n  Select it: is there a draft button? Right-click something: any orders?"
        );
    }
}
