using System.Text;

namespace Cosmere.Core.BetaHub;

/// <summary>
///     Everything the client collects about the running game. Kept free of Unity and Verse
///     types so the formatting can be unit tested.
/// </summary>
public sealed class DiagnosticsFacts {
    public string Revision = string.Empty;
    public string BuildTime = string.Empty;
    public string SteamId = string.Empty;
    public string SteamPersona = string.Empty;
    public string GameVersion = string.Empty;
    public string OperatingSystem = string.Empty;
    public string GraphicsDevice = string.Empty;
    public int SystemMemoryMb;
    public IReadOnlyList<string> ActiveMods = [];
}

public static class DiagnosticsText {
    public static string Build(DiagnosticsFacts facts, string logTail) {
        StringBuilder builder = new StringBuilder();

        builder.AppendLine("=== Cosmere ===");
        builder.AppendLine($"Revision: {facts.Revision}");
        builder.AppendLine($"Built: {facts.BuildTime}");
        builder.AppendLine();
        builder.AppendLine("=== Reporter ===");
        builder.AppendLine($"SteamID: {facts.SteamId}");
        builder.AppendLine($"Persona: {facts.SteamPersona}");
        builder.AppendLine();
        builder.AppendLine("=== System ===");
        builder.AppendLine($"RimWorld: {facts.GameVersion}");
        builder.AppendLine($"OS: {facts.OperatingSystem}");
        builder.AppendLine($"GPU: {facts.GraphicsDevice}");
        builder.AppendLine($"RAM: {facts.SystemMemoryMb} MB");
        builder.AppendLine();
        builder.AppendLine($"=== Active mods ({facts.ActiveMods.Count}) ===");
        foreach (string mod in facts.ActiveMods) {
            builder.AppendLine(mod);
        }

        builder.AppendLine();
        builder.AppendLine("=== Player.log tail ===");
        builder.Append(logTail);

        return builder.ToString();
    }

    /// <summary>
    ///     One line BetaHub parses into structured device data on the issue.
    /// </summary>
    public static string BuildDeviceInfo(DiagnosticsFacts facts) {
        return $"{facts.OperatingSystem} | {facts.GraphicsDevice} | {facts.SystemMemoryMb} MB RAM";
    }

    /// <summary>
    ///     Suggestions accept no attachments, so build context rides in the description instead.
    ///     The SteamID stays out since that text is public.
    /// </summary>
    public static string BuildInlineFooter(DiagnosticsFacts facts) {
        string cosmereMods = string.Join(", ", facts.ActiveMods.Where(m => m.StartsWith("Cosmere")));

        return $"\n\n---\nCosmere {facts.Revision} | RimWorld {facts.GameVersion} | {facts.OperatingSystem}\n{cosmereMods}";
    }
}
