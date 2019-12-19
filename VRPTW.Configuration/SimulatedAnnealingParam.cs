using System;
namespace VRPTW.Configuration
{
    public class SimulatedAnnealingParam
    {
        public double InitialTemperature {get; set;}
        public double Alpha { get; set; }
        public int IterationCount { get; set; }
    }
}
