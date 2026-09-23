using System.CommandLine;
using Cosmere.Tools.Cli.Commands;

LoadDotEnv();
var root = new RootCommand("Cosmere Dev CLI");
root.Subcommands.Add(BuildAssetsCommand.Create());
root.Subcommands.Add(GenerateCommand.Create());

return await root.Parse(args).InvokeAsync();

static void LoadDotEnv() {
    string path = Path.Combine(Directory.GetCurrentDirectory(), ".env");
    if (!File.Exists(path)) return;
    foreach (string line in File.ReadAllLines(path)) {
        if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#')) continue;
        int eq = line.IndexOf('=');
        if (eq < 1) continue;
        string key = line[..eq].Trim();
        string value = line[(eq + 1)..].Trim();
        Environment.SetEnvironmentVariable(key, value);
    }
}
