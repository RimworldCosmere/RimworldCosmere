using System.Reflection;
using Cosmere.Core.Comp.Thing;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.Core.Patch;

[HarmonyPatch]
public static class CutoutLUTPatch {
    #region PawnRenderUtility patches

    [HarmonyPatch(typeof(PawnRenderNodeWorker), nameof(PawnRenderNodeWorker.GetMaterialPropertyBlock))]
    [HarmonyPostfix]
    public static MaterialPropertyBlock PawnRenderNodeWorkerGetMaterialPropertyBlockPrefix(
        MaterialPropertyBlock block,
        PawnRenderNode node,
        Material material,
        PawnDrawParms parms
    ) {
        if (node.apparel == null || !node.apparel.TryGetComp(out CutoutLUT comp)) return block;

        node.PrimaryGraphic.MatSingle.shader = ShaderDatabase.CutoutLUT;
        material.shader = ShaderDatabase.CutoutLUT;

        return comp.UpdateMaterialPropertyBlock(block, node.PrimaryGraphic, material);
    }

    [HarmonyPatch(typeof(PawnRenderUtility), nameof(PawnRenderUtility.DrawEquipmentAiming))]
    [HarmonyPrefix]
    public static bool PawnRenderUtilityDrawEquipmentAimingPrefix(Verse.Thing eq, Vector3 drawLoc, float aimAngle) {
        if (!eq.TryGetComp(out CutoutLUT comp)) return true;

        float num = aimAngle - 90f;
        Mesh mesh;
        switch (aimAngle) {
            case > 20f and < 160f:
                mesh = MeshPool.plane10;
                num += eq.def.equippedAngleOffset;
                break;
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

        MaterialPropertyBlock mpb = comp.UpdateMaterialPropertyBlock(CutoutLUT.MPB, eq.Graphic, material);
        Graphics.DrawMesh(mesh, matrix, material, 0, null, 0, mpb);

        return false;
    }

    #endregion

    #region Graphic patches

    [HarmonyPatch(typeof(Graphic), nameof(Graphic.DrawWorker))]
    [HarmonyPrefix]
    public static void GraphicDrawWorkerPrefix(out Verse.Thing __state, Verse.Thing thing) {
        __state = thing;
    }

    [HarmonyPatch(typeof(Graphic), "DrawMeshInt")]
    [HarmonyPrefix]
    public static bool GraphicDrawMeshInt(
        Verse.Thing __state,
        Mesh mesh,
        Vector3 loc,
        Quaternion quat,
        Material mat
    ) {
        if (!__state.TryGetComp(out CutoutLUT comp)) return true;

        MaterialPropertyBlock mpb = comp.UpdateMaterialPropertyBlock(CutoutLUT.MPB, __state.Graphic, mat);
        Graphics.DrawMesh(mesh, loc, quat, mat, 0, null, 0, mpb);
        return false;
    }

    #endregion

    #region Graphic_RandomRotated Patches

    private static MethodInfo GraphicsRandomRotatedGetRotInRack =>
        AccessTools.Method(typeof(Graphic_RandomRotated), "GetRotInRack");

    [HarmonyPatch(typeof(Graphic_RandomRotated), nameof(Graphic.DrawWorker))]
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
        if (thing == null || !thing.TryGetComp(out CutoutLUT comp)) return true;

        Mesh mesh = __instance.MeshAt(rot);

        float? rotInRack =
            (float?)GraphicsRandomRotatedGetRotInRack.Invoke(__instance, [thing, thingDef, loc.ToIntVec3()]);

        float num = rotInRack ?? (float)(-(double)___maxAngle + thing.thingIDNumber * 542 % (___maxAngle * 2.0));

        float angle = num + extraRotation;
        Vector3 position = loc;
        Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.up);
        Material material = __instance.MatSingleFor(thing);
        MaterialPropertyBlock mpb = comp.UpdateMaterialPropertyBlock(CutoutLUT.MPB, __instance.SubGraphic, material);
        Graphics.DrawMesh(mesh, position, rotation, material, 0, null, 0, mpb);
        return false;
    }

    #endregion
}