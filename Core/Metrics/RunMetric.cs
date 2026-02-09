using Core.Simulation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.Metrics
{
    public class RunMetric
    {
        public int RunID { get; set; }
        public int Seed { get; set; }
        public string ConstraintMode { get; set; }
        public bool Survived { get; set; }
        public int SurvivalLength { get; set; }
        public DeathClassification DeathCause { get; set; }
        public int FinalHP { get; set; }
        public int FinalResources { get; set; }

        public double EntropyScore { get; set; }
        public double DeltaVariance { get; set; }
        public double AverageDifficulty { get; set; }

        public int CorrectionCount { get; set; }
        public double CorrectionsPerEncounter { get; set; }
        public int MaxCorrectionStreak { get; set; }

        public int NearDeath_EncounterCount { get; set; }
        public int NearDeath_TotalEncounters { get; set; }
        public int NearDeath_Entered { get; set; }

        public double HealthDelta_Mean { get; set; }
        public double HealthDelta_StdDev { get; set; }

        public double AvgResourceConsumedPct { get; set; }
        public int MinResourceRemaining { get; set; }

        public double FPI_Avg { get; set; }
        public double FPI_Max { get; set; }

        public double RecoveryLag_Avg { get; set; }
        public int RecoveryLag_FailedCount { get; set; }

        public static RunMetric FromResult(SimulationResult result, SimulationConfig config, string constraintMode, int runId)
        {
            var cause = result.Survived
                ? DeathClassification.None
                : DeathClassifier.Classify(result, config);

            var metric = new RunMetric
            {
                RunID = runId,
                Seed = result.Seed,
                ConstraintMode = constraintMode,
                Survived = result.Survived,
                SurvivalLength = result.StepsSurvived,
                DeathCause = cause,
                FinalHP = result.FinalHP,
                FinalResources = result.FinalResources,
                EntropyScore = EntropyCalculator.CalculateShannonEntropy(result.Sequence),
                DeltaVariance = EntropyCalculator.CalculateDeltaVariance(result.Sequence),
                AverageDifficulty = result.Sequence.Count > 0 ?
                    (double)result.Sequence.Sum(e => e.Difficulty) / result.Sequence.Count : 0
            };

            CalculateCorrectionMetrics(metric, result);

            CalculateAdvancedMetrics(metric, result, config);

            return metric;
        }

        private static void CalculateCorrectionMetrics(RunMetric metric, SimulationResult result)
        {
            int totalCorrections = 0;
            int currentStreak = 0;
            int maxStreak = 0;

            foreach (var e in result.Sequence)
            {
                bool isCorrected = e.Difficulty != e.OriginalDifficulty || e.Reward != e.OriginalReward;

                if (isCorrected)
                {
                    totalCorrections++;
                    currentStreak++;
                }
                else
                {
                    if (currentStreak > maxStreak) maxStreak = currentStreak;
                    currentStreak = 0;
                }
            }
            if (currentStreak > maxStreak) maxStreak = currentStreak;

            metric.CorrectionCount = totalCorrections;
            metric.MaxCorrectionStreak = maxStreak;
            metric.CorrectionsPerEncounter = result.Sequence.Count > 0 ? (double)totalCorrections / result.Sequence.Count : 0;
        }

        private static void CalculateAdvancedMetrics(RunMetric metric, SimulationResult result, SimulationConfig config)
        {
            if (result.History == null || result.History.Count == 0)
            {
                metric.MinResourceRemaining = config.InitialResources;
                metric.RecoveryLag_Avg = -1;
                return;
            }

            int nearDeathThreshold = (int)(config.MaxHP * 0.2);
            int nearDeathDuration = 0;
            int nearDeathTransitions = 0;
            bool wasNearDeath = false;

            var healthDeltas = new List<double>();
            int previousHP = config.InitialHP;

            var resourceConsumptionPcts = new List<double>();
            int previousResources = config.InitialResources;
            int minResources = config.InitialResources;

            var fpiValues = new List<double>();
            int previousDifficulty = 0;

            int recoveryCriticalThreshold = (int)(config.MaxHP * 0.4);
            int recoverySafeThreshold = (int)(config.MaxHP * 0.7);
            bool inRecoveryMode = false;
            int currentRecoveryDuration = 0;
            var successfulRecoveryLags = new List<int>();
            int failedRecoveryCount = 0;

            for (int i = 0; i < result.History.Count; i++)
            {
                var state = result.History[i];
                var encounter = result.Sequence[i];

                bool isNearDeath = state.CurrentHP <= nearDeathThreshold && state.IsAlive;
                if (isNearDeath)
                {
                    nearDeathDuration++;
                    if (!wasNearDeath) nearDeathTransitions++;
                }
                wasNearDeath = isNearDeath;

                double delta = Math.Abs(state.CurrentHP - previousHP);
                healthDeltas.Add(delta);
                previousHP = state.CurrentHP;

                int preSpendResources = state.IsAlive ? previousResources + encounter.Reward : previousResources;

                int spent = preSpendResources - state.Resources;

                double pctConsumed = preSpendResources > 0
                    ? (double)spent / preSpendResources
                    : 0.0;
                resourceConsumptionPcts.Add(pctConsumed);

                if (state.Resources < minResources) minResources = state.Resources;
                previousResources = state.Resources;

                double termHealth = 1.0 - ((double)state.CurrentHP / config.MaxHP);
                termHealth = Math.Clamp(termHealth, 0.0, 1.0);

                double termResource = (double)spent / config.MaxResource;
                termResource = Math.Clamp(termResource, 0.0, 1.0);

                int diffDelta = encounter.Difficulty - previousDifficulty;
                double termSpike = Math.Max(0, diffDelta) / 9.0; 
                termSpike = Math.Clamp(termSpike, 0.0, 1.0);

                double fpi = termHealth + termResource + termSpike;
                fpiValues.Add(fpi);

                previousDifficulty = encounter.Difficulty;

                if (inRecoveryMode)
                {
                    currentRecoveryDuration++;
                    if (state.CurrentHP >= recoverySafeThreshold)
                    {
                        successfulRecoveryLags.Add(currentRecoveryDuration);
                        inRecoveryMode = false;
                        currentRecoveryDuration = 0;
                    }
                }
                else
                {
                    if (state.CurrentHP < recoveryCriticalThreshold && state.IsAlive)
                    {
                        inRecoveryMode = true;
                        currentRecoveryDuration = 0;
                    }
                }
            }

            metric.NearDeath_EncounterCount = nearDeathTransitions;
            metric.NearDeath_TotalEncounters = nearDeathDuration;
            metric.NearDeath_Entered = nearDeathDuration > 0 ? 1 : 0;

            if (healthDeltas.Count > 0)
            {
                metric.HealthDelta_Mean = healthDeltas.Average();
                double mean = metric.HealthDelta_Mean;
                double sumSquares = healthDeltas.Sum(d => Math.Pow(d - mean, 2));
                metric.HealthDelta_StdDev = Math.Sqrt(sumSquares / healthDeltas.Count);
            }

            if (resourceConsumptionPcts.Count > 0)
            {
                metric.AvgResourceConsumedPct = resourceConsumptionPcts.Average();
            }
            metric.MinResourceRemaining = minResources;

            if (fpiValues.Count > 0)
            {
                metric.FPI_Avg = fpiValues.Average();
                metric.FPI_Max = fpiValues.Max();
            }

            if (inRecoveryMode)
            {
                failedRecoveryCount++;
            }

            metric.RecoveryLag_FailedCount = failedRecoveryCount;
            metric.RecoveryLag_Avg = successfulRecoveryLags.Count > 0 ? successfulRecoveryLags.Average() : -1.0;
        }
    }
}