using System.Drawing;

namespace Cosmere.Tools.Models;

public class GemInfo
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? DefName { get; set; }
    public Color Color { get; set; }
    public Color? ColorTwo { get; set; }
    public bool Stackable { get; set; } = true;
    public float DrawSize { get; set; } = 1f;
    public float Beauty { get; set; }
    public float MarketValue { get; set; }
    public int MaxAmount { get; set; } = 100;
    public Dictionary<string, string[]> GenesToGrant { get; set; } = new();
    public BuildableInfo? Buildable { get; set; }
    public MiningInfo? Mining { get; set; }
}