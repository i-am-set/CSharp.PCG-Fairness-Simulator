using System;
using System.Collections.Generic;
using Core.Simulation;

namespace Core.Constraints
{
    public class LethalEncounterGuard : IConstraint
    {
        public Encounter Apply(Encounter current, IReadOnlyList<Encounter> history, PlayerState state, SimulationConfig config)
        {
            int potentialDamage = (int)(current.Difficulty * config.DamageMultiplier);

            if (potentialDamage >= state.CurrentHP)
            {
                double healthPercent = (double)state.CurrentHP / config.MaxHP;
                bool isLowHealth = healthPercent <= 0.30;

                bool isMassiveDamage = potentialDamage > (config.MaxHP * 0.5);

                if (!isLowHealth || isMassiveDamage)
                {
                    int maxSafeDamage = Math.Max(0, state.CurrentHP - 1);

                    int maxSafeDifficulty = (int)(maxSafeDamage / config.DamageMultiplier);

                    maxSafeDifficulty = Math.Max(maxSafeDifficulty, config.MinDifficulty);

                    if (current.Difficulty > maxSafeDifficulty)
                    {
                        return new Encounter(current.Index, maxSafeDifficulty, current.Reward)
                        {
                            OriginalDifficulty = current.OriginalDifficulty,
                            OriginalReward = current.OriginalReward
                        };
                    }
                }
            }

            return current;
        }
    }
}