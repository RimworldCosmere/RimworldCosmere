using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.Sound;

namespace Cosmere.Roshar.Surgebinding.Ability;

public class Shardblade : SurgebindingAbility {
    private static readonly ThingDef ShardbladeDef = ThingDefOf.Cosmere_Roshar_MeleeWeapon_Shardblade;

    public Shardblade(Pawn pawn) : base(pawn) { }
    public Shardblade(Pawn pawn, AbilityDef def) : base(pawn, def) { }

    public override AcceptanceReport CanCast => base.CanCast && PawnHasShardblade();

    private bool PawnHasShardblade() {
        if (gene.currentIdeal >= 2) return true;
        if (pawn.equipment?.Primary?.def == ShardbladeDef) return true;

        return pawn.inventory?.innerContainer.InnerListForReading.Exists(t => t.def == ShardbladeDef) ?? false;
    }

    public override void QueueCastingJob(GlobalTargetInfo targetInfo) {
        if (gene.currentIdeal >= 2) {
            SummonBladeInstantly();
            return;
        }

        StartDeadBladeSummoning();
    }

    protected override void OnDisable() {
        base.OnDisable();
        if (pawn.equipment?.Primary?.def != ShardbladeDef) return;

        pawn.equipment.Primary.Destroy();

        SoundDefOf.PsychicPulseGlobal.PlayOneShot(new TargetInfo(pawn.Position, pawn.Map));

        FleckMaker.Static(pawn.Position, pawn.Map, FleckDefOf.PsycastAreaEffect, 2f);

        Messages.Message(
            "CRO_ShardbladeDismissed".Translate(pawn.NameFullColored),
            pawn,
            MessageTypeDefOf.NeutralEvent
        );
    }

    private void SummonBladeInstantly() {
        ThingDef shardbladeDef = ThingDefOf.Cosmere_Roshar_MeleeWeapon_Shardblade;
        ThingWithComps shardblade = (ThingWithComps)ThingMaker.MakeThing(shardbladeDef, radiantOrder.gemstone.Item);

        if (pawn.equipment != null) {
            // @todo Maybe dont drop it, but put it in inventory?
            pawn.equipment.DropAllEquipment(pawn.Position, false);
            pawn.equipment.AddEquipment(shardblade);
        }

        SoundDefOf.PsychicPulseGlobal.PlayOneShot(new TargetInfo(pawn.Position, pawn.Map));

        FleckMaker.Static(pawn.Position, pawn.Map, FleckDefOf.PsycastAreaEffect, 2f);

        Messages.Message(
            "CRO_ShardbladeSummoned".Translate(pawn.NameFullColored),
            pawn,
            MessageTypeDefOf.NeutralEvent
        );
    }

    private void StartDeadBladeSummoning() {
        Messages.Message(
            "CRO_DeadBladeSummoning".Translate(pawn.NameFullColored),
            pawn,
            MessageTypeDefOf.NeutralEvent
        );

        pawn.health.AddHediff(HediffDefOf.Cosmere_Roshar_Hediff_ShardbladeSummoning);
    }
}