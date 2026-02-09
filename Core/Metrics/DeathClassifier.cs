using Core.Simulation;
using System.Linq;

namespace Core.Metrics
{
    public static class DeathClassifier
    {
        public static DeathClassification Classify(SimulationResult result, SimulationConfig config)
        {
            if (result.Survived) return DeathClassification.None;

            if (result.Sequence.Count == 0) return DeathClassification.None;
            Encounter fatalEncounter = result.Sequence.Last();

            int finalDamage = (int)(fatalEncounter.Difficulty * config.DamageMultiplier);

            if (finalDamage >= (config.MaxHP * 0.5))
            {
                return DeathClassification.Spike;
            }

            if (result.FinalResources < (config.HealingCost * 0.5))
            {
                return DeathClassification.Starvation;
            }

            return DeathClassification.Attrition;
        }
    }
}