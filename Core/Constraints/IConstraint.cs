using System.Collections.Generic;
using Core.Simulation;

namespace Core.Constraints
{

    public interface IConstraint
    {
        Encounter Apply(Encounter current, IReadOnlyList<Encounter> history, PlayerState state, SimulationConfig config);
    }
}