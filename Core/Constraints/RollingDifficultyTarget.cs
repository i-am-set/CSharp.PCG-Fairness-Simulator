using System;
using System.Collections.Generic;
using System.Linq;
using Core.Simulation;

namespace Core.Constraints
{
    public class RollingDifficultyTarget : IConstraint
    {
        private readonly int _windowSize;
        private readonly double _targetAverage;
        private readonly double _tolerance;

        public RollingDifficultyTarget(int windowSize = 4, double targetAverage = 6.5, double tolerance = 1.0)
        {
            _windowSize = windowSize;
            _targetAverage = targetAverage;
            _tolerance = tolerance;
        }

        public Encounter Apply(Encounter current, IReadOnlyList<Encounter> history, PlayerState state, SimulationConfig config)
        {
            if (history == null || history.Count == 0)
                return current;

            var window = history.TakeLast(_windowSize - 1).ToList();
            int currentSum = window.Sum(e => e.Difficulty);
            int count = window.Count + 1;

            double projectedAverage = (currentSum + current.Difficulty) / (double)count;

            if (projectedAverage > (_targetAverage + _tolerance))
            {
                double maxAllowedAvg = _targetAverage + _tolerance;
                int maxDifficulty = (int)(maxAllowedAvg * count) - currentSum;

                maxDifficulty = Math.Max(maxDifficulty, config.MinDifficulty);

                if (current.Difficulty > maxDifficulty)
                {
                    return new Encounter(current.Index, maxDifficulty, current.Reward)
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