using System;
using Cosmere.Core.Def;
using Cosmere.Pickle.Lookup;
using RimWorks.Pickle;
using Verse;

namespace Cosmere.Pickle.Steps;

/// <summary>Proves the harness: step discovery, the step table and the assertion path.</summary>
[PickleSteps]
public class SmokeSteps {
    /// <summary>Asserts a Cosmere metal def loaded.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="defName">The metal def expected.</param>
    [Then("the Cosmere metal {string} is loaded")]
    public void AssertMetalLoaded(PickleContext ctx, string defName) {
        MetalDef? metal = DefDatabase<MetalDef>.GetNamedSilentFail(defName);

        CosmereLookup.AssertThat(
            ctx,
            metal != null,
            $"the Cosmere metal '{defName}' should be loaded",
            () => $"loaded metals: {CosmereLookup.DescribeAll<MetalDef>()}");
    }
}
