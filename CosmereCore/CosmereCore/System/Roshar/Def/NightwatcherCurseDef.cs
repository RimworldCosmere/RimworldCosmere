using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Def;

public class NightwatcherCurseDef : Verse.Def {
    public List<float> curseWeights = [1f, 1f, 1f];
    public int minBoonTier = 1;
    public HediffDef? hediff;
    public TraitDef? forceTrait;
    public int forceTraitDegree = 0;
    public TraitDef? stripTrait;
    public SkillDef? penaltySkill;
    public int penaltySkillLevels = 0;
    public int cultivationEvolutionDays = 0;
    public HediffDef? evolutionHediff;
    public TraitDef? evolutionGrantTrait;
    public Type? applicatorClass;

    public ICurseApplicator? Applicator =>
        applicatorClass != null
            ? (ICurseApplicator)Activator.CreateInstance(applicatorClass)
            : null;

    public override IEnumerable<string> ConfigErrors() {
        foreach (string err in base.ConfigErrors()) yield return err;
        if (applicatorClass != null && !typeof(ICurseApplicator).IsAssignableFrom(applicatorClass))
            yield return $"applicatorClass {applicatorClass} does not implement ICurseApplicator";
        if (curseWeights.Count != 3)
            yield return $"curseWeights must have exactly 3 entries (one per power tier), has {curseWeights.Count}";
    }
}

public interface ICurseApplicator {
    void Apply(Verse.Pawn pawn, NightwatcherCurseDef def);
}
