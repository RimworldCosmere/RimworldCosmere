namespace Cosmere.Tools.Models;

public class SurgeInfo {
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string? DefName { get; set; }

    public List<string> Abilities { get; set; } = new();
}
