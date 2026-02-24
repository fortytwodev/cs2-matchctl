using System.Text.Json;
using BasicFaceitServer.Core;
using BasicFaceitServer.Utils;

namespace BasicFaceitServer.Configs;

public static class ConfigLoader
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true
    };

    public static PluginConfig Load(string moduleDirectory)
    {
        var pluginConfigPath = Path.Combine(moduleDirectory, PluginConstants.ConfigFileName);
        try
        {
            if (!File.Exists(pluginConfigPath))
            {
                MyLogger.Debug("Configs file does not exists, saving defaults.");

                var defaultConfig = new PluginConfig();
                Save(pluginConfigPath, defaultConfig);

                return defaultConfig;
            }

            var json = File.ReadAllText(pluginConfigPath);
            var config = JsonSerializer.Deserialize<PluginConfig>(json);
            if (config == null)
            {
                MyLogger.Error("Config is null. Failed to parse config, using defaults.");
                return new PluginConfig();
            }

            MyLogger.Debug("Plugin config loaded successfully!");
            return config;
        }
        catch (Exception ex)
        {
            MyLogger.Error($"Failed to parse config, using defaults: {ex.Message}");
            return new PluginConfig();
        }
    }

    private static void Save(string configPath, PluginConfig config)
    {
        try
        {
            var json = JsonSerializer.Serialize(config, SerializerOptions);
            File.WriteAllText(configPath, json);
            MyLogger.Debug("Config saved successfully!");
        }
        catch (Exception ex)
        {
            MyLogger.Error($"Exception reading config: {ex.Message}");
        }
    }
}