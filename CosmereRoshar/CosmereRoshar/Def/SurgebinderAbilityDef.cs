using System.Collections.Generic;
using Cosmere.Core.Def;
using Cosmere.Resources.Def;

namespace Cosmere.Roshar.Def;

public class SurgebinderAbilityDef : AbilityDef {
    public GemDef gem = null!;

    public override IEnumerable<string> ConfigErrors() {
        foreach (string? error in base.ConfigErrors()) yield return error;

        if (gem is null) yield return "gem is null";
    }
}