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
        ReleaseIfImprisoned();
    }

    /// <summary>
    ///     Lets out a spren jailed before arresting one was blocked.
    /// </summary>
    /// <remarks>
    ///     The prisoner tab offers nothing to a creature with no needs and no faction, so a spren
    ///     put in a cell stayed there. Nothing arrests one now, and this frees the ones that
    ///     already were.
    /// </remarks>
    private void ReleaseIfImprisoned() {
        if (guest is not { IsPrisoner: true }) return;

        guest.SetGuestStatus(null);
        Log.Info($"Released {LabelShort}, which had been imprisoned.");
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
