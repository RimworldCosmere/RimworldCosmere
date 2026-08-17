using System.IO.Abstractions;
using Cosmere.Tools.Extensions;
using Cosmere.Tools.Models;
using HandlebarsDotNet;

namespace Cosmere.Tools.Generation;

public abstract class BaseGenerator : IGenerator {
    // A stat floored at a thousandth is off; a capacity floored there is a pawn who
    // cannot see, hear or stand. Tin storing all the way down should cost a colonist
    // their senses, not their ability to be a colonist.
    private const double MinimumCapacityFactor = 0.15;

    // What a compounded burn pays over an ordinary tap.
    private const double FactorAmplification = 10.0;
    private const double OffsetAmplification = 3.0;

    /// <summary>Where a ladder sits at one rung, given the peak its top rung reaches.</summary>
    /// <remarks>
    ///     Geometric, so the storing ladder is the tapping one inverted and the pair multiplies
    ///     back to 1 at every rung. It also never reaches zero, which is what lets a peak of ten
    ///     exist at all - the linear form would need the storing side to pass through it.
    /// </remarks>
    private static double FactorForStage(double peak, int stage, int stages) {
        if (peak <= 0 || stages <= 0) return 1;

        return Math.Pow(peak, (stage + 1) / (double)stages);
    }

    protected readonly GeneratorOptions Options;
    protected readonly IFileSystem FileSystem;
    protected readonly IHandlebars Handlebars;

    protected BaseGenerator(GeneratorOptions options, IFileSystem fileSystem) {
        Options = options;
        FileSystem = fileSystem;
        Handlebars = HandlebarsDotNet.Handlebars.Create();
        RegisterHelpers();
    }

