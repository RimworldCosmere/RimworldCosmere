using System;
using UnityEngine;
using Vector2 = System.Numerics.Vector2;
using Vector4 = System.Numerics.Vector4;

namespace CosmereFramework.UI;

public struct Padding {
    public static readonly Padding Zero = new Padding(0);
    public static readonly Padding One = new Padding(1);
    public static readonly Padding Four = new Padding(4);

    public float top { get; set; }
    public float right { get; set; }
    public float bottom { get; set; }
    public float left { get; set; }

    public float x {
        get {
            if (!Mathf.Approximately(right, left)) {
                throw new Exception("Padding must have equal values for right and left to get x");
            }

            return right;
        }
    }

    public float y {
        get {
            if (!Mathf.Approximately(top, bottom)) {
                throw new Exception("Padding must have equal values for top and bottom to get y");
            }

            return top;
        }
    }

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

    public static implicit operator Vector2(Padding padding) {
        if (!Mathf.Approximately(padding.right, padding.left)) {
            throw new Exception("Padding must have equal values for right and left to convert to a Vector2");
        }

        if (!Mathf.Approximately(padding.top, padding.bottom)) {
            throw new Exception("Padding must have equal values for top and bottom to convert to a Vector2");
        }

        return new Vector2(padding.right, padding.top);
    }

    public static implicit operator Vector4(Padding padding) {
        return new Vector4(padding.top, padding.right, padding.bottom, padding.left);
    }
}