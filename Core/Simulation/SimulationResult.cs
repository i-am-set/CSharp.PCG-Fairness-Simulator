using System.Collections.Generic;

namespace Core.Simulation
{
    public class SimulationResult
    {
        public int Seed { get; set; }
        public bool Survived { get; set; }
        public int FinalHP { get; set; }
        public int FinalResources { get; set; }
        public int StepsSurvived { get; set; }
        public DeathClassification DeathCause { get; set; }
        public List<Encounter> Sequence { get; set; } = new List<Encounter>();

        public List<PlayerState> History { get; set; } = new List<PlayerState>();

        public double EntropyScore { get; set; }
        public double DeltaVariance { get; set; }
    }
}