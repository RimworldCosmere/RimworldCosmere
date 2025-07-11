using UnityEngine;
using Verse;

namespace CosmereFramework.Comp.Map;

public record struct LineToRender {
    public readonly Verse.Thing from;
    public readonly Material lineMaterial;
    public readonly float thickness;
    public readonly Verse.Thing to;

    public LineToRender(Verse.Thing from, Verse.Thing to, Material lineMaterial, float? alpha, float? thickness) {
        this.from = from;
        this.to = to;
        this.thickness = thickness ?? 1f;

        this.lineMaterial = FadedMaterialPool.FadedVersionOf(lineMaterial, alpha ?? 1f);
        this.lineMaterial.color = lineMaterial.color.WithAlpha(this.lineMaterial.color.a);
    }
}

public class LineRenderer(Verse.Map map) : Renderer<LineToRender>(map) {
    protected override void RenderItem(LineToRender line) {
        GenDraw.DrawLineBetween(
            line.from.DrawPos,
            line.to.DrawPos,
            line.lineMaterial,
            line.thickness
        );
    }
}