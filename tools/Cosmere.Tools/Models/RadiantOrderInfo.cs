namespace Cosmere.Tools.Models;

public class RadiantOrderInfo
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? DefName { get; set; }
    public List<string> Surges { get; set; } = new();
    public List<string> Abilities { get; set; } = new();
    public List<Ideal> Ideals { get; set; } = new();
    public string SprenType { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
}

public class Ideal
{
    public int Level { get; set; }
    public string Text { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}