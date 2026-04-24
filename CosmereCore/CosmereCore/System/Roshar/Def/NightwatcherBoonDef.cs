using System;
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

    public IBoonApplicator? Applicator =>
        applicatorClass != null
            ? (IBoonApplicator)Activator.CreateInstance(applicatorClass)
            : null;

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

public interface IBoonApplicator {
    void Apply(Pawn pawn, NightwatcherBoonDef def, Dictionary<string, object>? context = null);
}