    protected virtual void RegisterHelpers() {
        // String transformation helpers
        Handlebars.RegisterHelper("toDefName", (writer, context, parameters) => {
            if (parameters[0] is string str) {
                writer.WriteSafeString(str.ToDefName());
            }
        });

        Handlebars.RegisterHelper("defName", (writer, context, parameters) => {
            if (parameters[0] is string str) {
                writer.WriteSafeString(str.ToDefName());
            }
        });

        Handlebars.RegisterHelper("upperFirst", (writer, context, parameters) => {
            if (parameters[0] is string str && !string.IsNullOrEmpty(str)) {
                writer.WriteSafeString(char.ToUpper(str[0]) + str[1..]);
            }
        });

        Handlebars.RegisterHelper("capitalize", (writer, context, parameters) => {
            if (parameters.Length > 0 && parameters[0] != null) {
                var str = parameters[0].ToString();
                if (!string.IsNullOrEmpty(str)) {
                    writer.WriteSafeString(char.ToUpper(str[0]) + str[1..]);
                }
            }
        });

        Handlebars.RegisterHelper("lower", (writer, context, parameters) => {
            if (parameters[0] is string str) {
                writer.WriteSafeString(str.ToLowerInvariant());
            }
        });

        Handlebars.RegisterHelper("title", (writer, context, parameters) => {
            if (parameters[0] is string str) {
                writer.WriteSafeString(str.ToTitleCase());
            }
        });

        // Collection helpers
        Handlebars.RegisterHelper("join", (writer, context, parameters) => {
            if (parameters.Length >= 2 && parameters[0] is IEnumerable<object> collection && parameters[1] is string separator) {
                var joined = string.Join(separator, collection.Select(x => x?.ToString() ?? string.Empty));
                writer.WriteSafeString(joined);
            }
        });

        Handlebars.RegisterHelper("count", (writer, context, parameters) => {
            if (parameters[0] is IEnumerable<object> collection) {
                writer.WriteSafeString(collection.Count().ToString());
            }
        });

        // Logic helpers
        Handlebars.RegisterHelper("eq", (writer, context, parameters) => {
            if (parameters.Length >= 2) {
                writer.WriteSafeString((parameters[0]?.Equals(parameters[1]) == true).ToString().ToLower());
            }
        });

        Handlebars.RegisterHelper("ne", (writer, context, parameters) => {
            if (parameters.Length >= 2) {
                writer.WriteSafeString((parameters[0]?.Equals(parameters[1]) != true).ToString().ToLower());
            }
        });

        Handlebars.RegisterHelper("lt", (writer, context, parameters) => {
            if (parameters.Length >= 2 &&
                double.TryParse(parameters[0]?.ToString(), out var v1) &&
                double.TryParse(parameters[1]?.ToString(), out var v2)) {
                writer.WriteSafeString((v1 < v2).ToString().ToLower());
            }
        });

        Handlebars.RegisterHelper("gt", (writer, context, parameters) => {
            if (parameters.Length >= 2 &&
                double.TryParse(parameters[0]?.ToString(), out var v1) &&
                double.TryParse(parameters[1]?.ToString(), out var v2)) {
                writer.WriteSafeString((v1 > v2).ToString().ToLower());
            }
        });

        Handlebars.RegisterHelper("lte", (writer, context, parameters) => {
            if (parameters.Length >= 2 &&
                double.TryParse(parameters[0]?.ToString(), out var v1) &&
                double.TryParse(parameters[1]?.ToString(), out var v2)) {
                writer.WriteSafeString((v1 <= v2).ToString().ToLower());
            }
        });

        Handlebars.RegisterHelper("gte", (writer, context, parameters) => {
            if (parameters.Length >= 2 &&
                double.TryParse(parameters[0]?.ToString(), out var v1) &&
                double.TryParse(parameters[1]?.ToString(), out var v2)) {
                writer.WriteSafeString((v1 >= v2).ToString().ToLower());
            }
        });

        Handlebars.RegisterHelper("and", (writer, context, parameters) => {
            var result = parameters.All(p => p != null &&
                (p is bool b ? b : !string.IsNullOrEmpty(p.ToString())));
            writer.WriteSafeString(result.ToString().ToLower());
        });

        Handlebars.RegisterHelper("or", (writer, context, parameters) => {
            var result = parameters.Any(p => p != null &&
                (p is bool b ? b : !string.IsNullOrEmpty(p.ToString())));
            writer.WriteSafeString(result.ToString().ToLower());
        });

        Handlebars.RegisterHelper("isdefined", (writer, context, parameters) => {
            writer.WriteSafeString((parameters.Length > 0 && parameters[0] != null).ToString().ToLower());
        });

        // Math helpers
        Handlebars.RegisterHelper("multiply", (writer, context, parameters) => {
            if (parameters.Length >= 2 &&
                double.TryParse(parameters[0]?.ToString(), out var v1) &&
                double.TryParse(parameters[1]?.ToString(), out var v2)) {
                writer.WriteSafeString((v1 * v2).ToString("F6"));
            }
        });

        Handlebars.RegisterHelper("add", (writer, context, parameters) => {
            if (parameters.Length >= 2 &&
                double.TryParse(parameters[0]?.ToString(), out var v1) &&
                double.TryParse(parameters[1]?.ToString(), out var v2)) {
                writer.WriteSafeString((v1 + v2).ToString());
            }
        });

        // Offsets sit on zero, not one. Sharing the factor helper meant brass shipped
        // a stored ComfyTemperatureMin of +101 instead of +100 - and every stage below
        // it was off by the same one.
        Handlebars.RegisterHelper("getOffsetForStage", (writer, context, parameters) => {
            if (parameters.Length >= 2 &&
                int.TryParse(parameters[0]?.ToString(), out var stage) &&
                double.TryParse(parameters[1]?.ToString(), out var step)) {
                var result = step * (stage + 1);

                // Format with up to 8 decimal places, then trim trailing zeros
                var formatted = result.ToString("F8").TrimEnd('0').TrimEnd('.');
                writer.WriteSafeString(formatted);
            }
        });

        // A factor ladder is geometric, not linear. The metal names the peak its top rung
        // reaches and each rung takes an even fraction of the way there, so tapping to x10
        // pairs with storing to exactly 1/10 - the two sides multiply back to 1 at every
        // rung, which is what conservation means for a multiplier. A linear ladder cannot
        // reach a peak like that at all: the storing side would need to pass through zero.
        Handlebars.RegisterHelper("getFactorForStage", (writer, context, parameters) => {
            if (parameters.Length >= 3 &&
                int.TryParse(parameters[0]?.ToString(), out var stage) &&
                double.TryParse(parameters[1]?.ToString(), out var peak) &&
                int.TryParse(parameters[2]?.ToString(), out var stages)) {
                var result = FactorForStage(peak, stage, stages);

                var formatted = result.ToString("F8").TrimEnd('0').TrimEnd('.');
                writer.WriteSafeString(formatted);
            }
        });

        Handlebars.RegisterHelper("getCapacityFactorForStage", (writer, context, parameters) => {
            if (parameters.Length >= 3 &&
                int.TryParse(parameters[0]?.ToString(), out var stage) &&
                double.TryParse(parameters[1]?.ToString(), out var peak) &&
                int.TryParse(parameters[2]?.ToString(), out var stages)) {
                var result = Math.Max(FactorForStage(peak, stage, stages), MinimumCapacityFactor);

                var formatted = result.ToString("F8").TrimEnd('0').TrimEnd('.');
                writer.WriteSafeString(formatted);
            }
        });

        // Compounded charge pays out ten times harder. Reading that off the factor itself
        // drives "lower is better" stats negative - Gold's IncomingDamageFactor would reach
        // damage that heals - so amplify the benefit the factor represents and invert back.
        Handlebars.RegisterHelper("getCompoundedStatForStage", (writer, context, parameters) => {
            if (parameters.Length >= 3 &&
                int.TryParse(parameters[0]?.ToString(), out var stage) &&
                double.TryParse(parameters[1]?.ToString(), out var peak) &&
                int.TryParse(parameters[2]?.ToString(), out var stages)) {
                var ordinary = FactorForStage(peak, stage, stages);

                var result = ordinary >= 1
                    ? 1 + (ordinary - 1) * FactorAmplification
                    : 1.0 / (1.0 + FactorAmplification * (1.0 / ordinary - 1.0));

                var formatted = result.ToString("F8").TrimEnd('0').TrimEnd('.');
                writer.WriteSafeString(formatted);
            }
        });

        // Offsets have no reciprocal reading, so the tenfold a factor gets would run
        // straight off the end of any real scale - brass reached a comfort band 300
        // degrees below zero. Three is what an absolute number can carry.
        Handlebars.RegisterHelper("getCompoundedOffsetForStage", (writer, context, parameters) => {
            if (parameters.Length >= 2 &&
                int.TryParse(parameters[0]?.ToString(), out var stage) &&
                double.TryParse(parameters[1]?.ToString(), out var step)) {
                var result = step * OffsetAmplification * (stage + 1);
                var formatted = result.ToString("F8").TrimEnd('0').TrimEnd('.');
                writer.WriteSafeString(formatted);
            }
        });

        // Color helpers
        Handlebars.RegisterHelper("rgb", (writer, context, parameters) => {
            if (parameters[0] is ColorInfo color) {
                writer.WriteSafeString($"({color.R}, {color.G}, {color.B})");
            }
        });

        Handlebars.RegisterHelper("rgba", (writer, context, parameters) => {
            if (parameters.Length >= 2 && parameters[0] is ColorInfo color &&
                double.TryParse(parameters[1]?.ToString(), out var alpha)) {
                writer.WriteSafeString($"({color.R}, {color.G}, {color.B}, {alpha})");
            }
        });

        // Range helper
        Handlebars.RegisterHelper("range", (writer, context, parameters) => {
            if (parameters[0] is int[] range && range.Length >= 2) {
                writer.WriteSafeString($"{range[0]}~{range[1]}");
            }
        });

        // Fallback helper
        Handlebars.RegisterHelper("fallback", (writer, context, parameters) => {
            if (parameters.Length >= 2) {
                var value = parameters[0];
                var fallback = parameters[1];
                writer.WriteSafeString((value ?? fallback)?.ToString() ?? string.Empty);
            }
        });

        // MayRequire and WithoutRequire helpers
        Handlebars.RegisterHelper("mayRequire", (writer, context, parameters) => {
            if (parameters.Length > 0 && parameters[0] is string value) {
                if (value.Contains(':')) {
                    var parts = value.Split(':');
                    writer.WriteSafeString($" MayRequire=\"{parts[1]}\"");
                }
            }
        });

        Handlebars.RegisterHelper("withoutRequire", (writer, context, parameters) => {
            if (parameters.Length > 0 && parameters[0] is string value) {
                if (value.Contains(':')) {
                    writer.WriteSafeString(value.Split(':')[0]);
                } else {
                    writer.WriteSafeString(value);
                }
            }
        });

        // Block helpers
        Handlebars.RegisterHelper("times", (output, options, context, arguments) => {
            int count = Convert.ToInt32(arguments[0]);

            for (int i = 0; i < count; i++) {
                options.Data.CreateProperty("key", i, out _);
                options.Data.CreateProperty("time", i, out _);
                options.Data.CreateProperty("index", i, out _);
                options.Data.CreateProperty("first", i == 0, out _);
                options.Data.CreateProperty("last", i == (count - 1), out _);

                options.Template(output, context);
            }
        });
    }

