using Cosmere.System.Roshar.Def;
using Verse;

namespace Cosmere.System.Roshar.Hediff;

public class NightwatcherCurseHediff : NightwatcherPassiveHediff {
    private NightwatcherCurseDef curseDef = null!;

    public NightwatcherCurseDef Curse => curseDef;
    public override string LabelBase => $"Nightwatcher: {curseDef?.LabelCap ?? "unknown"}";

    public override string Description {
        get {
            string intro = "The Nightwatcher has laid a curse upon {PAWN_nameDef}. It cannot be removed by any ordinary means.";
            return (intro + "\n\n" + (curseDef?.description ?? "")).Formatted(pawn.Named("PAWN"));
        }
    }

    public void Initialize(NightwatcherCurseDef def) {
        curseDef = def;
        Severity = 1f;
    }

    public override bool TryMergeWith(Verse.Hediff other) => false;

    public override void ExposeData() {
        base.ExposeData();
        Scribe_Defs.Look(ref curseDef, "curseDef");
    }
}
