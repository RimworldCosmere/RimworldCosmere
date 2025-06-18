using System.Linq;
using CosmereCore.Utils;
using CosmereResources.ModExtensions;
using CosmereScadrial.Comps.Things;
using CosmereScadrial.Defs;
using CosmereScadrial.Utils;
using RimWorld;
using UnityEngine;
using Verse;

namespace CosmereScadrial.Genes;

public class Metalborn : Gene_Resource {
    public MetallicArtsMetalDef metal =>
        MetallicArtsMetalDef.GetFromMetalDef(def.GetModExtension<MetalsLinked>().Metals.First()!);

    public override float InitialResourceMax => 1f;
    public override float MinLevelForAlert => .15f;
    public override float MaxLevelOffset => .1f;
    public override int MaxForDisplay => 100;
    public override float Max => MetalReserves.MAX_AMOUNT;
    protected override Color BarColor => metal.color;
    protected override Color BarHighlightColor => metal.colorTwo ?? metal.color.SaturationChanged(50f);

    public override float Value => pawn.GetComp<MetalReserves>().GetReserve(metal);
    public override float ValuePercent => Value / MetalReserves.MAX_AMOUNT * 100;

    public override void Reset() {
        targetValue = 0.25f;
    }

    public override void PostAdd() {
        base.PostAdd();

        MetalbornUtility.HandleMistbornAndFullFeruchemistTraits(pawn);
        MetalbornUtility.HandleMetalbornTrait(pawn);
        InvestitureUtility.AssignHeighteningFromBEUs(pawn);
        MetalbornUtility.HandleBurningMetalHediff(pawn);
    }

    public override void PostRemove() {
        base.PostRemove();

        MetalbornUtility.HandleMistbornAndFullFeruchemistTraits(pawn);
        MetalbornUtility.HandleMetalbornTrait(pawn);
        MetalbornUtility.HandleBurningMetalHediff(pawn);
    }
}