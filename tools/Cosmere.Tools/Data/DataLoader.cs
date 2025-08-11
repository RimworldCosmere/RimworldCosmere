using System.IO.Abstractions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Cosmere.Tools.Data;

public class DataLoader
{
    private readonly IFileSystem _fileSystem;
    private readonly JsonSerializerSettings _settings;

    public DataLoader(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
        _settings = new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            },
            NullValueHandling = NullValueHandling.Ignore
        };
    }

    public async Task<List<T>> LoadAllAsync<T>(string dataType)
    {
        var dataDir = _fileSystem.Path.Combine(Environment.CurrentDirectory, ".scripts", "Data", dataType);
        
        if (!_fileSystem.Directory.Exists(dataDir))
            return new List<T>();

        var jsonFiles = _fileSystem.Directory.GetFiles(dataDir, "*.json");
        var results = new List<T>();

        foreach (var file in jsonFiles)
        {
            var content = await _fileSystem.File.ReadAllTextAsync(file);
            var item = JsonConvert.DeserializeObject<T>(content, _settings);
            if (item != null)
            {
                results.Add(item);
            }
        }

        return results;
    }

    public async Task<T?> LoadAsync<T>(string dataType, string fileName)
    {
        var filePath = _fileSystem.Path.Combine(Environment.CurrentDirectory, ".scripts", "Data", dataType, $"{fileName}.json");
        
        if (!_fileSystem.File.Exists(filePath))
            return default;

        var content = await _fileSystem.File.ReadAllTextAsync(filePath);
        return JsonConvert.DeserializeObject<T>(content, _settings);
    }
}