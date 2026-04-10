namespace Cosmere.Tools.Models;

public class GemInfo
{
    public string Name { get; set; } = string.Empty;
    public GemDescriptions Descriptions { get; set; } = new();
    public string? DefName { get; set; }
    public ColorInfo Color { get; set; } = new();
    public ColorInfo? ColorTwo { get; set; }
    public ColorInfo? GlowColor { get; set; }
    public bool Stackable { get; set; } = true;
    public float DrawSize { get; set; } = 1f;
    public float BaseBeauty { get; set; }
    public float BaseMarketValue { get; set; }
    public int BaseStormlight { get; set; }
    public int MaxAmount { get; set; } = 100;
    public Dictionary<string, string[]> GenesToGrant { get; set; } = new();
    public BuildableInfo? Buildable { get; set; }
    public MiningInfo? Mining { get; set; }
}