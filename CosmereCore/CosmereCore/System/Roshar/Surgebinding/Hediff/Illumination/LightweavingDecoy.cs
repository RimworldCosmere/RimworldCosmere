using RimWorld;
using Verse;
using DecoyAbility = Cosmere.System.Roshar.Surgebinding.Ability.Illumination.LightweavingDecoy;

namespace Cosmere.System.Roshar.Surgebinding.Hediff.Illumination;

public class LightweavingDecoy : Verse.Hediff {
    private static HediffDef? cachedDef;

    public Verse.Pawn? caster;

    public static HediffDef Def =>
        cachedDef ??= DefDatabase<HediffDef>.GetNamed("Cosmere_Roshar_Hediff_LightweavingDecoy");

    public override void Tick() {
        base.Tick();
    }

    public override string TipStringExtra {
        get {
            string tip = "CRO_LightweavingDecoy_Inspect".Translate();
            if (caster != null) {
                tip += "\n" + "CRO_LightweavingDecoy_CasterLabel".Translate(caster.NameShortColored.Named("PAWN"));
            }
            return tip;
        }
    }

    public static bool IsDecoy(Verse.Pawn pawn) {
        return pawn.health?.hediffSet?.GetFirstHediffOfDef(Def) != null;
    }

    public override void PostRemoved() {
        base.PostRemoved();
        DecoyAbility.RemoveDecoy(pawn);
    }

    public override void ExposeData() {
        base.ExposeData();
        Scribe_References.Look(ref caster, "caster");
    }
}
