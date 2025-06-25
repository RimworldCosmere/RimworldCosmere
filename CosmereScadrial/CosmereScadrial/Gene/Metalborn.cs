using System.Linq;
using CosmereCore.Util;
using CosmereResources.DefModExtension;
using CosmereScadrial.Def;
using CosmereScadrial.Extension;
using CosmereScadrial.Util;
using RimWorld;
using UnityEngine;
using Verse;

namespace CosmereScadrial.Gene;

public abstract class Metalborn : Gene_Resource {
    public MetallicArtsMetalDef metal => def.GetModExtension<MetalsLinked>().Metals.First()!.ToMetallicArts();

    public override float InitialResourceMax => 1f;
    public override float MinLevelForAlert => .15f;
    public override float MaxLevelOffset => .1f;

    // @TODO IMPLEMENT
    public override float Max => 100f;
    protected override Color BarColor => metal.color;
    protected override Color BarHighlightColor => metal.colorTwo ?? metal.color.SaturationChanged(50f);

    // @TODO IMPLEMENT
    public override float Value => 0f;

    // @TODO IMPLEMENT
    public override float ValuePercent => 0f;

    public override int ValueForDisplay => PostProcessValue(Value);
    public override int MaxForDisplay => PostProcessValue(Max);

    public override void Reset() {
        targetValue = 0.5f;
    }

    protected abstract void PostAddOrRemove();

    public override void PostAdd() {
        base.PostAdd();
        PostAddOrRemove();

        MetalbornUtility.HandleMetalbornTrait(pawn);
        InvestitureUtility.AssignHeighteningFromBEUs(pawn);
    }

    public override void PostRemove() {
        base.PostRemove();
        PostAddOrRemove();

        MetalbornUtility.HandleMetalbornTrait(pawn);
        InvestitureUtility.AssignHeighteningFromBEUs(pawn);
    }
}