using System.IO;
using System.Text.Json;
using AnsemBullOverlay.Models;

namespace AnsemBullOverlay.Services;

/// <summary>Loads/saves <see cref="AppConfig"/> to %AppData%.</summary>
public sealed class ConfigService
{
    private static readonly string ConfigDir =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AnsemBullOverlay");

    private static readonly string ConfigPath = Path.Combine(ConfigDir, "config.json");

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public AppConfig Current { get; private set; } = new();

    public AppConfig Load()
    {
        try
        {
            if (File.Exists(ConfigPath))
            {
                var json = File.ReadAllText(ConfigPath);
                Current = JsonSerializer.Deserialize<AppConfig>(json, JsonOpts) ?? new AppConfig();
            }
        }
        catch
        {
            Current = new AppConfig();
        }
        return Current;
    }

    public void Save(AppConfig config)
    {
        try
        {
            Directory.CreateDirectory(ConfigDir);
            File.WriteAllText(ConfigPath, JsonSerializer.Serialize(config, JsonOpts));
            Current = config;
        }
        catch { /* non-fatal */ }
    }
}
