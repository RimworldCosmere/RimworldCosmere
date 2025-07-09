using UnityEngine;
using Verse;

namespace CosmereFramework.Comp.Map;

public record struct CircleToRender {
    public readonly Verse.Thing from;
    public readonly Material lineMaterial;
    public readonly float radius;

    public CircleToRender(Verse.Thing from, float radius, Material lineMaterial, float alpha = 1f) {
        this.from = from;
        this.from = from;
        this.radius = radius;

        this.lineMaterial = FadedMaterialPool.FadedVersionOf(lineMaterial, alpha);
        this.lineMaterial.color = lineMaterial.color.WithAlpha(this.lineMaterial.color.a);
    }
}

public class CircleRenderer(Verse.Map map) : Renderer<CircleToRender>(map) {
    protected override void RenderItem(CircleToRender circle) {
        GenDraw.DrawCircleOutline(
            circle.from.DrawPos,
            circle.radius,
            circle.lineMaterial
        );
    }
}