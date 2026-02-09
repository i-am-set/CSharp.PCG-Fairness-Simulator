using System;
using System.Collections.Generic;
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

        public SimulationController()
        {
            _runner = new SimulationRunner();
            _runner.OnProgressChanged += p => Progress = p;
            _runner.OnStatusChanged += s => StatusMessage = s;

            Config = new SimulationConfig();
        }

        public async void StartSimulation()
        {
            if (IsRunning) return;

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
            RawResults = null;
            BatchSummaries = null;
            _cts = new CancellationTokenSource();

            Logger.Log($"Starting Simulation. Runs: {Config.RunCount}, Seed: {Config.MasterSeed}");
            Logger.Log($"Config: {Config.ToJson()}");

            try
            {
                RawResults = await _runner.RunSimulationAsync(Config, _cts.Token);

                BatchSummaries = new List<BatchMetrics>();
                var allRuns = new List<RunMetric>();

                foreach (var kvp in RawResults)
                {
                    BatchSummaries.Add(new BatchMetrics(kvp.Key, kvp.Value));
                    allRuns.AddRange(kvp.Value);
                }

                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

                string summaryCsv = CsvExporter.ExportBatchSummaryToCsv(BatchSummaries);
                string summaryFilename = $"Results_Summary_{timestamp}.csv";
                System.IO.File.WriteAllText(summaryFilename, summaryCsv);

                string rawCsv = CsvExporter.ExportRunsToCsv(allRuns);
                string rawFilename = $"Results_RawData_{timestamp}.csv";
                System.IO.File.WriteAllText(rawFilename, rawCsv);

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

        public void SaveConfig(string name)
        {
            ConfigManager.SavePreset(Config, name);
            StatusMessage = $"Saved Config: {name}";
        }

        public void LoadConfig(string name)
        {
            if (IsRunning) return;
            Config = ConfigManager.LoadPreset(name);
            StatusMessage = $"Loaded Config: {name}";
        }
    }
}