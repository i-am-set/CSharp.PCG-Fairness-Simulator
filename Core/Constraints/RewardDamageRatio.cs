using System;
using System.Collections.Generic;
using Core.Simulation;

namespace Core.Constraints
{
    public class RewardDamageRatio : IConstraint
    {
        private readonly double _minRatio;

        public RewardDamageRatio(double minRatio = 0.5)
        {
            _minRatio = minRatio;
        }

        public Encounter Apply(Encounter current, IReadOnlyList<Encounter> history, PlayerState state, SimulationConfig config)
        {
            double potentialDamage = current.Difficulty * config.DamageMultiplier;

            if (potentialDamage < 1.0)
                return current;

            double currentRatio = current.Reward / potentialDamage;

            if (currentRatio < _minRatio)
            {
                int targetReward = (int)Math.Ceiling(potentialDamage * _minRatio);
                targetReward = Math.Min(targetReward, config.MaxReward);

                if (targetReward > current.Reward)
                {
                    return new Encounter(current.Index, current.Difficulty, targetReward)
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