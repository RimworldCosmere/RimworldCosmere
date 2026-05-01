using System;

namespace Cosmere.Lightweave.Doc;

[AttributeUsage(
    AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property,
    AllowMultiple = false,
    Inherited = false)]
public sealed class DocUsageAttribute : Attribute { }
