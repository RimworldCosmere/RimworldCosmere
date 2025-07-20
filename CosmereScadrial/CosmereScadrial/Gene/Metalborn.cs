using System.Linq;
using Cosmere.Core.Gene;
using Cosmere.Resources.DefModExtension;
using Cosmere.Scadrial.Def;
using Cosmere.Scadrial.Extension;
using Cosmere.Scadrial.Util;
using UnityEngine;
using Verse;

namespace Cosmere.Scadrial.Gene;

public abstract class Metalborn : Invested {
    public MetallicArtsMetalDef metal => def.GetModExtension<MetalsLinked>().Metals.First()!.ToMetallicArts();

    protected override Color BarColor => metal.color.SaturationChanged(1f);
    protected override Color BarHighlightColor => metal.color.SaturationChanged(2f);

    protected override void PostAddOrRemove() {
        MetalbornUtility.HandleMetalbornTrait(pawn);
    }
}