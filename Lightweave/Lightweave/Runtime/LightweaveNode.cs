using System;
using UnityEngine;

namespace Cosmere.Lightweave.Runtime;

public sealed class LightweaveNode {
    public int CallSiteId;
    public int BuildParentPathHash;
    public List<LightweaveNode> Children = new List<LightweaveNode>();
    public Rect ContentRect;
    public string DebugName = string.Empty;
    public object? ExplicitKey;
    public bool IsFooter;
    public Func<float, float>? Measure;
    public Func<Rect, List<Rect>>? MeasureChildren;
    public Rect MeasuredRect;
    public Action<Rect, Action>? Paint;
    public float? PreferredHeight;
}