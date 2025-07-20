using System;

namespace Cosmere.Framework.Attribute;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class Profile(string? label = null) : System.Attribute {
    public readonly string? label = label;
}