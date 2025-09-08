using RimWorld;
using Verse;
using Verse.Sound;
using SoundDefOf = Cosmere.Core.SoundDefOf;

namespace Cosmere.Roshar.Surgebinding.Ability;

public class Shardblade : SurgebindingAbility {
    private static readonly ThingDef ShardbladeDef = ThingDefOf.Cosmere_Roshar_MeleeWeapon_RadiantShardblade;

    private readonly List<ThingWithComps> previousEquipment = [];

    public Shardblade(Pawn pawn) : base(pawn) { }
    public Shardblade(Pawn pawn, AbilityDef def) : base(pawn, def) { }

    public override AcceptanceReport CanCast => base.CanCast && PawnHasShardblade();

    private bool PawnHasShardblade() {
        if (gene.currentIdeal >= 2) return true;
        if (pawn.equipment?.Primary?.def == ShardbladeDef) return true;

        return pawn.inventory?.innerContainer.InnerListForReading.Exists(t => t.def == ShardbladeDef) ?? false;
    }

    protected override void OnEnable() {
        if (pawn.equipment != null) {
            foreach (ThingWithComps equipment in pawn.equipment.AllEquipmentListForReading) {
                if (pawn.inventory.innerContainer.TryAdd(equipment)) {
                    previousEquipment.Add(equipment);
                    equipment.DeSpawn();
                } else {
                    pawn.equipment.TryDropEquipment(equipment, out _, pawn.Position);
                }
            }
        }

        if (gene.currentIdeal >= 2) {
            SummonBladeInstantly();
            return;
        }

        StartDeadBladeSummoning();
    }

    protected override void OnDisable() {
        base.OnDisable();

        pawn.equipment.Primary.Destroy();
        pawn.equipment.bondedWeapon = null;
        foreach (ThingWithComps equipment in previousEquipment) {
            pawn.equipment.AddEquipment(equipment);
        }

        previousEquipment.Clear();

        SoundDefOf.Cosmere_Core_Sound_LoadingQuantumRiser.PlayOneShot(new TargetInfo(pawn.Position, pawn.Map));

        FleckMaker.Static(pawn.Position, pawn.Map, FleckDefOf.PsycastAreaEffect);
    }

    private void SummonBladeInstantly() {
        ThingWithComps shardblade = (ThingWithComps)ThingMaker.MakeThing(ShardbladeDef, radiantOrder.gemstone.Item);

        if (pawn.equipment != null) {
            pawn.equipment.AddEquipment(shardblade);
            pawn.equipment.bondedWeapon = shardblade;
        }

        SoundDefOf.Cosmere_Core_Sound_LoadingQuantumRiser.PlayOneShot(new TargetInfo(pawn.Position, pawn.Map));

        FleckMaker.Static(pawn.Position, pawn.Map, FleckDefOf.PsycastAreaEffect);
    }

    private void StartDeadBladeSummoning() {
        pawn.health.AddHediff(HediffDefOf.Cosmere_Roshar_Hediff_ShardbladeSummoning);
    }
}