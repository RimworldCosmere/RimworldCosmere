using System;
using UnityEngine;
using Vector2 = System.Numerics.Vector2;
using Vector4 = System.Numerics.Vector4;

namespace Cosmere.Core.UI;

public record struct Padding {
    public static readonly Padding Zero = new Padding(0);
    public static readonly Padding One = new Padding(1);
    public static readonly Padding Four = new Padding(UI.Spacing.Get(.25));
    public static readonly Padding Spacing = new Padding(UI.Spacing.Get());

    public Padding(float value) {
        Top = value;
        Right = value;
        Bottom = value;
        Left = value;
    }

    public Padding(float yValue, float xValue) {
        Top = yValue;
        Right = xValue;
        Bottom = yValue;
        Left = xValue;
    }

    public Padding(float topValue, float xValue, float bottomValue) {
        Top = topValue;
        Right = xValue;
        Bottom = bottomValue;
        Left = xValue;
    }

    public Padding(float topValue, float rightValue, float bottomValue, float leftValue) {
        Top = topValue;
        Right = rightValue;
        Bottom = bottomValue;
        Left = leftValue;
    }

    public float Top { get; set; }

    public float Right { get; set; }

    public float Bottom { get; set; }

    public float Left { get; set; }

    public Vector2 ToSymmetricVector2() {
        if (!Mathf.Approximately(Right, Left)) {
            throw new InvalidOperationException("Padding must have equal right and left to convert to a Vector2");
        }

        if (!Mathf.Approximately(Top, Bottom)) {
            throw new InvalidOperationException("Padding must have equal top and bottom to convert to a Vector2");
        }

        return new Vector2(Right, Top);
    }

    public static implicit operator Vector4(Padding padding) {
        return new Vector4(padding.Top, padding.Right, padding.Bottom, padding.Left);
    }
}
