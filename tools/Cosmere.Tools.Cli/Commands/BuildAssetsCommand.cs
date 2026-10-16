#nullable enable
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace Cosmere.Tools.Cli.Commands;

public static class BuildAssetsCommand {
    public static Command Create() {
        var cmd = new Command("build-assets", "Build Unity AssetBundles for mods that changed their Assets");

        var unityPathOpt = new Option<string?>("--unity-path") {
            Description = "Path to Unity editor (overrides UNITY_PATH)",
        };
        var unityProjectOpt = new Option<string?>("--unity-project") {
            Description = "Unity project path (default: ../AssetBuilder or ./AssetBuilder)",
        };
        var targetOpt = new Option<string?>("--target") {
            Description = "Build target: windows | mac | linux (overrides UNITY_BUILD_TARGET)",
        };
        var verboseOpt = new Option<bool>("--verbose") {
            Description = "Verbose logging",
        };

        cmd.Options.Add(unityPathOpt);
        cmd.Options.Add(unityProjectOpt);
        cmd.Options.Add(targetOpt);
        cmd.Options.Add(verboseOpt);

        // New handler style: SetAction with ParseResult
        cmd.SetAction((ParseResult pr) => {
            var unityPathArg = pr.GetValue(unityPathOpt);
            var unityProjectArg = pr.GetValue(unityProjectOpt);
            var targetArg = pr.GetValue(targetOpt);
            var verbose = pr.GetValue(verboseOpt);

            try {
                var repoRoot = FindRepoRoot();
                var unityPath = ResolveUnityPath(unityPathArg);
                var buildTarget = ResolveBuildTarget(targetArg);
                var unityProject = ResolveUnityProject(repoRoot, unityProjectArg);

                if (string.IsNullOrWhiteSpace(unityPath) || !File.Exists(unityPath))
                    throw new InvalidOperationException("Unity editor not found. Set --unity-path or UNITY_PATH.");

                if (string.IsNullOrWhiteSpace(buildTarget)) {
                    throw new InvalidOperationException(
                        "Build target not resolved. Use --target or UNITY_BUILD_TARGET (windows|mac|linux).");
                }

                Log(verbose, $"repo:   {repoRoot}");
                Log(verbose, $"unity:  {unityPath}");
                Log(verbose, $"uproject: {unityProject}");
                Log(verbose, $"target: {buildTarget}");

                var modDirs = Directory.EnumerateDirectories(repoRoot, "*", SearchOption.TopDirectoryOnly)
                    .Where(d => Directory.Exists(Path.Combine(d, "About")))
                    .OrderBy(d => d, StringComparer.OrdinalIgnoreCase)
                    .ToArray();

                Console.WriteLine("mods:");
                foreach (var d in modDirs) Console.WriteLine($"  {d}");

                foreach (var modDir in modDirs) {
                    var modName = Path.GetFileName(modDir);
                    var assetsDir = Path.Combine(modDir, "Assets");
                    if (!Directory.Exists(assetsDir)) {
                        Log(verbose, $"- {modName}: no Assets/, skipping");
                        continue;
                    }

                    var bundlesDir = Path.Combine(modDir, "AssetBundles");
                    Directory.CreateDirectory(bundlesDir);
                    var hashFile = Path.Combine(bundlesDir, ".lastassetbuildhash");

                    var currentHash = FolderHash(assetsDir);
                    var previousHash = File.Exists(hashFile) ? (File.ReadAllText(hashFile) ?? string.Empty).Trim() : string.Empty;

                    if (string.Equals(currentHash, previousHash, StringComparison.OrdinalIgnoreCase)) {
                        Console.WriteLine($"- {modName}: no changes");
                        continue;
                    }

                    Console.WriteLine($"* {modName}: building…");

                    var args = new[]
                    {
                        "-batchmode",
                        "-quit",
                        $"-projectPath \"{unityProject}\"",
                        "-executeMethod ModAssetBundleBuilder.BuildBundles",
                        $"-buildTarget={buildTarget}",
                        $"-source=\"{modDir}\"",
                    };

                    var exit = RunUnity(unityPath, unityProject, args, verbose);
                    if (exit != 0)
                        throw new InvalidOperationException($"Unity failed for {modName} (exit {exit}).");

                    File.WriteAllText(hashFile, currentHash, Encoding.ASCII);
                    Console.WriteLine($"✓ {modName}: done");
                }

                Console.WriteLine("All bundles built.");
                return 0;
            } catch (Exception ex) {
                Console.Error.WriteLine($"ERROR: {ex.Message}");
                return 1;
            }
        });

        return cmd;
    }

