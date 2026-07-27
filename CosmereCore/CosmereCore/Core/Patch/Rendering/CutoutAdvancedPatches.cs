using System.Reflection;
using Cosmere.Core.Comp.Thing;
using Cosmere.Core.Shader.Properties;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using ShaderDatabase = Cosmere.Core.Shader.ShaderDatabase;

namespace Cosmere.Core.Patch.Rendering;

[StaticConstructorOnStartup]
[HarmonyPatch]
public static class CutoutAdvancedPatch {
    private static Material? DrawNowMaterial;

    private static MethodInfo GraphicsRandomRotatedGetRotInRack =>
        AccessTools.Method(typeof(Graphic_RandomRotated), "GetRotInRack");

    private static void ApplyMPBToMaterial(Material mat, MaterialPropertyBlock mpb) {
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

    [HarmonyPatch(
        typeof(DynamicPawnRenderNodeSetup_Apparel),
        nameof(DynamicPawnRenderNodeSetup_Apparel.GetDynamicNodes)
    )]
    [HarmonyPostfix]
    public static IEnumerable<(PawnRenderNode node, PawnRenderNode parent)>
        DynamicPawnRenderNodeSetup_ApparelGetDynamicNodesPostfix(
            this IEnumerable<(PawnRenderNode node, PawnRenderNode parent)> nodes
        ) {
        foreach ((PawnRenderNode node, PawnRenderNode parent) entry in nodes) {
            if (entry.node?.apparel?.TryGetComp(out CutoutAdvanced _) ?? false) {
                entry.node.Props.subworkerClasses ??= [];
                entry.node.Props.subworkerClasses.Add(typeof(Gene.PawnRender.SubWorker.CutoutAdvanced));
            }

            yield return entry;
        }
    }

    [HarmonyPatch(
        typeof(GenDraw),
        nameof(GenDraw.DrawMeshNowOrLater),
        typeof(Mesh),
        typeof(Matrix4x4),
        typeof(Material),
        typeof(bool),
        typeof(MaterialPropertyBlock)
    )]
    [HarmonyPrefix]
    public static bool GenDrawDrawMeshNowOrLaterPrefix(
        Mesh mesh,
        Matrix4x4 matrix,
        Material mat,
        bool drawNow,
        MaterialPropertyBlock? properties = null
    ) {
        if (mat.shader != ShaderDatabase.CutoutAdvanced) return true;

        if (drawNow) {
            if (DrawNowMaterial == null) {
                DrawNowMaterial = new Material(mat);
            } else {
                DrawNowMaterial.CopyPropertiesFromMaterial(mat);
            }

            if (properties != null) {
                ApplyMPBToMaterial(DrawNowMaterial, properties);
            }

            DrawNowMaterial.SetPass(0);
            Graphics.DrawMeshNow(mesh, matrix);
        } else {
            Graphics.DrawMesh(mesh, matrix, mat, 0, null, 0, properties);
        }

        return false;
    }

    [HarmonyPatch(typeof(PawnRenderUtility), nameof(PawnRenderUtility.DrawEquipmentAiming))]
    [HarmonyPrefix]
    public static bool PawnRenderUtilityDrawEquipmentAimingPrefix(Verse.Thing eq, Vector3 drawLoc, float aimAngle) {
        if (!eq.TryGetComp(out CutoutAdvanced comp)) return true;

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

        return false;
    }

    [HarmonyPatch(typeof(Verse.Graphic), nameof(Verse.Graphic.DrawWorker))]
    [HarmonyPrefix]
    public static void GraphicDrawWorkerPrefix(out Verse.Thing __state, Verse.Thing thing) {
        __state = thing;
    }

    [HarmonyPatch(typeof(Verse.Graphic), "DrawMeshInt")]
    [HarmonyPrefix]
    public static bool GraphicDrawMeshInt(
        Verse.Thing __state,
        Mesh mesh,
        Vector3 loc,
        Quaternion quat,
        Material mat
    ) {
        if (!__state.TryGetComp(out CutoutAdvanced comp)) return true;

        MaterialPropertyBlock mpb = comp.UpdateMaterialPropertyBlock(CutoutAdvanced.MPB, __state.Graphic, mat);
        Graphics.DrawMesh(mesh, loc, quat, mat, 0, null, 0, mpb);
        return false;
    }

    [HarmonyPatch(typeof(Graphic_RandomRotated), nameof(Verse.Graphic.DrawWorker))]
    [HarmonyPrefix]
    public static bool GraphicRandomRotatedDrawWorkerPrefix(
        Graphic_RandomRotated __instance,
        float ___maxAngle,
        Vector3 loc,
        Rot4 rot,
        ThingDef thingDef,
        Verse.Thing? thing,
        float extraRotation
    ) {
        if (thing == null || !thing.TryGetComp(out CutoutAdvanced comp)) return true;

        Mesh mesh = __instance.MeshAt(rot);

        float? rotInRack =
            (float?)GraphicsRandomRotatedGetRotInRack.Invoke(__instance, [thing, thingDef, loc.ToIntVec3()]);

        float num = rotInRack ?? (float)(-(double)___maxAngle + thing.thingIDNumber * 542 % (___maxAngle * 2.0));

        float angle = num + extraRotation;
        Vector3 position = loc;
        Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.up);
        Material material = __instance.MatSingleFor(thing);
        MaterialPropertyBlock mpb = comp.UpdateMaterialPropertyBlock(
            CutoutAdvanced.MPB,
            __instance.SubGraphic,
            material
        );
        Graphics.DrawMesh(mesh, position, rotation, material, 0, null, 0, mpb);
        return false;
    }
}
