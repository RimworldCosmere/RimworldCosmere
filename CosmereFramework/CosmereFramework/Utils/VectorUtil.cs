using UnityEngine;
using Verse;

namespace CosmereFramework.Utils {
    public static class VectorUtil {
        public static IntVec3 Direction8(this Vector3 offset) {
            var angle = Mathf.Atan2(offset.z, offset.x) * Mathf.Rad2Deg;
            angle = (angle + 360f + 22.5f) % 360f; // Snap to 45° sectors
            var dir = Mathf.FloorToInt(angle / 45f);

            return GenAdj.AdjacentCellsAround[dir];
        }
    }
}