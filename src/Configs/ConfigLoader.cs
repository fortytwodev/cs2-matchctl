using System.Text.Json;
using BasicFaceitServer.Core;
using BasicFaceitServer.Infrastructure;

namespace BasicFaceitServer.Configs;

public static class ConfigLoader
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true
    };

    public static PluginConfig Load(string moduleDirectory)
    {
        var pluginConfigPath = Path.Combine(moduleDirectory, Constants.ConfigFileName);
        try
        {
            if (!File.Exists(pluginConfigPath))
            {
                PluginLogger.Debug("Configs file does not exists, saving defaults.");

                var defaultConfig = new PluginConfig();
                Save(pluginConfigPath, defaultConfig);

                return defaultConfig;
            }

            var json = File.ReadAllText(pluginConfigPath);
            var config = JsonSerializer.Deserialize<PluginConfig>(json);
            if (config == null)
            {
                PluginLogger.Error("Config is null. Failed to parse config, using defaults.");
                return new PluginConfig();
            }

            PluginLogger.Debug("Plugin config loaded successfully!");
            return config;
        }
        catch (Exception ex)
        {
            PluginLogger.Error($"Failed to parse config, using defaults: {ex.Message}");
            return new PluginConfig();
        }
    }

    private static void Save(string configPath, PluginConfig config)
    {
        try
        {
            var json = JsonSerializer.Serialize(config, SerializerOptions);
            File.WriteAllText(configPath, json);
            PluginLogger.Debug("Config saved successfully!");
        }
        catch (Exception ex)
        {
            PluginLogger.Error($"Exception reading config: {ex.Message}");
        }
    }
}
