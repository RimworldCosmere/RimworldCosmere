using RimWorld;
using Verse;
using Verse.Sound;
using SoundDefOf = Cosmere.Core.SoundDefOf;

namespace Cosmere.System.Roshar.Hediff;

public class ShardbladeSummoning : Verse.Hediff {
    private readonly int ticksPerHeartbeat = 60; // 1 second per heartbeat
    private int heartbeatCount;
    private int ticksSinceLastHeartbeat;

    public override string TipStringExtra {
        get {
            string tip = base.TipStringExtra;
            if (!tip.NullOrEmpty()) {
                tip += "\n";
            }

            tip += $"Heartbeats: {heartbeatCount}/10";
            return tip;
        }
    }

    public override void Tick() {
        base.Tick();

        ticksSinceLastHeartbeat++;

        if (ticksSinceLastHeartbeat >= ticksPerHeartbeat) {
            ticksSinceLastHeartbeat = 0;
            heartbeatCount++;

            // Optional: Play heartbeat sound
            if (heartbeatCount <= 10) {
                RimWorld.SoundDefOf.Interact_Sow.PlayOneShot(new TargetInfo(pawn.Position, pawn.Map));
            }

            if (heartbeatCount >= 10) {
                CompleteSummoning();
            }
        }
    }

    private void CompleteSummoning() {
        ThingDef shardbladeDef = DefDatabase<ThingDef>.GetNamed("Cosmere_Roshar_MeleeWeapon_Shardblade");
        ThingWithComps shardblade = (ThingWithComps)ThingMaker.MakeThing(shardbladeDef, RimWorld.ThingDefOf.Steel);

        if (pawn.equipment != null) {
            pawn.equipment.DropAllEquipment(pawn.Position, false);
            pawn.equipment.AddEquipment(shardblade);
        }

        SoundDefOf.Cosmere_Core_Sound_LoadingQuantumRiser.PlayOneShot(new TargetInfo(pawn.Position, pawn.Map));

        FleckMaker.Static(pawn.Position, pawn.Map, FleckDefOf.PsycastAreaEffect);

        Messages.Message(
            "CRO_ShardbladeSummoned".Translate(pawn.NameFullColored),
            pawn,
            MessageTypeDefOf.PositiveEvent
        );

        pawn.health.RemoveHediff(this);
    }

    public override void ExposeData() {
        base.ExposeData();
        Scribe_Values.Look(ref heartbeatCount, "heartbeatCount");
        Scribe_Values.Look(ref ticksSinceLastHeartbeat, "ticksSinceLastHeartbeat");
    }
}