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
        top = value;
        right = value;
        bottom = value;
        left = value;
    }

    public Padding(float yValue, float xValue) {
        top = yValue;
        right = xValue;
        bottom = yValue;
        left = xValue;
    }

    public Padding(float topValue, float xValue, float bottomValue) {
        top = topValue;
        right = xValue;
        bottom = bottomValue;
        left = xValue;
    }

    public Padding(float topValue, float rightValue, float bottomValue, float leftValue) {
        top = topValue;
        right = rightValue;
        bottom = bottomValue;
        left = leftValue;
    }

    public float top { get; set; }

    public float right { get; set; }

    public float bottom { get; set; }

    public float left { get; set; }

    public Vector2 ToSymmetricVector2() {
        if (!Mathf.Approximately(right, left)) {
            throw new InvalidOperationException("Padding must have equal right and left to convert to a Vector2");
        }

        if (!Mathf.Approximately(top, bottom)) {
            throw new InvalidOperationException("Padding must have equal top and bottom to convert to a Vector2");
        }

        return new Vector2(right, top);
    }

    public static implicit operator Vector4(Padding padding) {
        return new Vector4(padding.top, padding.right, padding.bottom, padding.left);
    }
}
