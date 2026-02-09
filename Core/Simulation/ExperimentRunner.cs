using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Core.Constraints;
using Core.Metrics;

namespace Core.Simulation
{
    public class SimulationRunner
    {
        public event Action<double> OnProgressChanged;
        public event Action<string> OnStatusChanged;

        public async Task<Dictionary<string, List<RunMetric>>> RunSimulationAsync(
            SimulationConfig config,
            CancellationToken token)
        {
            var results = new Dictionary<string, List<RunMetric>>();

            Random masterRng = new Random(config.MasterSeed);
            List<int> seeds = new List<int>(config.RunCount);
            for (int i = 0; i < config.RunCount; i++)
            {
                seeds.Add(masterRng.Next());
            }

            var groups = new Dictionary<string, List<IConstraint>>
            {
                { "Group A (Control)", new List<IConstraint>() },
                { "Group B (Spike Limiter)", new List<IConstraint> { new DifficultySpikeLimiter() } },
                { "Group C (Resource Floor)", new List<IConstraint> { new ResourceStarvationFloor(config.HealingCost) } },
                { "Group D (Rolling Target)", new List<IConstraint> { new RollingDifficultyTarget() } },
                { "Group F (Reward Ratio)", new List<IConstraint> { new RewardDamageRatio(0.5) } },
                { "Group E (Combined)", new List<IConstraint>
                    {
                        new DifficultySpikeLimiter(),
                        new RollingDifficultyTarget(),
                        new LethalEncounterGuard(),
                        new ResourceStarvationFloor(config.HealingCost),
                        new RewardDamageRatio(0.5)
                    }
                }
            };

            int totalRuns = groups.Count * config.RunCount;
            int completedRuns = 0;

            foreach (var group in groups)
            {
                string groupName = group.Key;
                List<IConstraint> constraints = group.Value;
                var groupMetrics = new List<RunMetric>(config.RunCount);

                OnStatusChanged?.Invoke($"Running {groupName}...");

                var groupConfig = CloneConfig(config);
                groupConfig.EnableConstraints = constraints.Count > 0;

                var simulator = new Simulator(groupConfig, constraints);

                await Task.Run(() =>
                {
                    for (int i = 0; i < seeds.Count; i++)
                    {
                        if (token.IsCancellationRequested) break;

                        int seed = seeds[i];
                        SimulationResult result = simulator.Run(seed);

                        groupMetrics.Add(RunMetric.FromResult(result, groupConfig, groupName, i));

                        Interlocked.Increment(ref completedRuns);

                        if (completedRuns % 100 == 0)
                        {
                            OnProgressChanged?.Invoke((double)completedRuns / totalRuns);
                        }
                    }
                }, token);

                if (token.IsCancellationRequested) break;

                results.Add(groupName, groupMetrics);
            }

            OnStatusChanged?.Invoke("Simulation Complete.");
            OnProgressChanged?.Invoke(1.0);

            return results;
        }

        private SimulationConfig CloneConfig(SimulationConfig original)
        {
            string json = original.ToJson();
            return SimulationConfig.FromJson(json);
        }
    }
}