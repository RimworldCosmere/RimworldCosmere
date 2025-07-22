using Cosmere.Core.Gizmo;
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
) : CosmereGeneCommand<AllomanticAbilitySubGizmo, TGene>(gene, drainGenes, barColor, barHighlightColor)
    where TGene : Metalborn {
    protected NamedArgument coloredMetal;
    internal MetallicArtsMetalDef metal => gene.metal;
    protected override string Title => metal.LabelCap;

    protected override string GetTooltipDescription() {
        if (cachedTooltipDescription != null) return cachedTooltipDescription;

        if (!gene.def.resourceDescription.NullOrEmpty()) {
            return cachedTooltipDescription =
                "\n" + gene.def.resourceDescription.Formatted(coloredPawn, coloredMetal).Resolve();
        }

        return cachedTooltipDescription = "";
    }

    protected override void Initialize() {
        if (initialized) return;

        coloredMetal = metal.coloredLabel.Named("METAL");

        base.Initialize();
    }
}