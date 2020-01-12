
namespace VRPTW.Configuration
{
    public class HeuristicsParam
    {
        public int IterationCount { get; set; }
        public InitialSolutionParam InitialSolutionParam { get; set; }
        public SimulatedAnnealingParam SimulatedAnnealingParam { get; set; }
        public TabuSearchParam TabuSearchParam { get; set; }
        public DiversificationParam DiversificationParam { get; set; }
    }
}
