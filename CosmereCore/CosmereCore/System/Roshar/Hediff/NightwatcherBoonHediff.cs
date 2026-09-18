using Cosmere.System.Roshar.Def;
using Cosmere.System.Roshar.Nightwatcher;
using Verse;

namespace Cosmere.System.Roshar.Hediff;

public class NightwatcherBoonHediff : NightwatcherPassiveHediff {
    private NightwatcherBoonDef boonDef = null!;
    private string? choiceKey;

    public NightwatcherBoonDef Boon => boonDef;

    public override string LabelBase => $"Nightwatcher: {boonDef?.LabelCap ?? "unknown"}";

    public override string Description {
        get {
            string intro =
                "The Nightwatcher has granted {PAWN_nameDef} a boon. The gift is permanent, woven into {PAWN_possessive} very Spiritweb.";
            return (intro + "\n\n" + (boonDef?.description ?? string.Empty)).Formatted(pawn.Named("PAWN"));
        }
    }

    public override string TipStringExtra {
        get {
            string tip = base.TipStringExtra;
            if (boonDef == null) return tip;

            string effects = NightwatcherEffectText.Boon(boonDef, choiceKey);
            if (effects.Length == 0) return tip;

            return tip.NullOrEmpty() ? effects : tip + "\n" + effects;
        }
    }

    public void Initialize(NightwatcherBoonDef def, string? selectedDefName = null) {
        boonDef = def;
        choiceKey = selectedDefName;
        Severity = 1f;
    }

    public override bool TryMergeWith(Verse.Hediff other) {
        return false;
    }

    public override void ExposeData() {
        base.ExposeData();
        Scribe_Defs.Look(ref boonDef, "boonDef");
        Scribe_Values.Look(ref choiceKey, "choiceKey");
    }
}
