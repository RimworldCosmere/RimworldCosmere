using Cosmere.Core.Gizmo;
using Cosmere.Scadrial.Allomancy.Ability;
using Cosmere.Scadrial.Def;
using Cosmere.Scadrial.Gene;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.Scadrial.Gizmo;

[StaticConstructorOnStartup]
public abstract class ScadrialGeneCommand<TGene>(
    Gene_Resource gene,
    List<IGeneResourceDrain> drainGenes,
    Color barColor,
    Color barHighlightColor
) : CosmereGeneCommand<AbilitySubGizmo, TGene>(gene, drainGenes, barColor, barHighlightColor) where TGene : Metalborn {
    protected NamedArgument coloredMetal;
    internal MetallicArtsMetalDef metal => gene.metal;
    protected override string Title => metal.LabelCap;

    protected virtual string GetTooltipDescription() {
        if (cachedTooltipDescription != null) return cachedTooltipDescription;

        if (!gene.def.resourceDescription.NullOrEmpty()) {
            return cachedTooltipDescription =
                "\n" + gene.def.resourceDescription.Formatted(coloredPawn, coloredMetal).Resolve();
        }

        return cachedTooltipDescription = "";
    }

    protected override IEnumerable<AbilitySubGizmo> GetSubGizmos() {
        return gene.def.abilities?
                   .OrderBy(x => x.uiOrder)
                   .Select(x => pawn.abilities.GetAbility(x))
                   .Cast<AbstractAllomancyAbility>()
                   .Where(x => x.GizmosVisible())
                   .Select(x => new AbilitySubGizmo(this, gene, x)) ??
               [];
    }

    protected override void Initialize() {
        if (initialized) return;

        coloredMetal = metal.coloredLabel.Named("METAL");

        base.Initialize();
    }
}