using UnityEngine;
using Verse;

namespace Cosmere.System.Roshar.Surgebinding;

public class BondViolationLogEntry : LogEntry {
    internal Pawn pawn;
    internal string orderLabel;
    internal string reason;
    internal string severityLabel;

    public BondViolationLogEntry() { }

    public BondViolationLogEntry(Pawn pawn, string orderLabel, string reason, string severityLabel) {
        this.pawn = pawn;
        this.orderLabel = orderLabel;
        this.reason = reason;
        this.severityLabel = severityLabel;
    }

    public override bool Concerns(Verse.Thing t) {
        return t == pawn;
    }

    public override IEnumerable<Verse.Thing> GetConcerns() {
        if (pawn != null) yield return pawn;
    }

    public new string ToGameStringFromPOV(Verse.Thing pov, bool forceLog = false) {
        if (pov != pawn) return ToString();

        return $"Bond strained ({severityLabel}): {reason}";
    }

    public override void ClickedFromPOV(Verse.Thing pov) { }

    public override Texture2D? IconFromPOV(Verse.Thing pov) {
        return null;
    }

    public override string ToString() {
        return $"{pawn?.NameShortColored ?? "Unknown"}'s {orderLabel} bond strained ({severityLabel}): {reason}";
    }

    public override void ExposeData() {
        base.ExposeData();
        Scribe_References.Look(ref pawn, "pawn");
        Scribe_Values.Look(ref orderLabel, "orderLabel", "");
        Scribe_Values.Look(ref reason, "reason", "");
        Scribe_Values.Look(ref severityLabel, "severityLabel", "");
    }
}
