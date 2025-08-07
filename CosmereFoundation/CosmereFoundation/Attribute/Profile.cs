using System;

namespace Cosmere.Foundation.Attribute;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class Profile(string? label = null) : System.Attribute {
    public readonly string? label = label;
}