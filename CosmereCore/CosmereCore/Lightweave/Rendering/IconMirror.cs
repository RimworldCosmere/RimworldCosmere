using Cosmere.Lightweave.Types;
using UnityEngine;

namespace Cosmere.Lightweave.Rendering;

public static class IconMirror {
    public static Matrix4x4 PushIfRtl(Rect rect, Direction dir) {
        Matrix4x4 saved = GUI.matrix;
        if (dir == Direction.Rtl) {
            Vector2 center = rect.center;
            GUI.matrix = Matrix4x4.TRS(new Vector3(center.x, center.y, 0), Quaternion.identity, new Vector3(-1, 1, 1)) *
                         Matrix4x4.TRS(new Vector3(-center.x, -center.y, 0), Quaternion.identity, Vector3.one);
        }

        return saved;
    }

    public static void Pop(Matrix4x4 saved) {
        GUI.matrix = saved;
    }
}