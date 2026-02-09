using System.Collections.Generic;
using System.Linq;
using Core.Simulation;

namespace Core.Constraints
{
    public class DifficultySpikeLimiter : IConstraint
    {
        private readonly int _maxIncrease;

        public DifficultySpikeLimiter(int maxIncrease = 5)
        {
            _maxIncrease = maxIncrease;
        }

        public Encounter Apply(Encounter current, IReadOnlyList<Encounter> history, PlayerState state, SimulationConfig config)
        {
            if (history == null || history.Count == 0)
                return current;

            Encounter previous = history.Last();
            int maxAllowed = previous.Difficulty + _maxIncrease;

            if (current.Difficulty > maxAllowed)
            {
                return new Encounter(current.Index, maxAllowed, current.Reward)
                {
                    OriginalDifficulty = current.OriginalDifficulty,
                    OriginalReward = current.OriginalReward
                };
            }

            return current;
        }
    }
}