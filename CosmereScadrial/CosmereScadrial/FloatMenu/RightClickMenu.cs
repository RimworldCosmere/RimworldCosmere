using System.Collections.Generic;
using System.Linq;
using CosmereScadrial.Abilities.Allomancy;
using CosmereScadrial.Comps.Things;
using CosmereScadrial.Defs;
using FloatSubMenus;
using RimWorld;
using Verse;

namespace CosmereScadrial.FloatMenu;

public class RightClickMenu : FloatMenuOptionProvider {
    protected override bool Drafted => true;
    protected override bool Undrafted => true;
    protected override bool Multiselect => false;
    protected override bool RequiresManipulation => true;
    protected override bool CanSelfTarget => true;

    public override IEnumerable<FloatMenuOption> GetOptions(FloatMenuContext context) {
        return [];
        Pawn? pawn = context.FirstSelectedPawn;
        List<FloatMenuOption> options = new List<FloatMenuOption>();
        MetalBurning? metalBurning = pawn.GetComp<MetalBurning>();
        TargetingParameters? targetParams = TargetingParameters.ForAttackAny();
        targetParams.canTargetSelf = true;
        targetParams.canTargetLocations = true;
        targetParams.canTargetItems = true;
        targetParams.canTargetPlants = true;
        targetParams.mapObjectTargetsMustBeAutoAttackable = false;
        targetParams.mustBeSelectable = true;

        List<FloatMenuOption> subOptions = new List<FloatMenuOption>();
        FloatSubMenu allomancyMenu = new FloatSubMenu("Allomancy", subOptions);
        LocalTargetInfo[] targets = GenUI.TargetsAt(context.clickPosition, targetParams, true).ToArray();

        Dictionary<LocalTargetInfo, List<FloatMenuOption>> targetMenus =
            new Dictionary<LocalTargetInfo, List<FloatMenuOption>>();
        foreach (LocalTargetInfo target in targets) {
            List<FloatMenuOption>? subTargetOptions = GetSubTargetOptions(pawn, metalBurning, target);
            if (subTargetOptions == null) continue;
            targetMenus[target] = subTargetOptions;
        }

        if (targetMenus.Count == 1) {
            subOptions.AddRange(targetMenus.First().Value);
        } else if (targetMenus.Count > 1) {
            foreach ((LocalTargetInfo key, List<FloatMenuOption>? value) in targetMenus) {
                subOptions.Add(new FloatSubMenu($"Target: {key.Thing.LabelCap}", value));
            }
        } else {
            return [];
        }

        options.Add(allomancyMenu);

        return options;
    }

    private static List<FloatMenuOption> GetSubTargetOptions(Pawn pawn, MetalBurning metalBurning,
        LocalTargetInfo target) {
        if (target == null || !target.IsValid) return null;

        AbstractAbility?[] allomanticAbilities = pawn.abilities.AllAbilitiesForReading
            .Where(x => x.def is AllomanticAbilityDef && x.CanApplyOn(target)).Select(x => x as AbstractAbility)
            .ToArray();
        if (allomanticAbilities.Length == 0) return null;
        IEnumerable<IGrouping<MetallicArtsMetalDef, AbstractAbility?>> metals =
            allomanticAbilities.GroupBy(x => x.metal);

        List<FloatMenuOption> options = new List<FloatMenuOption> {
            new FloatMenuSearch(true),
        };
        if (target.Pawn != null && pawn.Equals(target.Pawn)) {
            options.Add(new FloatMenuOption("Stop Burning All Metals",
                metalBurning.IsBurning() ? () => metalBurning.RemoveAllBurnSources() : null));
        }

        foreach (IGrouping<MetallicArtsMetalDef, AbstractAbility?>? metal in metals) {
            if (metal.Count() == 1) {
                options.Add(new AllomanticAbilityFloatMenuOption(metal.First(), target));
            } else {
                FloatSubMenu metalMenu = new FloatSubMenu(metal.Key.LabelCap, new List<FloatMenuOption>());
                foreach (AbstractAbility? ability in metal) {
                    metalMenu.Options.Add(new AllomanticAbilityFloatMenuOption(ability, target));
                }

                options.Add(metalMenu);
            }
        }

        return options;
    }
}