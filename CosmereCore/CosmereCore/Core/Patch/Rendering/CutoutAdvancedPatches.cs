using System;
using Concord;
using Cosmere.Core.Comp.Thing;
using Cosmere.Core.Shader.Properties;
using RimWorld;
using UnityEngine;
using Verse;
using ShaderDatabase = Cosmere.Core.Shader.ShaderDatabase;

namespace Cosmere.Core.Patch.Rendering;

[StaticConstructorOnStartup]
public static class CutoutAdvancedMaterial {
    private static Material? DrawNowMaterial;

    public static void ApplyMPBToMaterial(Material mat, MaterialPropertyBlock mpb) {
        Texture tex = mpb.GetTexture(CutoutAdvancedShaderProperties.MainTex);
        if (tex != null) mat.SetTexture(CutoutAdvancedShaderProperties.MainTex, tex);

        tex = mpb.GetTexture(CutoutAdvancedShaderProperties.ColorMaskTex);
        if (tex != null) mat.SetTexture(CutoutAdvancedShaderProperties.ColorMaskTex, tex);

        mat.SetFloat(
            CutoutAdvancedShaderProperties.BlendStrength,
            mpb.GetFloat(CutoutAdvancedShaderProperties.BlendStrength)
        );
        mat.SetFloat(
            CutoutAdvancedShaderProperties.ColorCount,
            mpb.GetFloat(CutoutAdvancedShaderProperties.ColorCount)
        );
        mat.SetFloat(
            CutoutAdvancedShaderProperties.MaterialIntensity,
            mpb.GetFloat(CutoutAdvancedShaderProperties.MaterialIntensity)
        );

        Vector4[] colors = mpb.GetVectorArray(CutoutAdvancedShaderProperties.Colors);
        if (colors is { Length: > 0 }) mat.SetVectorArray(CutoutAdvancedShaderProperties.Colors, colors);

        float[] metallicValues = mpb.GetFloatArray(CutoutAdvancedShaderProperties.MetallicValues);
        if (metallicValues is { Length: > 0 })
            mat.SetFloatArray(CutoutAdvancedShaderProperties.MetallicValues, metallicValues);

        float[] smoothnessValues = mpb.GetFloatArray(CutoutAdvancedShaderProperties.SmoothnessValues);
        if (smoothnessValues is { Length: > 0 })
            mat.SetFloatArray(CutoutAdvancedShaderProperties.SmoothnessValues, smoothnessValues);

        mat.SetFloat(
            CutoutAdvancedShaderProperties.UseWearMask,
            mpb.GetFloat(CutoutAdvancedShaderProperties.UseWearMask)
        );
        mat.SetFloat(
            CutoutAdvancedShaderProperties.UseGlowMask,
            mpb.GetFloat(CutoutAdvancedShaderProperties.UseGlowMask)
        );
        mat.SetFloat(
            CutoutAdvancedShaderProperties.UseSpecialMask,
            mpb.GetFloat(CutoutAdvancedShaderProperties.UseSpecialMask)
        );

        tex = mpb.GetTexture(CutoutAdvancedShaderProperties.WearMaskTex);
        if (tex != null) mat.SetTexture(CutoutAdvancedShaderProperties.WearMaskTex, tex);
        mat.SetFloat(
            CutoutAdvancedShaderProperties.WearDarkness,
            mpb.GetFloat(CutoutAdvancedShaderProperties.WearDarkness)
        );
        mat.SetFloat(
            CutoutAdvancedShaderProperties.CurrentWearLevel,
            mpb.GetFloat(CutoutAdvancedShaderProperties.CurrentWearLevel)
        );
        mat.SetFloat(
            CutoutAdvancedShaderProperties.WearLevelCount,
            mpb.GetFloat(CutoutAdvancedShaderProperties.WearLevelCount)
        );

        tex = mpb.GetTexture(CutoutAdvancedShaderProperties.GlowMaskTex);
        if (tex != null) mat.SetTexture(CutoutAdvancedShaderProperties.GlowMaskTex, tex);
        mat.SetColor(CutoutAdvancedShaderProperties.GlowColor, mpb.GetColor(CutoutAdvancedShaderProperties.GlowColor));
        mat.SetFloat(
            CutoutAdvancedShaderProperties.GlowIntensity,
            mpb.GetFloat(CutoutAdvancedShaderProperties.GlowIntensity)
        );
        mat.SetFloat(
            CutoutAdvancedShaderProperties.CurrentGlowLevel,
            mpb.GetFloat(CutoutAdvancedShaderProperties.CurrentGlowLevel)
        );
        mat.SetFloat(
            CutoutAdvancedShaderProperties.GlowLevelCount,
            mpb.GetFloat(CutoutAdvancedShaderProperties.GlowLevelCount)
        );

        tex = mpb.GetTexture(CutoutAdvancedShaderProperties.SpecialMaskTex);
        if (tex != null) mat.SetTexture(CutoutAdvancedShaderProperties.SpecialMaskTex, tex);

        mat.SetVector(
            CutoutAdvancedShaderProperties.HighlightParams,
            mpb.GetVector(CutoutAdvancedShaderProperties.HighlightParams)
        );
        mat.SetVector(
            CutoutAdvancedShaderProperties.RimLightCenter,
            mpb.GetVector(CutoutAdvancedShaderProperties.RimLightCenter)
        );
    }

