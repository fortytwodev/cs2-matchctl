using System.Text.Json;
using BasicFaceitServer.Utils;

namespace BasicFaceitServer.Configs;

public class ConfigManager
{
    private const string ConfigPath = "configs.json";

    private static MyConfigs Config { get; set; } = new();

    public MyConfigs GetConfig(string moduleDirectory)
    {
        var cfgFullPath = Path.Combine(moduleDirectory, ConfigPath);
        try
        {
            if (!File.Exists(cfgFullPath))
            {
                MyLogger.Debug("Configs file does not exists, saving defaults.");

                Config = new MyConfigs();
                File.WriteAllText(cfgFullPath, JsonSerializer.Serialize(
                    Config,
                    new JsonSerializerOptions { WriteIndented = true }
                ));

                return new MyConfigs();
            }

            var json = File.ReadAllText(cfgFullPath);
            var tmpConfig = JsonSerializer.Deserialize<MyConfigs>(json);
            if (tmpConfig == null)
            {
                MyLogger.Debug("Failed to parse config, using defaults.");
                return new MyConfigs();
            }

            MyLogger.Debug("Processed the config file, now using it");
            return tmpConfig;
        }
        catch (Exception ex)
        {
            MyLogger.Error($"Exception reading config: {ex.Message}");
            MyLogger.Debug("Using defaults...");
            return new MyConfigs();
        }
    }
}