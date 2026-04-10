using Verse;

namespace Cosmere.System.Roshar.Surgebinding.Hediff;

public class BondsmithCalling : HediffWithComps {
    public string sprenName = "";

    public override string LabelBase => "CRO_Bondsmith_Calling_Label".Translate(sprenName);

    public override void ExposeData() {
        base.ExposeData();
        Scribe_Values.Look(ref sprenName, "sprenName", "");
    }
}
