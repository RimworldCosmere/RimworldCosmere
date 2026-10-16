using Cosmere.System.Roshar.Surgebinding.Ability.Illumination;
using Verse;

namespace Cosmere.System.Roshar.Surgebinding.Hediff.Illumination;

public class LightweavingDecoy : Verse.Hediff {
    public Pawn? caster;

    public static HediffDef Def => HediffDefOf.Cosmere_Roshar_Hediff_LightweavingDecoy;

    public override string TipStringExtra {
        get {
            string tip = "CRO_LightweavingDecoy_Inspect".Translate();
            if (caster != null) {
                tip += "\n" + "CRO_LightweavingDecoy_CasterLabel".Translate(caster.NameShortColored.Named("PAWN"));
            }

            return tip;
        }
    }

    public static bool IsDecoy(Pawn pawn) {
        return pawn.health?.hediffSet?.GetFirstHediffOfDef(Def) != null;
    }

    public override void PostRemoved() {
        base.PostRemoved();
        LightweavingDecoyRegistry.Remove(pawn);
    }

    public override void ExposeData() {
        base.ExposeData();
        Scribe_References.Look(ref caster, "caster");
    }
}
