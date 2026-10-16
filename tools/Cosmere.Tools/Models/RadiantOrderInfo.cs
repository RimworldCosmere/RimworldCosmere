namespace Cosmere.Tools.Models;

public class RadiantOrderInfo {
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string? DefName { get; set; }

    public List<string> Surges { get; set; } = new();

    public List<string> Abilities { get; set; } = new();

    public List<Ideal> Ideals { get; set; } = new();

    public string SprenType { get; set; } = string.Empty;

    public string SprenDescription { get; set; } = string.Empty;

    public List<string> SprenNamePool { get; set; } = new();

    public ColorInfo? Color { get; set; }

    public string Gemstone { get; set; } = string.Empty;
}

public class Ideal {
    public int StormlightMax { get; set; } = 0;

    public string Label { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public List<string> Quotes { get; set; } = new();

    public List<string> Abilities { get; set; } = new();
}
