
namespace VRPTW.Configuration
{
    public class SolverParameters
    {
        public string Source { get; set; }
        public double TimeLimit { get; set; }
        public double MIPGap { get; set; }
        public int Threads { get; set; }
    }
}