    // ---------- Helpers ----------
    private static void Log(bool verbose, string message) {
        if (verbose) Console.WriteLine(message);
    }

    private static string FindRepoRoot() {
        // Start at CWD and walk up until we see a .git folder OR a folder that contains top-level About/ dirs.
        var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (dir != null) {
            if (Directory.Exists(Path.Combine(dir.FullName, ".git")))
                return dir.FullName;

            var hasMods = Directory.EnumerateDirectories(dir.FullName, "*", SearchOption.TopDirectoryOnly)
                .Any(d => Directory.Exists(Path.Combine(d, "About")));
            if (hasMods) return dir.FullName;

            dir = dir.Parent;
        }

        return Directory.GetCurrentDirectory();
    }

    private static string? ResolveUnityPath(string? cli) {
        if (!string.IsNullOrWhiteSpace(cli)) return cli;

        var env = Environment.GetEnvironmentVariable("UNITY_PATH");
        if (!string.IsNullOrWhiteSpace(env)) return env;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return @"C:\Program Files\Unity\Hub\Editor\2022.3.35f1\Editor\Unity.exe";

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return "/Applications/Unity/Hub/Editor/2022.3.35f1/Unity.app/Contents/MacOS/Unity";

        // Linux: unlikely in your flow; require explicit UNITY_PATH if needed
        return null;
    }

    private static string ResolveBuildTarget(string? cli) {
        if (!string.IsNullOrWhiteSpace(cli)) return Normalize(cli);
        var env = Environment.GetEnvironmentVariable("UNITY_BUILD_TARGET");
        if (!string.IsNullOrWhiteSpace(env)) return Normalize(env);

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return "windows";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) return "mac";
        return "linux";

        static string Normalize(string s) {
            s = s.Trim().ToLowerInvariant();
            return s switch {
                "win" or "windows" => "windows",
                "mac" or "osx" => "mac",
                "linux" => "linux",
                _ => s,
            };
        }
    }

    private static string ResolveUnityProject(string repoRoot, string? cli) {
        if (!string.IsNullOrWhiteSpace(cli)) return Path.GetFullPath(cli);

        // Prefer sibling ../AssetBuilder; fallback to ./AssetBuilder
        var sibling = Path.GetFullPath(Path.Combine(repoRoot, "..", "AssetBuilder"));
        if (Directory.Exists(sibling)) return sibling;

        var local = Path.Combine(repoRoot, "AssetBuilder");
        return Directory.Exists(local) ? local : repoRoot; // last resort
    }

    private static string FolderHash(string folderPath) {
        var files = Directory.EnumerateFiles(folderPath, "*", SearchOption.AllDirectories)
            .OrderBy(p => p, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        using var sha = SHA256.Create();
        var sb = new StringBuilder(Math.Max(1024, files.Length * 64));

        foreach (var file in files) {
            using var fs = File.OpenRead(file);
            sb.Append(Convert.ToHexString(sha.ComputeHash(fs)));
        }

        var finalBytes = Encoding.UTF8.GetBytes(sb.ToString());
        return Convert.ToHexString(SHA256.HashData(finalBytes));
    }

    private static int RunUnity(string unityPath, string workingDir, string[] args, bool verbose) {
        var psi = new ProcessStartInfo {
            FileName = unityPath,
            Arguments = string.Join(' ', args),
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            WorkingDirectory = workingDir,
        };

        using var p = Process.Start(psi)!;
        p.OutputDataReceived += (_, e) => {
            if (e.Data is not null && verbose) Console.WriteLine("  " + e.Data);
        };
        p.ErrorDataReceived += (_, e) => {
            if (e.Data is not null) Console.Error.WriteLine("  " + e.Data);
        };
        p.BeginOutputReadLine();
        p.BeginErrorReadLine();
        p.WaitForExit();
        return p.ExitCode;
    }
}
