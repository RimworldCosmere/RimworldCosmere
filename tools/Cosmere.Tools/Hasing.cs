// tools/Cosmere.Tools/Hashing.cs
using System.Security.Cryptography;
using System.Text;

namespace Cosmere.Tools;

public static class Hashing
{
    public static string FolderHash(string folder)
    {
        using var sha = SHA256.Create();
        var files = Directory.EnumerateFiles(folder, "*", SearchOption.AllDirectories).OrderBy(p => p);
        var sb = new StringBuilder();
        foreach (var f in files)
            using (var fs = File.OpenRead(f))
                sb.Append(Convert.ToHexString(sha.ComputeHash(fs)));
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(sb.ToString())));
    }
}