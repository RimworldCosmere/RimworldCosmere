using System;

namespace Cosmere.System.Roshar.Dialog;

/// <summary>
///     Rect arithmetic for <see cref="SphereFilter{T}" />, kept free of Unity types so it can be unit tested.
/// </summary>
public static class SphereFilterLayout {
    public const float HeaderHeight = 32f;
    public const float FooterHeight = 40f;
    public const float RowHeight = 30f;
    public const float ScrollbarWidth = 20f;
    public const float CloseWidth = 100f;
    public const float CloseHeight = 30f;

    public static float ScrollY(float inY) {
        return inY + HeaderHeight;
    }

    public static float ScrollHeight(float inHeight) {
        return Math.Max(0f, inHeight - HeaderHeight - FooterHeight);
    }

    public static float ViewWidth(float inWidth) {
        return Math.Max(0f, inWidth - ScrollbarWidth);
    }

    public static float ViewHeight(int sphereCount) {
        return sphereCount * RowHeight;
    }

    public static float CloseX(float inX, float inWidth) {
        return inX + inWidth - CloseWidth;
    }

    public static float CloseY(float inY, float inHeight) {
        return inY + inHeight - CloseHeight;
    }
}