    public static Material ForDrawNow(Material source, MaterialPropertyBlock? properties) {
        if (DrawNowMaterial == null) {
            DrawNowMaterial = new Material(source);
        } else {
            DrawNowMaterial.CopyPropertiesFromMaterial(source);
        }

        if (properties != null) {
            ApplyMPBToMaterial(DrawNowMaterial, properties);
        }

        return DrawNowMaterial;
    }
}

[Patch]
public abstract class DynamicApparelNodesPatch : DynamicPawnRenderNodeSetup_Apparel {
    [Inject(At.Return, nameof(GetDynamicNodes))]
    private void AfterGetDynamicNodes(ControlHandle<IEnumerable<(PawnRenderNode node, PawnRenderNode parent)>> ch) {
        ch.ReturnValue = WithCutoutAdvancedSubworker(ch.ReturnValue);
    }

    private static IEnumerable<(PawnRenderNode node, PawnRenderNode parent)> WithCutoutAdvancedSubworker(
        IEnumerable<(PawnRenderNode node, PawnRenderNode parent)> nodes
    ) {
        foreach ((PawnRenderNode node, PawnRenderNode parent) entry in nodes) {
            if (entry.node?.apparel?.TryGetComp(out CutoutAdvanced _) ?? false) {
                entry.node.Props.subworkerClasses ??= [];
                entry.node.Props.subworkerClasses.Add(typeof(Gene.PawnRender.SubWorker.CutoutAdvanced));
            }

            yield return entry;
        }
    }
}

[Patch(typeof(GenDraw))]
public static class GenDrawMeshNowOrLaterPatch {
    [Inject(
        At.Head,
        nameof(GenDraw.DrawMeshNowOrLater),
        parameterTypes: [
            typeof(Mesh),
            typeof(Matrix4x4),
            typeof(Material),
            typeof(bool),
            typeof(MaterialPropertyBlock),
        ]
    )]
    private static Control BeforeDrawMeshNowOrLater(
        Mesh mesh,
        Matrix4x4 matrix,
        Material mat,
        bool drawNow,
        MaterialPropertyBlock? properties
    ) {
        if (mat.shader != ShaderDatabase.CutoutAdvanced) return Control.Continue;

        if (drawNow) {
            Material material = CutoutAdvancedMaterial.ForDrawNow(mat, properties);
            material.SetPass(0);
            Graphics.DrawMeshNow(mesh, matrix);
        } else {
            Graphics.DrawMesh(mesh, matrix, mat, 0, null, 0, properties);
        }

        return Control.Cancel;
    }
}

