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

            Logger.Important(
                $"AnimalControlSpike: {pawn.LabelShort} now has a drafter."
                + $"\n  IsColonist: {pawn.IsColonist}"
                + $"\n  IsColonistPlayerControlled: {pawn.IsColonistPlayerControlled}"
                + $"\n  playerSettings: {(pawn.playerSettings != null ? "yes" : "no")}"
                + $"\n  workSettings: {(pawn.workSettings != null ? "yes" : "no")}"
                + "\n  Select it and look for a draft button."
            );
        }
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

            Logger.Important(
                $"AnimalControlSpike: {pawn.LabelShort} ({pawn.def.defName})"
                + $"\n  intelligence: {pawn.RaceProps.intelligence}"
                + $"\n  Humanlike: {pawn.RaceProps.Humanlike} | Animal: {pawn.RaceProps.Animal}"
                + $"\n  IsColonist: {pawn.IsColonist}"
                + $"\n  IsColonistPlayerControlled: {pawn.IsColonistPlayerControlled}"
                + $"\n  drafter: {(pawn.drafter != null ? "yes" : "no")}"
                + $"\n  Drafted: {pawn.Drafted}"
            );
        }
    }
}
