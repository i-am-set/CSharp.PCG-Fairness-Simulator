using System;
using System.Text.Json;

namespace Core.Simulation
{
    public class SimulationConfig
    {
        // Config
        public int RunCount { get; set; } = 10000;
        public int MasterSeed { get; set; } = 12345;

        public int RunLength { get; set; } = 20;
        public int InitialHP { get; set; } = 100;
        public int MaxHP { get; set; } = 100;

        public int InitialResources { get; set; } = 10; 
        public int MaxResource { get; set; } = 100;

        public int HealingCost { get; set; } = 10;
        public int HealingAmount { get; set; } = 35; 

        public double DamageMultiplier { get; set; } = 6.5;

        public int MinDifficulty { get; set; } = 1;
        public int MaxDifficulty { get; set; } = 10;
        public int MinReward { get; set; } = 5;
        public int MaxReward { get; set; } = 15;

        // constraint toggles
        public bool EnableConstraints { get; set; } = false;
        public bool UseDifficultySpikeLimiter { get; set; } = false;
        public bool UseRollingDifficultyTarget { get; set; } = false;
        public bool UseLethalEncounterGuard { get; set; } = false;
        public bool UseResourceStarvationFloor { get; set; } = false;
        public bool UseRewardDamageRatio { get; set; } = false;

        public void Validate()
        {
            if (RunCount <= 0) throw new ArgumentException("RunCount must be greater than 0.");
            if (RunLength <= 0) throw new ArgumentException("RunLength must be greater than 0.");
            if (MinDifficulty > MaxDifficulty) throw new ArgumentException("MinDifficulty cannot be greater than MaxDifficulty.");
            if (MinReward > MaxReward) throw new ArgumentException("MinReward cannot be greater than MaxReward.");
            if (InitialHP <= 0) throw new ArgumentException("InitialHP must be positive.");
            if (MaxHP < InitialHP) throw new ArgumentException("MaxHP cannot be less than InitialHP.");
            if (HealingCost < 0) throw new ArgumentException("HealingCost cannot be negative.");
            if (HealingAmount <= 0) throw new ArgumentException("HealingAmount must be positive.");
            if (DamageMultiplier < 0) throw new ArgumentException("DamageMultiplier cannot be negative.");
            if (MaxResource <= 0) throw new ArgumentException("MaxResource must be positive.");
        }

        public string ToJson()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            return JsonSerializer.Serialize(this, options);
        }

        public static SimulationConfig FromJson(string json)
        {
            return JsonSerializer.Deserialize<SimulationConfig>(json) ?? new SimulationConfig();
        }
    }
}