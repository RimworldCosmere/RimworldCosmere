using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cosmere.Core.UI.Lightweave.Runtime;

public sealed class LightweaveNode
{
    public int CallSiteId;
    public object? ExplicitKey;
    public string DebugName = string.Empty;
    public Action<Rect, Action>? Paint;
    public Func<Rect, List<Rect>>? MeasureChildren;
    public List<LightweaveNode> Children = new List<LightweaveNode>();
    public Rect MeasuredRect;
    public Rect ContentRect;
}
