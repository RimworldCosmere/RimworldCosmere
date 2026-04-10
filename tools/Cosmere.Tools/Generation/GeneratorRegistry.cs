using System.IO.Abstractions;
using System.Reflection;

namespace Cosmere.Tools.Generation;

public class GeneratorRegistry
{
    private readonly Dictionary<string, Type> _generators = new();
    private readonly GeneratorOptions _options;
    private readonly IFileSystem _fileSystem;

    public GeneratorRegistry(GeneratorOptions options, IFileSystem fileSystem)
    {
        _options = options;
        _fileSystem = fileSystem;
        DiscoverGenerators();
    }

    private void DiscoverGenerators()
    {
        var generatorTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => typeof(IGenerator).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface);

        foreach (var type in generatorTypes)
        {
            var name = type.Name.Replace("Generator", "");
            _generators[name] = type;
        }
    }

    public IGenerator? GetGenerator(string name)
    {
        if (!_generators.TryGetValue(name, out var type))
            return null;

        return (IGenerator)Activator.CreateInstance(type, _options, _fileSystem)!;
    }

    public IEnumerable<string> GetGeneratorNames()
    {
        return _generators.Keys;
    }

    public async Task RunAllAsync()
    {
        foreach (var generatorName in GetGeneratorNames())
        {
            var generator = GetGenerator(generatorName);
            if (generator != null)
            {
                Console.WriteLine($"Running {generatorName} generator...");
                await generator.GenerateAsync();
            }
        }
    }
}