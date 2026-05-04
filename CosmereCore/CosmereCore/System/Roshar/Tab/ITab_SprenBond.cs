using Cosmere.System.Roshar.Comp.Thing;
using Cosmere.System.Roshar.Gene;
using Cosmere.System.Roshar.Surgebinding;
using Cosmere.System.Roshar.UI;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.System.Roshar.Tab;

public class ITab_SprenBond : ITab {
    public ITab_SprenBond() {
        labelKey = "CRO_Spren_Tab";
        size = new Vector2(400f, 320f);
    }

    private SprenBond? SprenBond {
        get {
            SprenBond? direct = SelPawn?.TryGetComp<SprenBond>();
            if (direct != null) return direct;

            Surgebinder? surgebinder = SelPawn?.genes?.GetFirstGeneOfType<Surgebinder>();
            return surgebinder?.bondedSpren?.TryGetComp<SprenBond>();
        }
    }

    private bool IsSprenSide => SelPawn?.TryGetComp<SprenBond>() != null;

    public override bool IsVisible => SprenBond?.BondedRadiant != null;

    protected override void FillTab() {
        SprenBond? bond = SprenBond;
        if (bond?.BondedRadiant == null) return;

        Pawn spren = (Pawn)bond.parent;
        Pawn radiant = bond.BondedRadiant;

        Rect rect = new Rect(0f, 0f, size.x, size.y).ContractedBy(17f);
        SprenBondDetailRenderer.Render(rect, spren, radiant, bond, IsSprenSide, 73948201);
    }
}