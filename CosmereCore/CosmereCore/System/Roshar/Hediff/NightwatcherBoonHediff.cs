using Cosmere.System.Roshar.Def;
using Verse;

namespace Cosmere.System.Roshar.Hediff;

public class NightwatcherBoonHediff : NightwatcherPassiveHediff {
    private NightwatcherBoonDef boonDef = null!;

    public NightwatcherBoonDef Boon => boonDef;
    public override string LabelBase => $"Nightwatcher: {boonDef?.LabelCap ?? "unknown"}";

    public override string Description {
        get {
            string intro =
                "The Nightwatcher has granted {PAWN_nameDef} a boon. The gift is permanent, woven into {PAWN_possessive} very Spiritweb.";
            return (intro + "\n\n" + (boonDef?.description ?? "")).Formatted(pawn.Named("PAWN"));
        }
    }

    public void Initialize(NightwatcherBoonDef def) {
        boonDef = def;
        Severity = 1f;
    }

    public override bool TryMergeWith(Verse.Hediff other) {
        return false;
    }

    public override void ExposeData() {
        base.ExposeData();
        Scribe_Defs.Look(ref boonDef, "boonDef");
    }
}