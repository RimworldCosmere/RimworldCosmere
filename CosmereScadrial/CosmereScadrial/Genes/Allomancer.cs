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

public class Allomancer : Gene_Resource {
    public MetallicArtsMetalDef metal =>
        MetallicArtsMetalDef.GetFromMetalDef(def.GetModExtension<MetalsLinked>().Metals.First()!);

    public override float InitialResourceMax => 1f;
    public override float MinLevelForAlert => .15f;
    public override float MaxLevelOffset => .1f;
    public override float Max => MetalReserves.MaxAmount;
    protected override Color BarColor => metal.color;
    protected override Color BarHighlightColor => metal.colorTwo ?? metal.color.SaturationChanged(50f);

    protected MetalReserves reserves => pawn.GetComp<MetalReserves>();
    public override float Value => reserves.GetReserve(metal);
    public override float ValuePercent => AllomancyUtility.GetReservePercent(pawn, metal);
    public override int ValueForDisplay => PostProcessValue(Value);
    public override int MaxForDisplay => PostProcessValue(Max);

    public override void Reset() {
        targetValue = 0.25f;
    }

    public override void PostAdd() {
        base.PostAdd();

        MetalbornUtility.HandleMistbornAndFullFeruchemistTraits(pawn);
        MetalbornUtility.HandleMetalbornTrait(pawn);
        InvestitureUtility.AssignHeighteningFromBEUs(pawn);
    }

    public override void PostRemove() {
        base.PostRemove();

        MetalbornUtility.HandleMistbornAndFullFeruchemistTraits(pawn);
        MetalbornUtility.HandleMetalbornTrait(pawn);
    }
}