using Cosmere;
using System;
using Cosmere.Def;
using Cosmere.System.Roshar.Surgebinding.IdealChecker;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.System.Roshar.Def;

public class Ideal {
    public List<AbilityDef> abilities;
    public string label;
    public List<string> quotes;
}

public class RadiantOrderDef : Verse.Def {
    public List<AbilityDef> abilities;
    public Texture2D bannerIcon;
    public Color color;
    public GemDef gemstone;
    public Texture2D icon;

    public AbstractIdealChecker idealChecker;
    public List<Ideal> ideals;
    public Texture2D invertedIcon;
    public List<SurgeDef> surges;

    private Type idealCheckerClass =>
        typeof(RadiantOrderDef).Assembly.GetType("Cosmere.Roshar.Surgebinding.IdealChecker." + defName);

    public GeneDef GetSurgebindingGene() {
        return DefDatabase<GeneDef>.GetNamed("Cosmere_Roshar_Gene_Radiant" + defName);
    }

    public IEnumerable<AbilityDef> GetAbilities(int idealLevel = 4) {
        foreach (AbilityDef abilityDef in abilities) {
            yield return abilityDef;
        }

        foreach (AbilityDef surgeDefAbility in surges.SelectMany(surgeDef => surgeDef.abilities
                 )) {
            yield return surgeDefAbility;
        }

        for (int i = 0; i < Math.Min(idealLevel, ideals.Count); i++) {
            Ideal? ideal = ideals[i];
            foreach (AbilityDef idealAbility in ideal.abilities) {
                yield return idealAbility;
            }
        }
    }

    public override void PostLoad() {
        base.PostLoad();

        idealChecker = (AbstractIdealChecker)Activator.CreateInstance(idealCheckerClass, this);
        LongEventHandler.ExecuteWhenFinished(() => {
                icon = ContentFinder<Texture2D>.Get($"UI/Surgebinding/Order/{defName}");
                bannerIcon = ContentFinder<Texture2D>.Get($"UI/Surgebinding/Order/Banner{defName}");
                if (icon != null) {
                    invertedIcon = icon.CloneTexture().InvertColors();
                }
            }
        );
    }
}