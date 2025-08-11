// scripts/build-assets.csx
// Run with: dotnet script ./scripts/build-assets.csx

#nullable enable
#r "System.Runtime"
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Runtime.InteropServices;

// ---------- Helpers ----------
static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
static bool IsMacOS   => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
static string Q(string s) => $"\"{s}\"";

static string GetFolderHash(string folderPath)
{
    var files = Directory.EnumerateFiles(folderPath, "*", SearchOption.AllDirectories)
                         .OrderBy(p => p, StringComparer.OrdinalIgnoreCase)
                         .ToArray();
    using var sha = SHA256.Create();
    var sb = new StringBuilder(Math.Max(1024, files.Length * 64));
    foreach (var file in files)
    {
        using var fs = File.OpenRead(file);
        sb.Append(Convert.ToHexString(sha.ComputeHash(fs)));
    }
    return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(sb.ToString())));
}

static string FindScriptDir()
{
    // 1) Look for a .csx in the command line args (absolute or relative)
    var args = Environment.GetCommandLineArgs().Skip(1).ToArray();
    var scriptArg = args
        .Select(a => Path.GetFullPath(a))
        .FirstOrDefault(a => a.EndsWith(".csx", StringComparison.OrdinalIgnoreCase) && File.Exists(a));
    if (!string.IsNullOrEmpty(scriptArg))
        return Path.GetDirectoryName(scriptArg)!;

    // 2) If invoked from repo root with "dotnet script ./scripts/build-assets.csx"
    var cwd = Directory.GetCurrentDirectory();
    var candidate = Path.Combine(cwd, "scripts", "build-assets.csx");
    if (File.Exists(candidate))
        return Path.GetDirectoryName(candidate)!;

    // 3) If invoked from within ./scripts with "dotnet script build-assets.csx"
    candidate = Path.Combine(cwd, "build-assets.csx");
    if (File.Exists(candidate))
        return cwd;

    // 4) Last resort (temp cache); not ideal but prevents nulls
    var exec = System.Reflection.Assembly.GetExecutingAssembly().Location;
    return Path.GetDirectoryName(exec) ?? cwd;
}

// ---------- Paths ----------
var scriptDir = FindScriptDir();
// repo root is one up from ./scripts
var root = Path.GetFullPath(Path.Combine(scriptDir, ".."));

// ---------- Unity path + target ----------
string? unityPath = null;
string? buildTarget = null;

if (IsWindows)
{
    unityPath   = @"C:\Program Files\Unity\Hub\Editor\2022.3.35f1\Editor\Unity.exe";
    buildTarget = "windows";
}
else if (IsMacOS)
{
    unityPath   = "/Applications/Unity/Hub/Editor/2022.3.35f1/Unity.app/Contents/MacOS/Unity";
    buildTarget = "mac";
}

// Fallbacks
bool UnityExists(string p) => !string.IsNullOrWhiteSpace(p) && File.Exists(p);
if (!UnityExists(unityPath ?? ""))
    unityPath = Environment.GetEnvironmentVariable("UNITY_PATH");
if (string.IsNullOrWhiteSpace(buildTarget))
    buildTarget = Environment.GetEnvironmentVariable("UNITY_BUILD_TARGET");

// Validate
if (string.IsNullOrWhiteSpace(unityPath))
{
    Console.WriteLine("Could not find unityPath. On Windows/Mac, update to PowerShell 7; otherwise set UNITY_PATH.");
    Environment.Exit(1);
}
if (string.IsNullOrWhiteSpace(buildTarget))
{
    Console.WriteLine("Could not find buildTarget. Set UNITY_BUILD_TARGET (windows, mac, linux).");
    Environment.Exit(1);
}

// ---------- Discover mods (must contain About/) ----------
var modDirs = Directory.EnumerateDirectories(root, "*", SearchOption.TopDirectoryOnly)
    .Where(d => Directory.Exists(Path.Combine(d, "About")))
    .OrderBy(d => d, StringComparer.OrdinalIgnoreCase)
    .ToArray();

foreach (var d in modDirs) Console.WriteLine($"  {d}");

// ---------- Build loop ----------
foreach (var modDir in modDirs)
{
    var modName   = Path.GetFileName(modDir); // e.g., "CosmereRoshar"
    var shortMod  = modName.StartsWith("Cosmere", StringComparison.OrdinalIgnoreCase) ? modName.Substring("Cosmere".Length) : modName;

    Console.WriteLine("--------------------------------------");
    Console.WriteLine($"Processing {modName}...");

    var srcAssets   = Path.Combine(modDir, "Assets");
    var bundlesDir  = Path.Combine(modDir, "AssetBundles");
    var hashFile    = Path.Combine(bundlesDir, ".lastassetbuildhash");

    if (!Directory.Exists(srcAssets))
    {
        Console.WriteLine($"    Skipping {modName} - no Assets folder found.");
        continue;
    }

    var currentHash  = GetFolderHash(srcAssets);
    var previousHash = File.Exists(hashFile) ? (File.ReadAllText(hashFile) ?? "").Trim() : "";

    Console.WriteLine($"    Testing {currentHash} vs {previousHash}");
    if (string.Equals(currentHash, previousHash, StringComparison.OrdinalIgnoreCase))
    {
        Console.WriteLine($"    No changes detected in {modName}. Skipping build.");
        continue;
    }

    Console.WriteLine("    Changes detected. Continuing with build.");

    // Unity project: prefer sibling ../AssetBuilder; fallback to ./AssetBuilder inside repo
    var unityProject = Path.GetFullPath(Path.Combine(root, "..", "AssetBuilder"));
    if (!Directory.Exists(unityProject))
        unityProject = Path.Combine(root, "AssetBuilder");

    var args = new[]
    {
        "-batchmode",
        "-quit",
        $"-projectPath {Q(unityProject)}",
        "-executeMethod ModAssetBundleBuilder.BuildBundles",
        $"-buildTarget={buildTarget}",
        $"-source={Q(modDir)}"
    };

    Console.WriteLine($"    Building asset bundle: Cosmere.{shortMod}");
    var psi = new ProcessStartInfo
    {
        FileName = unityPath!,
        Arguments = string.Join(" ", args),
        UseShellExecute = false,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        CreateNoWindow = true,
        WorkingDirectory = unityProject
    };

    using var proc = Process.Start(psi)!;
    proc.OutputDataReceived += (_, e) => { if (e.Data is not null) Console.WriteLine("    " + e.Data); };
    proc.ErrorDataReceived  += (_, e) => { if (e.Data is not null) Console.Error.WriteLine("    " + e.Data); };
    proc.BeginOutputReadLine();
    proc.BeginErrorReadLine();
    proc.WaitForExit();

    if (proc.ExitCode != 0)
    {
        Console.WriteLine($"    Unity failed for {shortMod} (exit code {proc.ExitCode}). Crashing build.");
        Environment.Exit(proc.ExitCode);
    }

    Directory.CreateDirectory(bundlesDir);
    File.WriteAllText(hashFile, currentHash, Encoding.ASCII);
    Console.WriteLine($"    Done with {modName}.");
}

Console.WriteLine("All bundles built.");
