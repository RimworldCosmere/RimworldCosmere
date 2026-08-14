using LudeonTK;
using RimWorld;
using Verse;

namespace Cosmere.System.Scadrial.Dev;

/// <summary>
///     Prints what a kandra actually looks like, and why.
/// </summary>
/// <remarks>
///     A missing face has several possible causes that all look identical on screen - a null head
///     type, a head type whose graphic never loaded, a render node vetoed by the shape subworker,
///     or a true body that was captured before the pawn finished being generated. Guessing between
///     them wasted a launch, so this asks.
/// </remarks>
public static class KandraAppearanceReport {
    [DebugAction(
        "Cosmere/Core",
        "Report kandra appearance",
        actionType = DebugActionType.ToolMap,
        allowedGameStates = AllowedGameStates.PlayingOnMap
    )]
    public static void Report() {
        foreach (Verse.Thing thing in Find.CurrentMap.thingGrid.ThingsListAt(Verse.UI.MouseCell())) {
            if (thing is not Pawn pawn) continue;

            Kandra.CompKandraForms? forms = pawn.TryGetComp<Kandra.CompKandraForms>();
            Kandra.KandraForm? worn = forms?.Current;
            Kandra.KandraForm? own = forms?.TrueBody;

            Cosmere.Core.Logger.Important(
                $"Kandra appearance: {pawn.LabelShort}"
                + $"\n  IsColonist: {pawn.IsColonist} | forms comp: {(forms == null ? "none" : "yes")}"
                + $"\n  wearing: {Describe(worn)}"
                + $"\n  true body: {Describe(own)}"
                + $"\n  live story: headType={pawn.story?.headType?.defName ?? "NULL"}"
                + $" bodyType={pawn.story?.bodyType?.defName ?? "NULL"}"
                + $" hair={pawn.story?.hairDef?.defName ?? "NULL"}"
                + $"\n  live gender: {pawn.gender} | skinOverride: {pawn.story?.skinColorOverride?.ToString() ?? "none"}"
                + $"\n  shape hediff: {(pawn.health?.hediffSet?.HasHediff(HediffFor()) == true ? "on" : "off")}"
                + $"\n  known forms: {forms?.Known.Count ?? 0} | cover blown: {forms?.CoverBlown}"
            );
        }
    }

    private static HediffDef? HediffFor() {
        return DefDatabase<HediffDef>.GetNamedSilentFail("Cosmere_Scadrial_Hediff_AnimalShape");
    }

    private static string Describe(Kandra.KandraForm? form) {
        if (form == null) return "nothing (own shape)";
        if (form.IsAnimal) return $"animal {form.animalKind?.defName}";

        return $"{form.Label} headType={form.headType?.defName ?? "NULL"}"
               + $" bodyType={form.bodyType?.defName ?? "NULL"}"
               + $" hair={form.hair?.defName ?? "NULL"} gender={form.gender}";
    }

    /// <summary>
    ///     Runs the koloss transformation on whoever is under the cursor, with no bill involved.
    /// </summary>
    /// <remarks>
    ///     The surgery has a lot between the player and the code - a doctor, a bed, four spikes and
    ///     a filter. When the result is wrong, this says whether the transformation itself is at
    ///     fault or whether the bill simply never ran.
    /// </remarks>
    [DebugAction(
        "Cosmere/Core",
        "Spike: make a koloss here",
        actionType = DebugActionType.ToolMap,
        allowedGameStates = AllowedGameStates.PlayingOnMap
    )]
    public static void MakeKolossHere() {
        foreach (Verse.Thing thing in Find.CurrentMap.thingGrid.ThingsListAt(Verse.UI.MouseCell())) {
            if (thing is not Pawn pawn) continue;
            if (pawn.genes == null) continue;

            XenotypeDef? koloss = DefDatabase<XenotypeDef>.GetNamedSilentFail(
                Util.KolossUtility.KolossXenotype
            );
            if (koloss == null) {
                Cosmere.Core.Logger.Warning("The koloss xenotype is missing.");

                return;
            }

            Util.KolossUtility.MakeFrom(pawn, koloss);

            return;
        }
    }
}