[Patch(typeof(PawnRenderUtility))]
public static class PawnRenderUtilityDrawEquipmentAimingPatch {
    [Inject(At.Head, nameof(PawnRenderUtility.DrawEquipmentAiming))]
    private static Control BeforeDrawEquipmentAiming(Verse.Thing eq, Vector3 drawLoc, float aimAngle) {
        if (!eq.TryGetComp(out CutoutAdvanced comp)) return Control.Continue;

        float num = aimAngle - 90f;
        Mesh mesh;
        switch (aimAngle) {
            case > 200f and < 340f:
                mesh = MeshPool.plane10Flip;
                num -= 180f;
                num -= eq.def.equippedAngleOffset;
                break;
            default:
                mesh = MeshPool.plane10;
                num += eq.def.equippedAngleOffset;
                break;
        }

        num %= 360f;
        CompEquippable compEquippable = eq.TryGetComp<CompEquippable>();
        if (compEquippable != null) {
            EquipmentUtility.Recoil(
                eq.def,
                EquipmentUtility.GetRecoilVerb(compEquippable.AllVerbs),
                out Vector3 drawOffset,
                out float angleOffset,
                aimAngle
            );
            drawLoc += drawOffset;
            num += angleOffset;
        }

        Material material = eq.Graphic is not Graphic_StackCount graphicStackCount
            ? eq.Graphic.MatSingleFor(eq)
            : graphicStackCount.SubGraphicForStackCount(1, eq.def).MatSingleFor(eq);
        Matrix4x4 matrix = Matrix4x4.TRS(
            s: new Vector3(eq.Graphic.drawSize.x, 0f, eq.Graphic.drawSize.y),
            pos: drawLoc,
            q: Quaternion.AngleAxis(num, Vector3.up)
        );

        MaterialPropertyBlock mpb = comp.UpdateMaterialPropertyBlock(CutoutAdvanced.MPB, eq.Graphic, material);
        Graphics.DrawMesh(mesh, matrix, material, 0, null, 0, mpb);

        return Control.Cancel;
    }
}

[Patch]
public abstract class GraphicDrawMeshIntPatch : Verse.Graphic {
    // DrawMeshInt never receives the Thing being drawn, so the enclosing DrawWorker call
    // parks it here for the duration of that call.
    [ThreadStatic]
    private static Verse.Thing? drawingThing;

    [Inject(At.Around, nameof(DrawWorker))]
    private void AroundDrawWorker(
        Vector3 loc,
        Rot4 rot,
        ThingDef thingDef,
        Verse.Thing thing,
        float extraRotation,
        VoidOperation<Vector3, Rot4, ThingDef, Verse.Thing, float> original
    ) {
        Verse.Thing? previous = drawingThing;
        drawingThing = thing;
        try {
            original.Invoke(loc, rot, thingDef, thing, extraRotation);
        } finally {
            drawingThing = previous;
        }
    }

    [Inject(At.Head, nameof(DrawMeshInt))]
    private Control BeforeDrawMeshInt(Mesh mesh, Vector3 loc, Quaternion quat, Material mat) {
        Verse.Thing? thing = drawingThing;
        if (thing == null) return Control.Continue;
        if (!thing.TryGetComp(out CutoutAdvanced comp)) return Control.Continue;

        MaterialPropertyBlock mpb = comp.UpdateMaterialPropertyBlock(CutoutAdvanced.MPB, thing.Graphic, mat);
        Graphics.DrawMesh(mesh, loc, quat, mat, 0, null, 0, mpb);
        return Control.Cancel;
    }
}

[Patch]
public abstract class GraphicRandomRotatedDrawWorkerPatch : Graphic_RandomRotated {
    [InjectField("maxAngle")]
    private readonly float maxAngle;

    protected GraphicRandomRotatedDrawWorkerPatch(Verse.Graphic subGraphic, float maxAngle)
        : base(subGraphic, maxAngle) { }

    [InjectMethod("GetRotInRack")]
    protected abstract float? GetRotInRack(Verse.Thing thing, ThingDef thingDef, IntVec3 loc);

    [Inject(At.Head, nameof(DrawWorker))]
    private Control BeforeDrawWorker(
        Vector3 loc,
        Rot4 rot,
        ThingDef thingDef,
        Verse.Thing? thing,
        float extraRotation
    ) {
        if (thing == null || !thing.TryGetComp(out CutoutAdvanced comp)) return Control.Continue;

        Mesh mesh = MeshAt(rot);

        float? rotInRack = GetRotInRack(thing, thingDef, loc.ToIntVec3());

        float num = rotInRack ?? (float)(-(double)maxAngle + thing.thingIDNumber * 542 % (maxAngle * 2.0));

        float angle = num + extraRotation;
        Vector3 position = loc;
        Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.up);
        Material material = MatSingleFor(thing);
        MaterialPropertyBlock mpb = comp.UpdateMaterialPropertyBlock(
            CutoutAdvanced.MPB,
            SubGraphic,
            material
        );
        Graphics.DrawMesh(mesh, position, rotation, material, 0, null, 0, mpb);
        return Control.Cancel;
    }
}
