using System;
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
    internal bool gizmoShrunk = true;
    public MetallicArtsMetalDef metal => def.GetModExtension<MetalsLinked>().Metals.First()!.ToMetallicArts();

    public override float InitialResourceMax => 1f;
    public override float MinLevelForAlert => .15f;
    public override float MaxLevelOffset => .1f;

    protected override Color BarColor => metal.color.SaturationChanged(1f);
    protected override Color BarHighlightColor => metal.color.SaturationChanged(2f);

    public override float Max => throw new NotImplementedException();
    public override float Value => throw new NotImplementedException();

    public override float ValuePercent => Max > 0 ? Value / Max : 0;

    public override int ValueForDisplay => PostProcessValue(Value);
    public override int MaxForDisplay => PostProcessValue(Max);

    public override void ExposeData() {
        base.ExposeData();
        Scribe_Values.Look(ref gizmoShrunk, "gizmoShrunk");
    }

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