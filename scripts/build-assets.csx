// scripts/build-assets.csx
// Run with: dotnet script ./scripts/build-assets.csx [-- --force]

#nullable enable
#r "System.Runtime"
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Runtime.InteropServices;

// ---------- Parse arguments ----------
var scriptArgs = Args.ToArray();
var forceRebuild = scriptArgs.Contains("--force", StringComparer.OrdinalIgnoreCase);
var verbose = scriptArgs.Contains("--verbose", StringComparer.OrdinalIgnoreCase) || scriptArgs.Contains("-v", StringComparer.OrdinalIgnoreCase);

// ---------- Helpers ----------
static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
static bool IsMacOS   => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
static bool IsLinux   => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

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

static bool RunCommand(string command, string arguments, string? workingDir = null)
{
    var psi = new ProcessStartInfo
    {
        FileName = command,
        Arguments = arguments,
        UseShellExecute = false,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        CreateNoWindow = true,
        WorkingDirectory = workingDir ?? Directory.GetCurrentDirectory()
    };

    using var proc = Process.Start(psi);
    if (proc == null) return false;
    
    proc.OutputDataReceived += (_, e) => { if (e.Data is not null) Console.WriteLine("    " + e.Data); };
    proc.ErrorDataReceived  += (_, e) => { if (e.Data is not null) Console.Error.WriteLine("    " + e.Data); };
    proc.BeginOutputReadLine();
    proc.BeginErrorReadLine();
    proc.WaitForExit();
    
    return proc.ExitCode == 0;
}

// ---------- Paths ----------
var scriptDir = FindScriptDir();
// repo root is one up from ./scripts
var root = Path.GetFullPath(Path.Combine(scriptDir, ".."));

Console.WriteLine($"Force rebuild: {forceRebuild}");

// ---------- Install/Update AssetBundleBuilder if needed ----------
const string RequiredToolVersion = "4.1.0";
Console.WriteLine($"Checking for AssetBundleBuilder tool (version {RequiredToolVersion})...");

var checkProc = Process.Start(new ProcessStartInfo
{
    FileName = "dotnet",
    Arguments = "tool list --global",
    UseShellExecute = false,
    RedirectStandardOutput = true,
    CreateNoWindow = true
});
checkProc?.WaitForExit();
var toolOutput = checkProc?.StandardOutput.ReadToEnd() ?? "";

// Parse the tool output to check for the tool and its version
var hasCorrectVersion = false;
var hasWrongVersion = false;
var lines = toolOutput.Split('\n', StringSplitOptions.RemoveEmptyEntries);
foreach (var line in lines)
{
    if (line.Contains("cryptiklemur.assetbundlebuilder", StringComparison.OrdinalIgnoreCase))
    {
        if (line.Contains(RequiredToolVersion))
        {
            hasCorrectVersion = true;
            Console.WriteLine($"  Found CryptikLemur.AssetBundleBuilder version {RequiredToolVersion}");
        }
        else
        {
            hasWrongVersion = true;
            Console.WriteLine($"  Found CryptikLemur.AssetBundleBuilder but wrong version: {line.Trim()}");
        }
        break;
    }
}

if (!hasCorrectVersion)
{
    if (hasWrongVersion)
    {
        Console.WriteLine($"Updating CryptikLemur.AssetBundleBuilder to version {RequiredToolVersion}...");
        RunCommand("dotnet", "tool uninstall --global CryptikLemur.AssetBundleBuilder");
    }
    else
    {
        Console.WriteLine($"Installing CryptikLemur.AssetBundleBuilder version {RequiredToolVersion}...");
    }
    
    if (!RunCommand("dotnet", $"tool install --global CryptikLemur.AssetBundleBuilder --version {RequiredToolVersion}"))
    {
        Console.WriteLine("Failed to install AssetBundleBuilder tool!");
        Environment.Exit(1);
    }
    Console.WriteLine("AssetBundleBuilder installed/updated successfully.");
}

// ---------- Discover mods (must contain About/) ----------
var modDirs = Directory.EnumerateDirectories(root, "*", SearchOption.TopDirectoryOnly)
    .Where(d => Directory.Exists(Path.Combine(d, "About")))
    .OrderBy(d => d, StringComparer.OrdinalIgnoreCase)
    .ToArray();

Console.WriteLine($"Found {modDirs.Length} mods:");
foreach (var d in modDirs) Console.WriteLine($"  {Path.GetFileName(d)}");

// ---------- Build loop ----------
foreach (var modDir in modDirs)
{
    var modName   = Path.GetFileName(modDir); // e.g., "CosmereRoshar"
    var shortMod  = modName.StartsWith("Cosmere", StringComparison.OrdinalIgnoreCase) 
        ? modName.Substring("Cosmere".Length) 
        : modName;
    var bundleName = $"cosmere.{shortMod.ToLowerInvariant()}";

    Console.WriteLine("--------------------------------------");
    Console.WriteLine($"Processing {modName}...");

    var srcAssets   = Path.Combine(modDir, "Assets");
    var bundlesDir  = Path.Combine(modDir, "AssetBundles");
    var hashFile    = Path.Combine(bundlesDir, $".lastassetbuildhash");

    if (!Directory.Exists(srcAssets))
    {
        Console.WriteLine($"    Skipping {modName} - no Assets folder found.");
        continue;
    }

    // Check hash for incremental builds (includes tool version)
    var folderHash = GetFolderHash(srcAssets);
    var currentHash = $"{folderHash}:{RequiredToolVersion}";
    var previousHash = File.Exists(hashFile) ? (File.ReadAllText(hashFile) ?? "").Trim() : "";

    if (!forceRebuild)
    {
        Console.WriteLine($"    Current hash:  {currentHash}");
        Console.WriteLine($"    Previous hash: {previousHash}");
        
        if (string.Equals(currentHash, previousHash, StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine($"    No changes detected in {modName}. Skipping build.");
            continue;
        }
    }
    else
    {
        Console.WriteLine("    Force rebuild enabled - cleaning existing bundles.");
        if (Directory.Exists(bundlesDir))
        {
            Directory.Delete(bundlesDir, true);
        }
    }

    Console.WriteLine("    Changes detected or force rebuild. Building asset bundles...");

    // Build using AssetBundleBuilder
    Console.WriteLine($"    Building asset bundle: {bundleName}");

    // Build arguments: use --debug for verbose mode, otherwise -v for normal verbosity
    var buildArgs = verbose ? "--debug --ci --non-interactive" : "-v";

    if (!RunCommand("assetbundlebuilder", buildArgs, modDir))
    {
        Console.WriteLine($"    AssetBundleBuilder failed for {modName}!");
        Environment.Exit(1);
    }

    // Save hash for next build
    Directory.CreateDirectory(bundlesDir);
    File.WriteAllText(hashFile, currentHash, Encoding.ASCII);
    Console.WriteLine($"    Done with {modName}.");
}

Console.WriteLine("--------------------------------------");
Console.WriteLine("All bundles built successfully!");