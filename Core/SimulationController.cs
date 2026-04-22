using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Core.Infrastructure;
using Core.Metrics;
using Core.Simulation;

namespace Core
{
    public class SimulationController
    {
        private readonly SimulationRunner _runner;
        private CancellationTokenSource _cts;

        public SimulationConfig Config { get; private set; }
        public Dictionary<string, List<RunMetric>> RawResults { get; private set; }
        public List<BatchMetrics> BatchSummaries { get; private set; }

        public bool IsRunning { get; private set; }
        public double Progress { get; private set; }
        public string StatusMessage { get; private set; } = "Ready";
        public string OutputDirectory { get; private set; }

        public bool UseRandomMasterSeed { get; set; } = true;

        public SimulationController()
        {
            _runner = new SimulationRunner();
            _runner.OnProgressChanged += p => Progress = p;
            _runner.OnStatusChanged += s => StatusMessage = s;

            Config = new SimulationConfig();

            OutputDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SimulationResults");
            if (!Directory.Exists(OutputDirectory))
            {
                Directory.CreateDirectory(OutputDirectory);
            }
        }

        public async void StartSimulation()
        {
            if (IsRunning) return;

            if (UseRandomMasterSeed)
            {
                Config.MasterSeed = new Random().Next();
            }

            try
            {
                Config.Validate();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Config Error: {ex.Message}";
                Logger.LogError("Validation failed", ex);
                return;
            }

            IsRunning = true;
            Progress = 0;
            _cts = new CancellationTokenSource();

            Logger.Log($"Starting Simulation. Runs: {Config.RunCount}, Seed: {Config.MasterSeed}");
            Logger.Log($"Config: {Config.ToJson()}");

            try
            {
                var newRawResults = await _runner.RunSimulationAsync(Config, _cts.Token);

                var newBatchSummaries = new List<BatchMetrics>();
                var allRuns = new List<RunMetric>();

                foreach (var kvp in newRawResults)
                {
                    newBatchSummaries.Add(new BatchMetrics(kvp.Key, kvp.Value));
                    allRuns.AddRange(kvp.Value);
                }

                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

                string summaryCsv = CsvExporter.ExportBatchSummaryToCsv(newBatchSummaries);
                string summaryFilename = Path.Combine(OutputDirectory, $"Results_Summary_{timestamp}.csv");
                File.WriteAllText(summaryFilename, summaryCsv);

                string rawCsv = CsvExporter.ExportRunsToCsv(allRuns);
                string rawFilename = Path.Combine(OutputDirectory, $"Results_RawData_{timestamp}.csv");
                File.WriteAllText(rawFilename, rawCsv);

                RawResults = newRawResults;
                BatchSummaries = newBatchSummaries;

                Logger.Log($"Simulation Complete. Saved {summaryFilename} and {rawFilename}");
            }
            catch (OperationCanceledException)
            {
                StatusMessage = "Cancelled";
                Logger.Log("Simulation Cancelled by User.");
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
                Logger.LogError("Simulation Failed", ex);
            }
            finally
            {
                IsRunning = false;
                _cts = null;
            }
        }

        public void Cancel()
        {
            if (IsRunning)
            {
                _cts?.Cancel();
                Logger.Log("Cancellation requested...");
            }
        }

        public void ClearResults()
        {
            RawResults = null;
            BatchSummaries = null;
            Progress = 0;
            StatusMessage = "Ready";
        }

        public void OpenOutputFolder()
        {
            if (Directory.Exists(OutputDirectory))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = OutputDirectory,
                    UseShellExecute = true,
                    Verb = "open"
                });
            }
        }
    }
}