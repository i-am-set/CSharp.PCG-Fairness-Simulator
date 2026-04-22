using System;
using System.Collections.Generic;
using Core.Constraints;

namespace Core.Simulation
{
    public class Simulator
    {
        private readonly SimulationConfig _config;
        private readonly List<IConstraint> _constraints;
        private readonly ControlGenerator _generator;

        public Simulator(SimulationConfig config, List<IConstraint> constraints)
        {
            _config = config;
            _constraints = constraints ?? new List<IConstraint>();
            _generator = new ControlGenerator();
        }

        public SimulationResult Run(int seed)
        {
            List<Encounter> rawSequence = _generator.GenerateSequence(seed, _config);

            List<Encounter> finalSequence = _config.EnableConstraints ? ProjectSequence(rawSequence) : rawSequence;

            return SimulateExecution(seed, finalSequence);
        }

        private List<Encounter> ProjectSequence(List<Encounter> rawSequence)
        {
            var projectedSequence = new List<Encounter>(_config.RunLength);
            PlayerState simState = PlayerState.Initial(_config);

            foreach (var rawEncounter in rawSequence)
            {
                Encounter current = rawEncounter;

                foreach (var constraint in _constraints)
                {
                    current = constraint.Apply(current, projectedSequence, simState, _config);
                }

                projectedSequence.Add(current);

                simState = simState.ApplyDamage(current.Difficulty, _config.DamageMultiplier);
                if (simState.IsAlive)
                {
                    simState = simState.AddReward(current.Reward);
                    simState = simState.PerformGreedyHealing(_config.HealingCost, _config.HealingAmount);
                }
            }

            return projectedSequence;
        }

        private SimulationResult SimulateExecution(int seed, List<Encounter> sequence)
        {
            PlayerState state = PlayerState.Initial(_config);
            int stepsSurvived = 0;
            var history = new List<PlayerState>(sequence.Count);

            foreach (var encounter in sequence)
            {
                int damage = (int)(encounter.Difficulty * _config.DamageMultiplier);

                if (damage >= state.CurrentHP)
                {
                    state = state.ApplyDamage(encounter.Difficulty, _config.DamageMultiplier);
                    history.Add(state);
                    break;
                }

                state = state.ApplyDamage(encounter.Difficulty, _config.DamageMultiplier);

                state = state.AddReward(encounter.Reward);

                state = state.PerformGreedyHealing(_config.HealingCost, _config.HealingAmount);

                history.Add(state);
                stepsSurvived++;
            }

            return new SimulationResult
            {
                Seed = seed,
                Survived = state.IsAlive,
                FinalHP = state.CurrentHP,
                FinalResources = state.Resources,
                StepsSurvived = stepsSurvived,
                DeathCause = DeathClassification.None,
                Sequence = sequence,
                History = history
            };
        }
    }
}