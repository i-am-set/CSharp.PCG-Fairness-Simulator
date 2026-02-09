using System;
using System.Collections.Generic;
using Core.Simulation;

namespace Core.Constraints
{
    public class ResourceStarvationFloor : IConstraint
    {
        private readonly int _safetyThreshold;

        public ResourceStarvationFloor(int safetyThreshold = 5)
        {
            _safetyThreshold = safetyThreshold;
        }

        public Encounter Apply(Encounter current, IReadOnlyList<Encounter> history, PlayerState state, SimulationConfig config)
        {
            if (state.Resources < _safetyThreshold)
            {
                int minRequiredReward = config.HealingCost;

                minRequiredReward = Math.Min(minRequiredReward, config.MaxReward);

                if (current.Reward < minRequiredReward)
                {
                    return new Encounter(current.Index, current.Difficulty, minRequiredReward)
                    {
                        OriginalDifficulty = current.OriginalDifficulty,
                        OriginalReward = current.OriginalReward
                    };
                }
            }

            return current;
        }
    }
}