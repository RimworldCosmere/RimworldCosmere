using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Def;

public class NightwatcherBoonDef : Verse.Def {
    public int powerTier = 1;
    public HediffDef? hediff;
    public List<BoonSkillBoost> skillBoosts = [];
    public TraitDef? grantTrait;
    public int grantTraitDegree = 0;
    public TraitDef? removeTrait;
    public HediffDef? removeHediff;
    public float investitureBonus = 0f;
    public float surgebindingConnectionBoost = 0f;
    public bool psylinkBoost = false;
    public Type? applicatorClass;
    public string? requiresMod;
    public string? metalSelectionType;

    public IBoonApplicator? Applicator =>
        applicatorClass != null
            ? (IBoonApplicator)Activator.CreateInstance(applicatorClass)
            : null;

    public override IEnumerable<string> ConfigErrors() {
        foreach (string err in base.ConfigErrors()) yield return err;
        if (applicatorClass != null && !typeof(IBoonApplicator).IsAssignableFrom(applicatorClass))
            yield return $"applicatorClass {applicatorClass} does not implement IBoonApplicator";
    }
}

public class BoonSkillBoost : IExposable {
    public SkillDef skill = null!;
    public int levels = 0;

    public void ExposeData() {
        Scribe_Defs.Look(ref skill, "skill");
        Scribe_Values.Look(ref levels, "levels");
    }
}

public interface IBoonApplicator {
    void Apply(Verse.Pawn pawn, NightwatcherBoonDef def, Dictionary<string, object>? context = null);
}
