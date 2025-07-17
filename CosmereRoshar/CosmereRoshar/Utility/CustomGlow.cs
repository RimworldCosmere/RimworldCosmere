using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace CosmereRoshar {
    public static class GlowCircleRenderer {
        private static Dictionary<Color, Material> cachedMaterials = new Dictionary<Color, Material>();

        public static void DrawCustomCircle(Pawn caster, float radius, Color color) {
            color.a = 0.15f;
            Vector3 center = caster.DrawPos;
            center.y = AltitudeLayer.MetaOverlays.AltitudeFor();

            Material mat = SolidColorMaterials.SimpleSolidColorMaterial(color);

            // Build a matrix for position/rotation/scale
            Matrix4x4 matrix = default;
            matrix.SetTRS(
                pos: center,
                q: Quaternion.identity,
                s: new Vector3(radius, 1f, radius)
            );

            Mesh circleMesh = MeshPool.circle;
            Graphics.DrawMesh(circleMesh, matrix, mat, -1);

        }

        public static void DrawCustomGlow(Pawn caster, float radius, Color color) {
            Vector3 center = caster.DrawPos;
            center.y = AltitudeLayer.MetaOverlays.AltitudeFor();


            float alpha = 0.15f;
            while (alpha > 0) {

                color.a = alpha;
                Material mat = SolidColorMaterials.SimpleSolidColorMaterial(color);
                // Build a matrix for position/rotation/scale
                Matrix4x4 matrix = default;
                matrix.SetTRS(
                    pos: center,
                    q: Quaternion.identity,
                    s: new Vector3(radius, 1f, radius)
                );

                Mesh circleMesh = MeshPool.circle;
                Graphics.DrawMesh(circleMesh, matrix, mat, -2);

                alpha -= 0.01f;
                radius += 0.05f;
            }
        }


        public static void DrawCustomCircle(IntVec3 center, float radius, Color color) {

            DrawCustomCircle(new Vector3(center.x, center.y, center.z), radius, color, 0.15f);
        }



        public static void DrawCustomCircle(Vector3 center, float radius, Color color) {
            DrawCustomCircle(center, radius, color, 0.15f);
        }

        public static void DrawCustomCircle(Vector3 center, float radius, Color color, float alpha) {
            color.a = alpha;
            center.y = AltitudeLayer.MetaOverlays.AltitudeFor();

            Material mat = SolidColorMaterials.SimpleSolidColorMaterial(color);

            // Build a matrix for position/rotation/scale
            Matrix4x4 matrix = default;
            matrix.SetTRS(
                pos: center,
                q: Quaternion.identity,
                s: new Vector3(radius, 1f, radius)
            );

            Mesh circleMesh = MeshPool.circle;
            Graphics.DrawMesh(circleMesh, matrix, mat, -1);

        }
    }
}