using System;
using System.IO;
using Core.Simulation;

namespace Core.Infrastructure
{
    public static class ConfigManager
    {
        private static readonly string ConfigDirectory = "Config";

        static ConfigManager()
        {
            if (!Directory.Exists(ConfigDirectory))
            {
                Directory.CreateDirectory(ConfigDirectory);
            }
        }

        public static void SavePreset(SimulationConfig config, string name)
        {
            try
            {
                string json = config.ToJson();
                string path = Path.Combine(ConfigDirectory, $"{name}.json");
                File.WriteAllText(path, json);
                Logger.Log($"Configuration saved to {path}");
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to save config '{name}'", ex);
            }
        }

        public static SimulationConfig LoadPreset(string name)
        {
            try
            {
                string path = Path.Combine(ConfigDirectory, $"{name}.json");
                if (!File.Exists(path))
                {
                    Logger.Log($"Config file not found: {path}. Using defaults.");
                    return new SimulationConfig();
                }

                string json = File.ReadAllText(path);
                var config = SimulationConfig.FromJson(json);
                config.Validate();
                Logger.Log($"Configuration loaded from {path}");
                return config;
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to load config '{name}'", ex);
                return new SimulationConfig();
            }
        }
    }
}