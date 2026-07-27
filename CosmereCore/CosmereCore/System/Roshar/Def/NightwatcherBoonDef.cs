using System;
using Cosmere.Core.Nightwatcher;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Def;

public class NightwatcherBoonDef : Verse.Def {
    public Type? applicatorClass;
    public TraitDef? grantTrait;
    public int grantTraitDegree = 0;
    public HediffDef? hediff;
    public float investitureBonus = 0f;
    public string? metalSelectionType;
    public int powerTier = 1;
    public bool psylinkBoost = false;
    public HediffDef? removeHediff;
    public TraitDef? removeTrait;
    public string? requiresMod;
    public List<BoonSkillBoost> skillBoosts = [];
    public float surgebindingConnectionBoost = 0f;

    private IBoonApplicator? applicatorCache;

    public IBoonApplicator? Applicator {
        get {
            if (applicatorClass == null) return null;
            return applicatorCache ??= (IBoonApplicator)Activator.CreateInstance(applicatorClass);
        }
    }

    public override IEnumerable<string> ConfigErrors() {
        foreach (string err in base.ConfigErrors()) yield return err;
        if (applicatorClass != null && !typeof(IBoonApplicator).IsAssignableFrom(applicatorClass)) {
            yield return $"applicatorClass {applicatorClass} does not implement IBoonApplicator";
        }
    }
}

public class BoonSkillBoost : IExposable {
    public int levels;
    public SkillDef skill = null!;

    public void ExposeData() {
        Scribe_Defs.Look(ref skill, "skill");
        Scribe_Values.Look(ref levels, "levels");
    }
}
