using System;
using System.Collections.Generic;

namespace Core.Simulation
{
    public class ControlGenerator
    {
        public List<Encounter> GenerateSequence(int seed, SimulationConfig config)
        {
            Random rng = new Random(seed);
            var sequence = new List<Encounter>(config.RunLength);

            for (int i = 0; i < config.RunLength; i++)
            {
                int d = rng.Next(config.MinDifficulty, config.MaxDifficulty + 1);
                int r = rng.Next(config.MinReward, config.MaxReward + 1);

                sequence.Add(new Encounter(i, d, r));
            }

            return sequence;
        }
    }
}