using Verse;

namespace Cosmere.System.Roshar.Surgebinding.Hediff;

public class BondsmithCalling : HediffWithComps {
    public string sprenName = string.Empty;

    public override string LabelBase => "CRO_Bondsmith_Calling_Label".Translate(sprenName);

    public override void ExposeData() {
        base.ExposeData();
        Scribe_Values.Look(ref sprenName, "sprenName", string.Empty);
    }
}
