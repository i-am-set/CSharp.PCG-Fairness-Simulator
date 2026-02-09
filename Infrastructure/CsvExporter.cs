using System.Collections.Generic;
using System.Text;
using Core.Metrics;

namespace Core.Infrastructure
{
    public static class CsvExporter
    {
        /// <summary>
        /// Generates a CSV string from a list of run metrics.
        /// </summary>
        public static string ExportRunsToCsv(List<RunMetric> runs)
        {
            var sb = new StringBuilder();

            // Header
            sb.AppendLine("RunID,Seed,ConstraintMode,Survived,SurvivalLength,DeathCause,FinalHP,FinalResources,EntropyScore,DeltaVariance,AvgDifficulty,TotalCorrections,CorrectionsPerEncounter,MaxCorrectionStreak,NearDeath_EncounterCount,NearDeath_TotalEncounters,NearDeath_Entered,HealthDelta_Mean,HealthDelta_StdDev,AvgResourceConsumedPct,MinResourceRemaining,FPI_Avg,FPI_Max,RecoveryLag_Avg,RecoveryLag_FailedCount");

            // Rows
            foreach (var run in runs)
            {
                sb.Append(run.RunID).Append(",");
                sb.Append(run.Seed).Append(",");
                sb.Append(Escape(run.ConstraintMode)).Append(",");
                sb.Append(run.Survived).Append(",");
                sb.Append(run.SurvivalLength).Append(",");
                sb.Append(run.DeathCause).Append(",");
                sb.Append(run.FinalHP).Append(",");
                sb.Append(run.FinalResources).Append(",");
                sb.Append(run.EntropyScore.ToString("F4")).Append(",");
                sb.Append(run.DeltaVariance.ToString("F4")).Append(",");
                sb.Append(run.AverageDifficulty.ToString("F4")).Append(",");

                // Corrections
                sb.Append(run.CorrectionCount).Append(",");
                sb.Append(run.CorrectionsPerEncounter.ToString("F4")).Append(",");
                sb.Append(run.MaxCorrectionStreak).Append(",");

                // Near Death & Volatility
                sb.Append(run.NearDeath_EncounterCount).Append(",");
                sb.Append(run.NearDeath_TotalEncounters).Append(",");
                sb.Append(run.NearDeath_Entered).Append(",");
                sb.Append(run.HealthDelta_Mean.ToString("F2")).Append(",");
                sb.Append(run.HealthDelta_StdDev.ToString("F2")).Append(",");
                sb.Append(run.AvgResourceConsumedPct.ToString("F4")).Append(",");
                sb.Append(run.MinResourceRemaining).Append(",");

                // FPI & Recovery
                sb.Append(run.FPI_Avg.ToString("F4")).Append(",");
                sb.Append(run.FPI_Max.ToString("F4")).Append(",");
                sb.Append(run.RecoveryLag_Avg.ToString("F2")).Append(",");
                sb.Append(run.RecoveryLag_FailedCount);

                sb.AppendLine();
            }

            return sb.ToString();
        }

        /// <summary>
        /// Generates a CSV string for batch summary metrics.
        /// </summary>
        public static string ExportBatchSummaryToCsv(List<BatchMetrics> batches)
        {
            var sb = new StringBuilder();

            // Header
            sb.AppendLine("GroupName,TotalRuns,SurvivalRate,MeanSurvivalLength,MeanEntropy,MeanDeltaVariance,SpikeDeathRate,AttritionDeathRate,StarvationDeathRate,AvgCorrections,MeanCorrectionsPerEncounter,MeanMaxCorrectionStreak,MeanNearDeathCount,MeanNearDeathDuration,NearDeathRate,MeanHealthVolatility,MeanResourceConsumption,MeanMinResources,MeanFPI_Avg,MeanFPI_Max,MeanRecoveryLag,MeanRecoveryFailures");

            foreach (var batch in batches)
            {
                sb.Append(Escape(batch.GroupName)).Append(",");
                sb.Append(batch.TotalRuns).Append(",");
                sb.Append(batch.SurvivalRate.ToString("F4")).Append(",");
                sb.Append(batch.MeanSurvivalLength.ToString("F2")).Append(",");
                sb.Append(batch.MeanEntropy.ToString("F4")).Append(",");
                sb.Append(batch.MeanDeltaVariance.ToString("F4")).Append(",");

                double spike = batch.DeathDistribution.ContainsKey(Simulation.DeathClassification.Spike) ? batch.DeathDistribution[Simulation.DeathClassification.Spike] : 0;
                double attrition = batch.DeathDistribution.ContainsKey(Simulation.DeathClassification.Attrition) ? batch.DeathDistribution[Simulation.DeathClassification.Attrition] : 0;
                double starvation = batch.DeathDistribution.ContainsKey(Simulation.DeathClassification.Starvation) ? batch.DeathDistribution[Simulation.DeathClassification.Starvation] : 0;

                sb.Append(spike.ToString("F4")).Append(",");
                sb.Append(attrition.ToString("F4")).Append(",");
                sb.Append(starvation.ToString("F4")).Append(",");

                // Corrections
                sb.Append(batch.AverageCorrectionsPerRun.ToString("F2")).Append(",");
                sb.Append(batch.MeanCorrectionsPerEncounter.ToString("F4")).Append(",");
                sb.Append(batch.MeanMaxCorrectionStreak.ToString("F2")).Append(",");

                // Advanced Metrics
                sb.Append(batch.MeanNearDeathCount.ToString("F2")).Append(",");
                sb.Append(batch.MeanNearDeathDuration.ToString("F2")).Append(",");
                sb.Append(batch.NearDeathRate.ToString("F4")).Append(",");
                sb.Append(batch.MeanHealthVolatility.ToString("F2")).Append(",");
                sb.Append(batch.MeanResourceConsumption.ToString("F4")).Append(",");
                sb.Append(batch.MeanMinResources.ToString("F2")).Append(",");

                // FPI & Recovery
                sb.Append(batch.MeanFPI_Avg.ToString("F4")).Append(",");
                sb.Append(batch.MeanFPI_Max.ToString("F4")).Append(",");
                sb.Append(batch.MeanRecoveryLag.ToString("F2")).Append(",");
                sb.Append(batch.MeanRecoveryFailures.ToString("F2"));

                sb.AppendLine();
            }

            return sb.ToString();
        }

        private static string Escape(string input)
        {
            if (input.Contains(",") || input.Contains("\"") || input.Contains("\n"))
            {
                return $"\"{input.Replace("\"", "\"\"")}\"";
            }
            return input;
        }
    }
}