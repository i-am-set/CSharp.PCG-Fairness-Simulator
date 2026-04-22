using System.Collections.Generic;
using System.Linq;
using Core.Simulation;

namespace Core.Metrics
{
    public class BatchMetrics
    {
        public string GroupName { get; set; }
        public int TotalRuns { get; set; }
        public double SurvivalRate { get; set; }
        public double MeanSurvivalLength { get; set; }
        public double MeanEntropy { get; set; }
        public double MeanDeltaVariance { get; set; }

        public Dictionary<DeathClassification, double> DeathDistribution { get; set; }

        public double AverageCorrectionsPerRun { get; set; }
        public double MeanCorrectionsPerEncounter { get; set; }
        public double MeanMaxCorrectionStreak { get; set; }

        public double MeanNearDeathCount { get; set; }
        public double MeanNearDeathDuration { get; set; }
        public double NearDeathRate { get; set; }
        public double MeanHealthVolatility { get; set; }
        public double MeanResourceConsumption { get; set; }
        public double MeanMinResources { get; set; }

        public double MeanFPI_Avg { get; set; }
        public double MeanFPI_Max { get; set; }
        public double MeanRecoveryLag { get; set; }
        public double MeanRecoveryFailures { get; set; }

        public BatchMetrics(string groupName, List<RunMetric> runs)
        {
            GroupName = groupName;
            TotalRuns = runs.Count;

            if (TotalRuns == 0)
            {
                DeathDistribution = new Dictionary<DeathClassification, double>();
                return;
            }

            SurvivalRate = (double)runs.Count(r => r.Survived) / TotalRuns;
            MeanSurvivalLength = runs.Average(r => r.SurvivalLength);
            MeanEntropy = runs.Average(r => r.EntropyScore);
            MeanDeltaVariance = runs.Average(r => r.DeltaVariance);

            AverageCorrectionsPerRun = runs.Average(r => r.CorrectionCount);
            MeanCorrectionsPerEncounter = runs.Average(r => r.CorrectionsPerEncounter);
            MeanMaxCorrectionStreak = runs.Average(r => r.MaxCorrectionStreak);

            MeanNearDeathCount = runs.Average(r => r.NearDeath_EncounterCount);
            MeanNearDeathDuration = runs.Average(r => r.NearDeath_TotalEncounters);
            NearDeathRate = (double)runs.Count(r => r.NearDeath_Entered == 1) / TotalRuns;
            MeanHealthVolatility = runs.Average(r => r.HealthDelta_StdDev);
            MeanResourceConsumption = runs.Average(r => r.AvgResourceConsumedPct);
            MeanMinResources = runs.Average(r => r.MinResourceRemaining);

            MeanFPI_Avg = runs.Average(r => r.FPI_Avg);
            MeanFPI_Max = runs.Average(r => r.FPI_Max);

            var validRecoveries = runs.Where(r => r.RecoveryLag_Avg >= 0).ToList();
            MeanRecoveryLag = validRecoveries.Count > 0
                ? validRecoveries.Average(r => r.RecoveryLag_Avg)
                : -1.0;

            MeanRecoveryFailures = runs.Average(r => r.RecoveryLag_FailedCount);

            var failedRuns = runs.Where(r => !r.Survived).ToList();
            int failCount = failedRuns.Count;

            DeathDistribution = new Dictionary<DeathClassification, double>
            {
                { DeathClassification.Spike, 0 },
                { DeathClassification.Attrition, 0 },
                { DeathClassification.Starvation, 0 },
                { DeathClassification.None, SurvivalRate }
            };

            if (failCount > 0)
            {
                DeathDistribution[DeathClassification.Spike] = (double)failedRuns.Count(r => r.DeathCause == DeathClassification.Spike) / TotalRuns;
                DeathDistribution[DeathClassification.Attrition] = (double)failedRuns.Count(r => r.DeathCause == DeathClassification.Attrition) / TotalRuns;
                DeathDistribution[DeathClassification.Starvation] = (double)failedRuns.Count(r => r.DeathCause == DeathClassification.Starvation) / TotalRuns;
            }
        }
    }
}