    protected HandlebarsTemplate<object, object> CompileTemplate(string templateDir, string templateName) {
        var templatePath = FileSystem.Path.Combine(templateDir, templateName);
        var templateContent = FileSystem.File.ReadAllText(templatePath);
        return Handlebars.Compile(templateContent);
    }

    protected async Task<HandlebarsTemplate<object, object>> CompileTemplateAsync(string templateDir, string templateName) {
        var templatePath = FileSystem.Path.Combine(templateDir, templateName);
        var templateContent = await FileSystem.File.ReadAllTextAsync(templatePath);
        return await Task.Run(() => Handlebars.Compile(templateContent));
    }

    protected void WriteGeneratedFile(string dir, string fileName, string content) {
        var fullPath = FileSystem.Path.Combine(dir, fileName);

        if (Options.Verbose) {
            var relativePath = FileSystem.Path.GetRelativePath(Environment.CurrentDirectory, fullPath);
            Console.WriteLine($"{(Options.DryRun ? "[DRY-RUN] " : string.Empty)}Generated file: {relativePath}");
        }

        if (Options.DryRun) return;

        FileSystem.Directory.CreateDirectory(FileSystem.Path.GetDirectoryName(fullPath)!);
        FileSystem.File.WriteAllText(fullPath, content);
    }

    protected async Task WriteGeneratedFileAsync(string dir, string fileName, string content) {
        var fullPath = FileSystem.Path.Combine(dir, fileName);

        if (Options.Verbose) {
            var relativePath = FileSystem.Path.GetRelativePath(Environment.CurrentDirectory, fullPath);
            Console.WriteLine($"{(Options.DryRun ? "[DRY-RUN] " : string.Empty)}Generated file: {relativePath}");
        }

        if (Options.DryRun) return;

        FileSystem.Directory.CreateDirectory(FileSystem.Path.GetDirectoryName(fullPath)!);
        await FileSystem.File.WriteAllTextAsync(fullPath, content);
    }

    public abstract Task GenerateAsync();
}
