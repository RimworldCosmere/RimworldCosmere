// tools/Cosmere.Tools/Repo.cs
using System.Runtime.InteropServices;

namespace Cosmere.Tools;

public static class Repo
{
    public static string Cwd => Directory.GetCurrentDirectory();

    // Find repo root: nearest parent containing "About" dirs or .git
    public static string FindRoot(string? start = null)
    {
        var dir = new DirectoryInfo(start ?? Cwd);
        while (dir != null)
        {
            if (Directory.Exists(Path.Combine(dir.FullName, ".git"))) return dir.FullName;
            if (Directory.EnumerateDirectories(dir.FullName, "*", SearchOption.TopDirectoryOnly)
                .Any(d => Directory.Exists(Path.Combine(d, "About")))) return dir.FullName;
            dir = dir.Parent;
        }
        return Cwd;
    }

    public static IEnumerable<string> ModDirs(string root) =>
        Directory.EnumerateDirectories(root, "*", SearchOption.TopDirectoryOnly)
            .Where(d => Directory.Exists(Path.Combine(d, "About")))
            .OrderBy(d => d, StringComparer.OrdinalIgnoreCase);

    public static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    public static bool IsMacOS   => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
    public static string? Env(string key) => Environment.GetEnvironmentVariable(key);
}