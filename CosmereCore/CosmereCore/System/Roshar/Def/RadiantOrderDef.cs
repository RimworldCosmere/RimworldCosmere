using System;
using Cosmere.Core.Def;
using RimWorld;
using UnityEngine;
using Verse;
using AbilityDef = RimWorld.AbilityDef;

namespace Cosmere.System.Roshar.Def;

public class Ideal {
    public List<AbilityDef> abilities = null!;
    public string description = null!;
    public string label = null!;
    public List<string> quotes = null!;
    public int stormlightMax = 0;
}

public class RadiantOrderDef : Verse.Def {
    public List<AbilityDef> abilities = null!;
    public Texture2D bannerIcon = null!;
    public Color color;

    public List<TraitRequirement> favorableTraits = null!;
    public GemDef gemstone = null!;
    public Texture2D icon = null!;
    public IIdealChecker idealChecker = null!;
    public List<Ideal> ideals = null!;
    public List<TraitRequirement> incompatibleTraits = null!;
    public Texture2D invertedIcon = null!;
    public string sprenDescription = string.Empty;
    public string sprenLabel = string.Empty;
    public List<string> sprenNamePool = [];
    public string sprenTexturePath = string.Empty;
    public List<SurgeDef> surges = null!;

    private Type idealCheckerClass =>
        typeof(RadiantOrderDef).Assembly.GetType("Cosmere.System.Roshar.Surgebinding.IdealChecker." + defName);

    public GeneDef GetSurgebindingGene() {
        return DefDatabase<GeneDef>.GetNamed("Cosmere_Roshar_Gene_Radiant" + defName);
    }

    public IEnumerable<AbilityDef> GetAbilities(int idealLevel = 4) {
        foreach (AbilityDef abilityDef in abilities) {
            yield return abilityDef;
        }

        foreach (AbilityDef surgeDefAbility in surges.SelectMany(surgeDef => surgeDef.abilities)) {
            int minIdeal = surgeDefAbility is SurgebindingAbilityDef surgeDef
                ? surgeDef.GetMinIdealForOrder(defName)
                : 0;
            if (idealLevel >= minIdeal) {
                yield return surgeDefAbility;
            }
        }

        for (int i = 0; i <= Math.Min(idealLevel, ideals.Count - 1); i++) {
            Ideal? ideal = ideals[i];
            foreach (AbilityDef idealAbility in ideal.abilities) {
                yield return idealAbility;
            }
        }
    }

    public override void PostLoad() {
        base.PostLoad();

        idealChecker = (IIdealChecker)Activator.CreateInstance(idealCheckerClass, this);
        LongEventHandler.ExecuteWhenFinished(() => {
            icon = ContentFinder<Texture2D>.Get($"UI/Surgebinding/Order/{defName}");
            bannerIcon = ContentFinder<Texture2D>.Get($"UI/Surgebinding/Order/Banner{defName}");
            if (icon != null) {
                invertedIcon = icon.CloneTexture().InvertColors();
            }

            AssignTraitCompatibility();
        }
        );
    }

    private static TraitRequirement Req(TraitDef traitDef, int? degree = null) {
        return new TraitRequirement { def = traitDef, degree = degree };
    }

    private void AssignTraitCompatibility() {
        TraitDef tough = DefDatabase<TraitDef>.GetNamed("Tough");
        TraitDef nerves = DefDatabase<TraitDef>.GetNamed("Nerves");
        TraitDef fastLearner = DefDatabase<TraitDef>.GetNamed("FastLearner");
        TraitDef naturalMood = DefDatabase<TraitDef>.GetNamed("NaturalMood");
        TraitDef speedOffset = DefDatabase<TraitDef>.GetNamed("SpeedOffset");

        switch (defName) {
            case "Windrunner":
                incompatibleTraits = [Req(RimWorld.TraitDefOf.Psychopath), Req(RimWorld.TraitDefOf.Bloodlust)];
                favorableTraits = [Req(RimWorld.TraitDefOf.Kind), Req(tough)];
                break;
            case "Skybreaker":
                incompatibleTraits = [Req(RimWorld.TraitDefOf.Pyromaniac), Req(RimWorld.TraitDefOf.Abrasive)];
                favorableTraits = [Req(nerves, 1), Req(nerves, 2)];
                break;
            case "Dustbringer":
                incompatibleTraits = [Req(nerves, -2), Req(RimWorld.TraitDefOf.Bloodlust)];
                favorableTraits = [Req(tough), Req(RimWorld.TraitDefOf.Industriousness, 2)];
                break;
            case "Edgedancer":
                incompatibleTraits = [Req(RimWorld.TraitDefOf.Psychopath), Req(RimWorld.TraitDefOf.Bloodlust)];
                favorableTraits = [Req(RimWorld.TraitDefOf.Kind), Req(naturalMood, 2)];
                break;
            case "Truthwatcher":
                incompatibleTraits = [Req(RimWorld.TraitDefOf.Psychopath), Req(RimWorld.TraitDefOf.Abrasive)];
                favorableTraits = [Req(RimWorld.TraitDefOf.Kind), Req(nerves, 1)];
                break;
            case "Lightweaver":
                incompatibleTraits = [Req(RimWorld.TraitDefOf.Psychopath), Req(RimWorld.TraitDefOf.Bloodlust)];
                favorableTraits = [Req(fastLearner), Req(naturalMood, -1)];
                break;
            case "Elsecaller":
                incompatibleTraits = [Req(RimWorld.TraitDefOf.Industriousness, -1), Req(nerves, -2)];
                favorableTraits = [Req(RimWorld.TraitDefOf.Industriousness, 2), Req(fastLearner)];
                break;
            case "Willshaper":
                incompatibleTraits = [Req(RimWorld.TraitDefOf.Greedy), Req(RimWorld.TraitDefOf.Abrasive)];
                favorableTraits = [Req(RimWorld.TraitDefOf.Kind), Req(speedOffset, 2)];
                break;
            case "Stoneward":
                incompatibleTraits = [Req(RimWorld.TraitDefOf.Wimp), Req(RimWorld.TraitDefOf.Industriousness, -1)];
                favorableTraits = [Req(tough), Req(nerves, 1), Req(RimWorld.TraitDefOf.Industriousness, 2)];
                break;
            case "Bondsmith":
                incompatibleTraits = [
                    Req(RimWorld.TraitDefOf.Psychopath), Req(RimWorld.TraitDefOf.DislikesMen),
                    Req(RimWorld.TraitDefOf.DislikesWomen),
                ];
                favorableTraits = [Req(RimWorld.TraitDefOf.Kind), Req(naturalMood, 2)];
                break;
        }
    }
}
