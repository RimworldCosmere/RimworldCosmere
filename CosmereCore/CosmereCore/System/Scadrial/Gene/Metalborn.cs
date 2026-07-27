using System;
using Cosmere.Core.DefModExtension;
using Cosmere.Core.Gene;
using Cosmere.System.Scadrial.Def;
using Cosmere.System.Scadrial.Util;
using UnityEngine;
using Verse;

namespace Cosmere.System.Scadrial.Gene;

public abstract class Metalborn : Invested {
    public MetallicArtsMetalDef metal =>
        def.GetModExtension<MetalsLinked>()?.Metals.FirstOrDefault()?.ToMetallicArts()
        ?? throw new InvalidOperationException($"Metalborn gene '{def.defName}' is missing MetalsLinked with a valid metal.");

    protected override Color BarColor => metal.color.SaturationChanged(1f);

    protected override Color BarHighlightColor => metal.color.SaturationChanged(2f);

    protected override void PostAddOrRemove() {
        MetalbornUtility.SyncMetalbornTrait(pawn);
    }
}
