using System;
using System.Collections.Generic;
using System.Linq;
using Core.Simulation;

namespace Core.Metrics
{
    public static class EntropyCalculator
    {
        public static double CalculateShannonEntropy(List<Encounter> sequence)
        {
            if (sequence == null || sequence.Count == 0) return 0.0;

            var frequencies = new Dictionary<int, int>();
            foreach (var encounter in sequence)
            {
                if (!frequencies.ContainsKey(encounter.Difficulty))
                    frequencies[encounter.Difficulty] = 0;
                frequencies[encounter.Difficulty]++;
            }

            double entropy = 0.0;
            int total = sequence.Count;

            foreach (var count in frequencies.Values)
            {
                double p = (double)count / total;
                entropy -= p * Math.Log(p, 2);
            }

            return entropy;
        }

        public static double CalculateDeltaVariance(List<Encounter> sequence)
        {
            if (sequence == null || sequence.Count < 2) return 0.0;

            var deltas = new List<double>();
            for (int i = 1; i < sequence.Count; i++)
            {
                deltas.Add(sequence[i].Difficulty - sequence[i - 1].Difficulty);
            }

            double meanDelta = deltas.Average();
            double sumSquares = deltas.Sum(d => Math.Pow(d - meanDelta, 2));

            return sumSquares / deltas.Count;
        }
    }
}