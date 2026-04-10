using System.CommandLine;
using Cosmere.Tools.Cli.Commands;
using dotenv.net;

DotEnv.Load(new DotEnvOptions(ignoreExceptions: true)); 
var root = new RootCommand("Cosmere Dev CLI");
root.Subcommands.Add(BuildAssetsCommand.Create());
root.Subcommands.Add(GenerateCommand.Create());

return await root.Parse(args).InvokeAsync();