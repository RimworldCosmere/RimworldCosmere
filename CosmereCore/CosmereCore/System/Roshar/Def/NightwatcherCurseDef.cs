using System;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Def;

public class NightwatcherCurseDef : Verse.Def {
    public Type? applicatorClass;
    public int cultivationEvolutionDays = 0;
    public List<float> curseWeights = [1f, 1f, 1f];
    public TraitDef? evolutionGrantTrait;
    public HediffDef? evolutionHediff;
    public TraitDef? forceTrait;
    public int forceTraitDegree = 0;
    public HediffDef? hediff;
    public int minBoonTier = 1;
    public SkillDef? penaltySkill;
    public int penaltySkillLevels = 0;
    public TraitDef? stripTrait;

    public ICurseApplicator? Applicator =>
        applicatorClass != null
            ? (ICurseApplicator)Activator.CreateInstance(applicatorClass)
            : null;

    public override IEnumerable<string> ConfigErrors() {
        foreach (string err in base.ConfigErrors()) yield return err;
        if (applicatorClass != null && !typeof(ICurseApplicator).IsAssignableFrom(applicatorClass)) {
            yield return $"applicatorClass {applicatorClass} does not implement ICurseApplicator";
        }

        if (curseWeights.Count != 3) {
            yield return $"curseWeights must have exactly 3 entries (one per power tier), has {curseWeights.Count}";
        }
    }
}

public interface ICurseApplicator {
    void Apply(Pawn pawn, NightwatcherCurseDef def);
}