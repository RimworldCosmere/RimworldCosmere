using Cosmere.Core.Gene;
using Cosmere.DefModExtension;
using Cosmere.System.Scadrial.Def;
using Cosmere.System.Scadrial.Utility;
using UnityEngine;
using Verse;

namespace Cosmere.System.Scadrial.Gene;

public abstract class Metalborn : Invested {
    public MetallicArtsMetalDef metal => def.GetModExtension<MetalsLinked>().Metals.First()!.ToMetallicArts();

    protected override Color BarColor => metal.color.SaturationChanged(1f);
    protected override Color BarHighlightColor => metal.color.SaturationChanged(2f);

    protected override void PostAddOrRemove() {
        MetalbornUtility.HandleMetalbornTrait(pawn);
    }
}