using Cosmere.Core.Comp.Game;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace Cosmere.System.Roshar.WorldObject;

[StaticConstructorOnStartup]
public class NightwatcherValley : RimWorld.Planet.WorldObject {
    private static readonly Texture2D Icon =
        ContentFinder<Texture2D>.Get("UI/Icons/Abilities/CultivationsPerpendicularity", false) ?? BaseContent.BadTex;

    private HashSet<int> notifiedCaravans = [];

    protected override void Tick() {
        base.Tick();
        if (GenTicks.TicksGame % 250 != 0) return;
        CheckForArrivingCaravans();
    }

    private void CheckForArrivingCaravans() {
        List<Caravan> caravans = Find.WorldObjects.Caravans;
        for (int i = 0; i < caravans.Count; i++) {
            Caravan caravan = caravans[i];
            if (caravan.Tile != Tile) continue;
            if (notifiedCaravans.Contains(caravan.ID)) continue;

            notifiedCaravans.Add(caravan.ID);
            TriggerEncounterForCaravan(caravan);
        }
    }

    private void TriggerEncounterForCaravan(Caravan caravan) {
        Shards? shards = Current.Game.GetComponent<Shards>();
        if (shards == null || !shards.IsEnabled("Cultivation")) return;

        List<Pawn> pawns = caravan.PawnsListForReading;
        for (int i = 0; i < pawns.Count; i++) {
            if (!NightwatcherSystem.IsEligible(pawns[i])) continue;
            NightwatcherSystem.InitiateSeek(pawns[i]);
        }
    }

    public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan) {
        yield break;
    }

    public override IEnumerable<Verse.Gizmo> GetCaravanGizmos(Caravan caravan) {
        foreach (Verse.Gizmo g in base.GetCaravanGizmos(caravan)) {
            yield return g;
        }

        Shards? shards = Current.Game.GetComponent<Shards>();
        if (shards == null || !shards.IsEnabled("Cultivation")) yield break;

        List<Pawn> pawns = caravan.PawnsListForReading;
        for (int i = 0; i < pawns.Count; i++) {
            Pawn pawn = pawns[i];
            if (!NightwatcherSystem.IsEligible(pawn)) continue;

            Pawn captured = pawn;
            yield return new Command_Action {
                defaultLabel = "Cosmere_Roshar_SeekNightwatcher_Label".Translate() + ": " + pawn.LabelShort,
                defaultDesc = "Cosmere_Roshar_SeekNightwatcher_Desc".Translate(pawn.Named("PAWN")),
                icon = Icon,
                action = () => NightwatcherSystem.InitiateSeek(captured),
            };
        }
    }

    public override void ExposeData() {
        base.ExposeData();
        Scribe_Collections.Look(ref notifiedCaravans, "notifiedCaravans", LookMode.Value);
        notifiedCaravans ??= [];
    }
}