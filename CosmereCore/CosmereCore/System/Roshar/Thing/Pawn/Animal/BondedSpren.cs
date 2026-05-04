using Cosmere.System.Roshar.Comp.Thing;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Thing.Pawn.Animal;

public class BondedSpren : Spren {
    public SprenBond? SprenBond => this.TryGetComp<SprenBond>();

    public override void SpawnSetup(Map map, bool respawningAfterLoad) {
        base.SpawnSetup(map, respawningAfterLoad);
        playerSettings ??= new Pawn_PlayerSettings(this);
        playerSettings.hostilityResponse = HostilityResponseMode.Ignore;
    }

    public override string GetInspectString() {
        SprenBond? bond = SprenBond;
        if (bond?.BondedRadiant == null) return base.GetInspectString();

        string result = "CRO_NahelBond_Inspect".Translate(bond.BondedRadiant.NameShortColored.Named("PAWN"));
        string baseStr = base.GetInspectString();
        if (!baseStr.NullOrEmpty()) {
            result += "\n" + baseStr;
        }

        return result;
    }
}