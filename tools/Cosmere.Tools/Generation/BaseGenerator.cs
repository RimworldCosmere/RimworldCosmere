using System.IO.Abstractions;
using HandlebarsDotNet;

namespace Cosmere.Tools.Generation;

public abstract class BaseGenerator : IGenerator
{
    protected readonly GeneratorOptions Options;
    protected readonly IFileSystem FileSystem;
    protected readonly IHandlebars Handlebars;
    
    protected BaseGenerator(GeneratorOptions options, IFileSystem fileSystem)
    {
        Options = options;
        FileSystem = fileSystem;
        Handlebars = HandlebarsDotNet.Handlebars.Create();
        RegisterHelpers();
    }

    protected virtual void RegisterHelpers()
    {
        Handlebars.RegisterHelper("toDefName", (writer, context, parameters) =>
        {
            if (parameters[0] is string str)
            {
                writer.WriteSafeString(str.Replace(" ", string.Empty));
            }
        });
        
        Handlebars.RegisterHelper("upperFirst", (writer, context, parameters) =>
        {
            if (parameters[0] is string str && !string.IsNullOrEmpty(str))
            {
                writer.WriteSafeString(char.ToUpper(str[0]) + str[1..]);
            }
        });
    }

    protected HandlebarsTemplate<object, object> CompileTemplate(string templateDir, string templateName)
    {
        var templatePath = FileSystem.Path.Combine(templateDir, templateName);
        var templateContent = FileSystem.File.ReadAllText(templatePath);
        return Handlebars.Compile(templateContent);
    }

    protected void WriteGeneratedFile(string dir, string fileName, string content)
    {
        var fullPath = FileSystem.Path.Combine(dir, fileName);
        
        if (Options.Verbose)
        {
            var relativePath = FileSystem.Path.GetRelativePath(Environment.CurrentDirectory, fullPath);
            Console.WriteLine($"{(Options.DryRun ? "[DRY-RUN] " : "")}Generated file: {relativePath}");
        }
        
        if (Options.DryRun) return;

        FileSystem.Directory.CreateDirectory(FileSystem.Path.GetDirectoryName(fullPath)!);
        FileSystem.File.WriteAllText(fullPath, content);
    }

    public abstract Task GenerateAsync();
}