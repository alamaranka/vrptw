using System;
namespace VRPTW.Entity
{
    public enum SolverType
    {
        None,
        GurobiSolver,
        BendersSolver,
        LocalSearch,
        SimulatedAnnealing
    }
}
