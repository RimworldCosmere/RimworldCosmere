using System.IO.Abstractions;
using Cosmere.Tools.Extensions;
using HandlebarsDotNet;
using Cosmere.Tools.Models;

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
        // String transformation helpers
        Handlebars.RegisterHelper("toDefName", (writer, context, parameters) =>
        {
            if (parameters[0] is string str)
            {
                writer.WriteSafeString(str.ToDefName());
            }
        });
        
        Handlebars.RegisterHelper("defName", (writer, context, parameters) =>
        {
            if (parameters[0] is string str)
            {
                writer.WriteSafeString(str.ToDefName());
            }
        });
        
        Handlebars.RegisterHelper("upperFirst", (writer, context, parameters) =>
        {
            if (parameters[0] is string str && !string.IsNullOrEmpty(str))
            {
                writer.WriteSafeString(char.ToUpper(str[0]) + str[1..]);
            }
        });
        
        Handlebars.RegisterHelper("capitalize", (writer, context, parameters) =>
        {
            if (parameters.Length > 0 && parameters[0] != null)
            {
                var str = parameters[0].ToString();
                if (!string.IsNullOrEmpty(str))
                {
                    writer.WriteSafeString(char.ToUpper(str[0]) + str[1..]);
                }
            }
        });
        
        Handlebars.RegisterHelper("lower", (writer, context, parameters) =>
        {
            if (parameters[0] is string str)
            {
                writer.WriteSafeString(str.ToLowerInvariant());
            }
        });
        
        Handlebars.RegisterHelper("title", (writer, context, parameters) =>
        {
            if (parameters[0] is string str)
            {
                writer.WriteSafeString(str.ToTitleCase());
            }
        });

        // Collection helpers
        Handlebars.RegisterHelper("join", (writer, context, parameters) =>
        {
            if (parameters.Length >= 2 && parameters[0] is IEnumerable<object> collection && parameters[1] is string separator)
            {
                var joined = string.Join(separator, collection.Select(x => x?.ToString() ?? ""));
                writer.WriteSafeString(joined);
            }
        });
        
        Handlebars.RegisterHelper("count", (writer, context, parameters) =>
        {
            if (parameters[0] is IEnumerable<object> collection)
            {
                writer.WriteSafeString(collection.Count().ToString());
            }
        });

        // Logic helpers
        Handlebars.RegisterHelper("eq", (writer, context, parameters) =>
        {
            if (parameters.Length >= 2)
            {
                writer.WriteSafeString((parameters[0]?.Equals(parameters[1]) == true).ToString().ToLower());
            }
        });
        
        Handlebars.RegisterHelper("ne", (writer, context, parameters) =>
        {
            if (parameters.Length >= 2)
            {
                writer.WriteSafeString((parameters[0]?.Equals(parameters[1]) != true).ToString().ToLower());
            }
        });
        
        Handlebars.RegisterHelper("lt", (writer, context, parameters) =>
        {
            if (parameters.Length >= 2 && 
                double.TryParse(parameters[0]?.ToString(), out var v1) && 
                double.TryParse(parameters[1]?.ToString(), out var v2))
            {
                writer.WriteSafeString((v1 < v2).ToString().ToLower());
            }
        });
        
        Handlebars.RegisterHelper("gt", (writer, context, parameters) =>
        {
            if (parameters.Length >= 2 && 
                double.TryParse(parameters[0]?.ToString(), out var v1) && 
                double.TryParse(parameters[1]?.ToString(), out var v2))
            {
                writer.WriteSafeString((v1 > v2).ToString().ToLower());
            }
        });
        
        Handlebars.RegisterHelper("lte", (writer, context, parameters) =>
        {
            if (parameters.Length >= 2 && 
                double.TryParse(parameters[0]?.ToString(), out var v1) && 
                double.TryParse(parameters[1]?.ToString(), out var v2))
            {
                writer.WriteSafeString((v1 <= v2).ToString().ToLower());
            }
        });
        
        Handlebars.RegisterHelper("gte", (writer, context, parameters) =>
        {
            if (parameters.Length >= 2 && 
                double.TryParse(parameters[0]?.ToString(), out var v1) && 
                double.TryParse(parameters[1]?.ToString(), out var v2))
            {
                writer.WriteSafeString((v1 >= v2).ToString().ToLower());
            }
        });
        
        Handlebars.RegisterHelper("and", (writer, context, parameters) =>
        {
            var result = parameters.All(p => p != null && 
                (p is bool b ? b : !string.IsNullOrEmpty(p.ToString())));
            writer.WriteSafeString(result.ToString().ToLower());
        });
        
        Handlebars.RegisterHelper("or", (writer, context, parameters) =>
        {
            var result = parameters.Any(p => p != null && 
                (p is bool b ? b : !string.IsNullOrEmpty(p.ToString())));
            writer.WriteSafeString(result.ToString().ToLower());
        });
        
        Handlebars.RegisterHelper("isdefined", (writer, context, parameters) =>
        {
            writer.WriteSafeString((parameters.Length > 0 && parameters[0] != null).ToString().ToLower());
        });

        // Math helpers
        Handlebars.RegisterHelper("multiply", (writer, context, parameters) =>
        {
            if (parameters.Length >= 2 && 
                double.TryParse(parameters[0]?.ToString(), out var v1) && 
                double.TryParse(parameters[1]?.ToString(), out var v2))
            {
                writer.WriteSafeString((v1 * v2).ToString("F6"));
            }
        });
        
        Handlebars.RegisterHelper("add", (writer, context, parameters) =>
        {
            if (parameters.Length >= 2 && 
                double.TryParse(parameters[0]?.ToString(), out var v1) && 
                double.TryParse(parameters[1]?.ToString(), out var v2))
            {
                writer.WriteSafeString((v1 + v2).ToString());
            }
        });
        
        Handlebars.RegisterHelper("getStatForStage", (writer, context, parameters) =>
        {
            if (parameters.Length >= 2 && 
                int.TryParse(parameters[0]?.ToString(), out var stage) && 
                double.TryParse(parameters[1]?.ToString(), out var step))
            {
                var result = 1 + step * (stage + 1);
                // Format with up to 8 decimal places, then trim trailing zeros
                var formatted = result.ToString("F8").TrimEnd('0').TrimEnd('.');
                writer.WriteSafeString(formatted);
            }
        });

        // Compounded charge pays out ten times harder. Naively scaling the step
        // drives "lower is better" stats negative - Gold's IncomingDamageFactor
        // would reach -8.6, i.e. damage that heals - so amplify the benefit each
        // factor represents rather than the step itself.
        Handlebars.RegisterHelper("getCompoundedStatForStage", (writer, context, parameters) =>
        {
            if (parameters.Length >= 2 &&
                int.TryParse(parameters[0]?.ToString(), out var stage) &&
                double.TryParse(parameters[1]?.ToString(), out var step))
            {
                const double amplification = 10.0;
                double result;

                if (step >= 0)
                {
                    result = 1 + step * amplification * (stage + 1);
                }
                else
                {
                    // Benefit of a sub-1 factor is (1/v - 1); scale that, then invert
                    // back. Stays positive and monotonic for every metal.
                    var ordinary = Math.Max(1 + step * (stage + 1), 0.01);
                    result = Math.Max(1.0 / (1.0 + amplification * (1.0 / ordinary - 1.0)), 0.001);
                }

                var formatted = result.ToString("F8").TrimEnd('0').TrimEnd('.');
                writer.WriteSafeString(formatted);
            }
        });

        // Offsets have no reciprocal reading, so they scale their step directly.
        Handlebars.RegisterHelper("getCompoundedOffsetForStage", (writer, context, parameters) =>
        {
            if (parameters.Length >= 2 &&
                int.TryParse(parameters[0]?.ToString(), out var stage) &&
                double.TryParse(parameters[1]?.ToString(), out var step))
            {
                var result = 1 + step * 10.0 * (stage + 1);
                var formatted = result.ToString("F8").TrimEnd('0').TrimEnd('.');
                writer.WriteSafeString(formatted);
            }
        });

        // Color helpers
        Handlebars.RegisterHelper("rgb", (writer, context, parameters) =>
        {
            if (parameters[0] is ColorInfo color)
            {
                writer.WriteSafeString($"({color.R}, {color.G}, {color.B})");
            }
        });
        
        Handlebars.RegisterHelper("rgba", (writer, context, parameters) =>
        {
            if (parameters.Length >= 2 && parameters[0] is ColorInfo color && 
                double.TryParse(parameters[1]?.ToString(), out var alpha))
            {
                writer.WriteSafeString($"({color.R}, {color.G}, {color.B}, {alpha})");
            }
        });

        // Range helper
        Handlebars.RegisterHelper("range", (writer, context, parameters) =>
        {
            if (parameters[0] is int[] range && range.Length >= 2)
            {
                writer.WriteSafeString($"{range[0]}~{range[1]}");
            }
        });

        // Fallback helper
        Handlebars.RegisterHelper("fallback", (writer, context, parameters) =>
        {
            if (parameters.Length >= 2)
            {
                var value = parameters[0];
                var fallback = parameters[1];
                writer.WriteSafeString((value ?? fallback)?.ToString() ?? "");
            }
        });

        // MayRequire and WithoutRequire helpers
        Handlebars.RegisterHelper("mayRequire", (writer, context, parameters) =>
        {
            if (parameters.Length > 0 && parameters[0] is string value)
            {
                if (value.Contains(':'))
                {
                    var parts = value.Split(':');
                    writer.WriteSafeString($" MayRequire=\"{parts[1]}\"");
                }
            }
        });
        
        Handlebars.RegisterHelper("withoutRequire", (writer, context, parameters) =>
        {
            if (parameters.Length > 0 && parameters[0] is string value)
            {
                if (value.Contains(':'))
                {
                    writer.WriteSafeString(value.Split(':')[0]);
                }
                else
                {
                    writer.WriteSafeString(value);
                }
            }
        });

        // Block helpers
        Handlebars.RegisterHelper("times", (output, options, context, arguments) => 
        {
            int count = Convert.ToInt32(arguments[0]);
    
            for (int i = 0; i < count; i++)
            {
                options.Data.CreateProperty("key", i, out _);
                options.Data.CreateProperty("time", i, out _);
                options.Data.CreateProperty("index", i, out _);
                options.Data.CreateProperty("first", i == 0, out _);
                options.Data.CreateProperty("last", i == (count - 1), out _);
            
                options.Template(output, context);
            }
        });
    }

    protected HandlebarsTemplate<object, object> CompileTemplate(string templateDir, string templateName)
    {
        var templatePath = FileSystem.Path.Combine(templateDir, templateName);
        var templateContent = FileSystem.File.ReadAllText(templatePath);
        return Handlebars.Compile(templateContent);
    }

    protected async Task<HandlebarsTemplate<object, object>> CompileTemplateAsync(string templateDir, string templateName)
    {
        var templatePath = FileSystem.Path.Combine(templateDir, templateName);
        var templateContent = await FileSystem.File.ReadAllTextAsync(templatePath);
        return await Task.Run(() => Handlebars.Compile(templateContent));
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

    protected async Task WriteGeneratedFileAsync(string dir, string fileName, string content)
    {
        var fullPath = FileSystem.Path.Combine(dir, fileName);
        
        if (Options.Verbose)
        {
            var relativePath = FileSystem.Path.GetRelativePath(Environment.CurrentDirectory, fullPath);
            Console.WriteLine($"{(Options.DryRun ? "[DRY-RUN] " : "")}Generated file: {relativePath}");
        }
        
        if (Options.DryRun) return;

        FileSystem.Directory.CreateDirectory(FileSystem.Path.GetDirectoryName(fullPath)!);
        await FileSystem.File.WriteAllTextAsync(fullPath, content);
    }

    public abstract Task GenerateAsync();
}